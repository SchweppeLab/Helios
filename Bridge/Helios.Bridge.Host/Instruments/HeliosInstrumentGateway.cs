using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Contracts = Helios.Bridge.Contracts;
using Helios.Interfaces;
using Helios.Interfaces.InstrumentAccess;
using Helios.Interfaces.InstrumentAccess.AnalogTraceContainer;
using Helios.Interfaces.InstrumentAccess.Control;
using Helios.Interfaces.InstrumentAccess.Control.Acquisition;
using Helios.Interfaces.InstrumentAccess.Control.Acquisition.Workflow;
using Helios.Interfaces.InstrumentAccess.Control.Peripherals;
using Helios.Interfaces.InstrumentAccess.Control.Scans;
using Helios.Interfaces.InstrumentAccess.MsScanContainer;
using Helios.Interfaces.SpectrumFormat;

namespace Helios.Bridge.Host.Instruments
{
  // Every Helios/IAPI event subscription in this file runs synchronously on whatever thread
  // raised it -- for Fusion/Exploris that's an IAPI callback thread, for VMS it's Corona's own
  // pipe-read thread. An unhandled exception there doesn't just fail that one callback: for VMS
  // specifically it kills Corona's pipe dispatch loop outright, silently ending all further
  // scan/acquisition-event delivery for the rest of the connection (confirmed empirically -- a
  // StatusLog/TuneData null-reference in the scan mapper took out Corona's dispatch entirely,
  // including every AcquisitionStreamOpening/Closing after the first scan, not just that scan).
  // Every subscription below goes through this so a bug in one mapping can't take the whole
  // connection down again.
  internal static class CallbackGuard
  {
    public static void Run(string what, Action action)
    {
      try
      {
        action();
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Helios.Bridge.Host: '{what}' callback threw and was swallowed to protect the instrument/Corona connection: {ex}");
      }
    }
  }

  // Wraps Helios.dll's own IAPI wrapper (IInstrumentAccessContainer / IInstrumentAccess /
  // IControl / ...) instead of re-implementing IAPI access a second time. Helios's public
  // interfaces already unify Fusion and Exploris -- e.g. SystemMode/InstrumentState are single
  // enums, not one per family -- so one adapter suffices here where a from-raw-IAPI approach would
  // have needed two (one per family).
  //
  // Takes an already-probed IInstrumentAccessContainer rather than calling
  // InstrumentAccessContainerFactory.Create() itself: Program.cs does that probe once at host
  // startup (Fusion, then Exploris, then VMS/Corona -- Helios's own auto-probe order, unchanged),
  // so it can fall back to SimulatedInstrumentGateway when nothing real answers, restoring the
  // same "try real hardware/Corona first, synthetic data as a last resort" behavior the original
  // in-process ScanSpy got for free from Helios.dll. Helios's per-family container classes are
  // `internal` with no InternalsVisibleTo, so there's no way to force a specific family from
  // outside Helios.dll -- not a problem here since Create() already picks whichever's actually
  // reachable.
  public sealed class HeliosInstrumentGateway : IInstrumentGateway
  {
    private readonly IInstrumentAccessContainer _container;
    private IInstrumentAccess? _access;
    private IScans? _scans;
    private int _connected; // 0/1 guard: IInstrumentGateway.ConnectAsync must be safe to call once only.

    private readonly HeliosMsScanChannelAdapter[] _msScanChannels = new HeliosMsScanChannelAdapter[1];
    private HeliosAcquisitionControlAdapter? _acquisitionAdapter;
    private HeliosScanControlAdapter? _scanControlAdapter;
    private HeliosSyringePumpAdapter? _syringePumpAdapter;
    private readonly Dictionary<int, HeliosAnalogTraceAdapter> _analogTraceAdapters = new();

    // Set in the constructor from the already-probed container's runtime type -- see the
    // Program.cs comment on why probing happens before this gateway is even constructed, not
    // inside ConnectAsync.
    public string InstrumentFamily { get; }

    public HeliosInstrumentGateway(IInstrumentAccessContainer container)
    {
      _container = container ?? throw new ArgumentNullException(nameof(container));

      // Container concrete types are internal to Helios.dll -- GetType().Name still works via
      // reflection and is more reliable than InstrumentType() (documented in Helios as
      // diagnostics-only and "likely to be removed at some point").
      string typeName = _container.GetType().Name;
      InstrumentFamily = typeName.Contains("Fusion") ? "Fusion"
        : typeName.Contains("Exploris") ? "Exploris"
        : typeName.Contains("VMS") ? "VMS"
        : "Unknown";
    }

    public bool ServiceConnected => _container.ServiceConnected;
    public bool InstrumentConnected => _access?.Connected ?? false;
    public int InstrumentId => _access?.InstrumentId ?? 0;
    public string InstrumentName => _access?.InstrumentName ?? string.Empty;
    public string[] DetectorClasses => _access?.DetectorClasses ?? Array.Empty<string>();
    public int CountMsDetectors => _access?.CountMsDetectors ?? 0;
    public int CountAnalogChannels => _access?.CountAnalogChannels ?? 0;
    public bool HasSyringePump => _access?.Control.SyringePumpControl != null;

    public event EventHandler<EventArgs>? ServiceConnectionChanged;
    public event EventHandler<EventArgs>? InstrumentConnectionChanged;
    public event EventHandler<MessagesArrivedEventArgs>? MessagesArrived;
    public event EventHandler<ContactClosureChangedEventArgs>? ContactClosureChanged;
#pragma warning disable CS0067 // Helios's IScans has no "open custom scan slots" concept today -- see RequestNumOpenCustomScanSlots below.
    public event EventHandler<NumOpenCustomScanSlotsEventArgs>? NumOpenCustomScanSlotsReceived;
#pragma warning restore CS0067

    public IAcquisitionControl Acquisition => _acquisitionAdapter ?? throw NotConnected();
    public IScanControl Scans => _scanControlAdapter ?? throw NotConnected();
    public ISyringePumpChannel? SyringePump => _syringePumpAdapter;

    // Real Fusion/Exploris hardware brings the service online asynchronously after
    // StartOnlineAccess() -- unlike Simulated/VMS, which have no such handshake at all, so this
    // race never surfaced against either of them. Get(1) called before ServiceConnectionChanged
    // actually reports ServiceConnected == true throws (or hands back an unusable
    // IInstrumentAccess) on real hardware; the original in-process pattern always waited for
    // exactly this before calling Get(1) (see CLAUDE.md: Create() -> ServiceConnectionChanged ->
    // Get(1)) -- this mirrors that instead of assuming StartOnlineAccess() completes synchronously.
    private static readonly TimeSpan ServiceConnectTimeout = TimeSpan.FromSeconds(15);

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
      if (Interlocked.CompareExchange(ref _connected, 1, 0) != 0) return;

      try
      {
        await WaitForServiceConnectedAsync(cancellationToken).ConfigureAwait(false);

        // Only wired up after the wait succeeds -- no gRPC client can be listening yet at this
        // point anyway (Connect hasn't returned), so there's nothing lost by not forwarding the
        // very first ServiceConnectionChanged transition; delaying this also means a failed
        // connect attempt doesn't leave a stale subscription behind for the retry below to double up.
        _container.MessagesArrived += (s, e) => CallbackGuard.Run("MessagesArrived", () => MessagesArrived?.Invoke(this, ToHost(e)));
        _container.ServiceConnectionChanged += (s, e) => CallbackGuard.Run("ServiceConnectionChanged", () => ServiceConnectionChanged?.Invoke(this, e));

        _access = _container.Get(1);
        _access.ConnectionChanged += (s, e) => CallbackGuard.Run("ConnectionChanged", () => InstrumentConnectionChanged?.Invoke(this, e));
        _access.ContactClosureChanged += (s, e) => CallbackGuard.Run("ContactClosureChanged", () =>
          ContactClosureChanged?.Invoke(this, new ContactClosureChangedEventArgs { RisingEdges = e.RisingEdges, FallingEdges = e.FallingEdges }));

        _acquisitionAdapter = new HeliosAcquisitionControlAdapter(_access.Control.Acquisition);
        _scans = _access.Control.GetScans(exclusiveAccess: false);
        _scanControlAdapter = new HeliosScanControlAdapter(_scans);
        var pump = _access.Control.SyringePumpControl;
        _syringePumpAdapter = pump is null ? null : new HeliosSyringePumpAdapter(pump);

        _msScanChannels[0] = new HeliosMsScanChannelAdapter(_access.GetMsScanContainer(0));
      }
      catch
      {
        // Let a retried Connect RPC actually retry instead of the guard above silently
        // short-circuiting every future attempt (returning as if already connected while _access
        // stays null) once the first one has failed.
        _connected = 0;
        throw;
      }
    }

    private async Task WaitForServiceConnectedAsync(CancellationToken cancellationToken)
    {
      var serviceConnectedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
      void OnServiceConnectionChanged(object? s, EventArgs e)
      {
        if (_container.ServiceConnected) serviceConnectedTcs.TrySetResult(true);
      }

      _container.ServiceConnectionChanged += OnServiceConnectionChanged;
      try
      {
        _container.StartOnlineAccess();
        if (_container.ServiceConnected) return; // already online -- StartOnlineAccess completed synchronously.

        using var timeoutCts = new CancellationTokenSource(ServiceConnectTimeout);
        using var callerRegistration = cancellationToken.Register(() => serviceConnectedTcs.TrySetCanceled(cancellationToken));
        using var timeoutRegistration = timeoutCts.Token.Register(() =>
          serviceConnectedTcs.TrySetException(new TimeoutException(
            $"Timed out after {ServiceConnectTimeout.TotalSeconds:F0}s waiting for the instrument service to come online (ServiceConnectionChanged never reported ServiceConnected == true).")));

        await serviceConnectedTcs.Task.ConfigureAwait(false);
      }
      finally
      {
        _container.ServiceConnectionChanged -= OnServiceConnectionChanged;
      }
    }

    public IMsScanChannel GetMsScanContainer(int msDetectorSet)
    {
      if (msDetectorSet == 0 && _msScanChannels[0] is not null) return _msScanChannels[0];
      // Additional detector sets beyond 0 aren't cached; wrap on demand (rare -- CountMsDetectors
      // is 1 for every Fusion/Exploris configuration seen so far).
      return new HeliosMsScanChannelAdapter((_access ?? throw NotConnected()).GetMsScanContainer(msDetectorSet));
    }

    public IAnalogTraceChannel? GetAnalogTraceContainer(int analogDetectorSet)
    {
      if (_analogTraceAdapters.TryGetValue(analogDetectorSet, out var cached)) return cached;
      var raw = (_access ?? throw NotConnected()).GetAnalogTraceContainer(analogDetectorSet);
      if (raw is null) return null;
      var adapter = new HeliosAnalogTraceAdapter(raw);
      _analogTraceAdapters[analogDetectorSet] = adapter;
      return adapter;
    }

    // Helios.Interfaces.InstrumentAccess.Control.InstrumentValues.HeliosInstrumentValues.Get(...)
    // is currently stubbed in Helios.dll (returns null for both the by-name and by-number
    // overloads, for both Fusion and Exploris) -- there is no instrument value to read yet via
    // Helios's public surface. This always returns empty until that's implemented upstream;
    // reported, not silently worked around further than this.
    public IReadOnlyDictionary<string, string> GetInstrumentValues(IReadOnlyList<string> names) =>
      new Dictionary<string, string>();

    // Helios's IScans has no equivalent of "open custom scan slots" (a PAGC/queue-depth concept)
    // today -- no-op until/unless that surfaces in Helios.dll.
    public void RequestNumOpenCustomScanSlots()
    {
    }

    public void Dispose()
    {
      _scans?.Dispose();
      _container.Dispose();
    }

    private static InvalidOperationException NotConnected() =>
      new("HeliosInstrumentGateway.ConnectAsync must complete before this member is used.");

    private static MessagesArrivedEventArgs ToHost(Helios.Interfaces.InstrumentAccess.MessagesArrivedEventArgs e)
    {
      var messages = new InstrumentMessage[e.Messages.Count];
      for (int i = 0; i < e.Messages.Count; i++)
      {
        var m = e.Messages[i];
        messages[i] = new InstrumentMessage
        {
          InstrumentId = m.InstrumentId,
          InstrumentName = m.InstrumentName ?? string.Empty,
          MessageId = m.MessageId,
          CreationTimeUtc = m.CreationTime,
          Status = m.Status,
          Message = m.Message ?? string.Empty,
          MessageArgs = m.MessageArgs ?? Array.Empty<string>(),
        };
      }
      return new MessagesArrivedEventArgs(messages);
    }
  }

  internal sealed class HeliosAcquisitionControlAdapter : IAcquisitionControl
  {
    private readonly IAcquisition _acquisition;

    public HeliosAcquisitionControlAdapter(IAcquisition acquisition)
    {
      _acquisition = acquisition;
      _acquisition.StateChanged += (s, e) => CallbackGuard.Run("Acquisition.StateChanged", () =>
        StateChanged?.Invoke(this, new AcquisitionStateChangedEventArgs(ToHost(e.State))));
      _acquisition.AcquisitionStreamOpening += (s, e) => CallbackGuard.Run("Acquisition.AcquisitionStreamOpening", () =>
        AcquisitionStreamOpening?.Invoke(this, new AcquisitionStreamOpeningEventArgs { StartingInformation = ToReadOnly(e.StartingInformation) }));
      _acquisition.AcquisitionStreamClosing += (s, e) => CallbackGuard.Run("Acquisition.AcquisitionStreamClosing", () =>
        AcquisitionStreamClosing?.Invoke(this, EventArgs.Empty));
    }

    public AcquisitionStateSnapshot State => ToHost(_acquisition.State);

    public event EventHandler<AcquisitionStateChangedEventArgs>? StateChanged;
    public event EventHandler<AcquisitionStreamOpeningEventArgs>? AcquisitionStreamOpening;
    public event EventHandler? AcquisitionStreamClosing;

    public void SetMode(bool on) =>
      _acquisition.SetMode(on ? (Helios.Interfaces.InstrumentAccess.Control.Acquisition.Modes.IHeliosMode)_acquisition.CreateOnMode() : _acquisition.CreateOffMode());

    public void StartAcquisition(AcquisitionWorkflowRequest request)
    {
      IAcquisitionWorkflow workflow = request.Kind switch
      {
        WorkflowKind.CountLimited => _acquisition.CreateAcquisitionLimitedByCount(request.ScanCount),
        WorkflowKind.DurationLimited => _acquisition.CreateAcquisitionLimitedByDuration(request.Duration),
        WorkflowKind.Method => _acquisition.CreateMethodAcquisition(request.MethodFileName),
        _ => _acquisition.CreatePermanentAcquisition(),
      };
      workflow.RawFileName = request.RawFileName;
      workflow.Comment = request.Comment;
      workflow.SingleProcessingDelay = request.SingleProcessingDelay;
      workflow.WaitForContactClosure = request.WaitForContactClosure;
      _acquisition.StartAcquisition(workflow);
    }

    public void CancelAcquisition() => _acquisition.CancelAcquisition();

    private static AcquisitionStateSnapshot ToHost(IHeliosState state) =>
      new() { Mode = EnumMap<SystemMode>(state.SystemMode), State = EnumMap<InstrumentState>(state.SystemState) };

    private static T EnumMap<T>(object value) where T : struct, Enum =>
      (T)Enum.Parse(typeof(T), value.ToString() ?? string.Empty);

    private static IReadOnlyDictionary<string, string> ToReadOnly(IDictionary<string, string> source) =>
      new Dictionary<string, string>(source);
  }

  internal sealed class HeliosScanControlAdapter : IScanControl
  {
    private readonly IScans _scans;

    public HeliosScanControlAdapter(IScans scans)
    {
      _scans = scans;
      _scans.CanAcceptNextCustomScan += (s, e) => CallbackGuard.Run("Scans.CanAcceptNextCustomScan", () =>
        CanAcceptNextCustomScan?.Invoke(this, EventArgs.Empty));
      _scans.PossibleParametersChanged += (s, e) => CallbackGuard.Run("Scans.PossibleParametersChanged", () =>
        PossibleParametersChanged?.Invoke(this, EventArgs.Empty));
    }

    public ScanParameterDescriptor[] PossibleParameters
    {
      get
      {
        var source = _scans.PossibleParameters;
        var result = new ScanParameterDescriptor[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
          result[i] = new ScanParameterDescriptor
          {
            Name = source[i].Name,
            Selection = source[i].Selection,
            DefaultValue = source[i].DefaultValue,
            Help = source[i].Help,
          };
        }
        return result;
      }
    }

    public event EventHandler? CanAcceptNextCustomScan;
    public event EventHandler<EventArgs>? PossibleParametersChanged;

    public bool SubmitCustomScan(CustomScan scan)
    {
      var cs = _scans.CreateCustomScan();
      cs.RunningNumber = scan.RunningNumber;
      cs.SingleProcessingDelay = scan.SingleProcessingDelay;
      cs.IsPAGCScan = scan.IsPagcScan;
      cs.PAGCGroupIndex = scan.PagcGroupIndex;
      foreach (var kv in scan.Values) cs.Values[kv.Key] = kv.Value;
      return _scans.SetCustomScan(cs);
    }

    public bool SubmitRepeatingScan(RepeatingScan scan)
    {
      var rs = _scans.CreateRepeatingScan();
      rs.RunningNumber = scan.RunningNumber;
      foreach (var kv in scan.Values) rs.Values[kv.Key] = kv.Value;
      return _scans.SetRepetitionScan(rs);
    }

    public bool CancelCustomScan() => _scans.CancelCustomScan();

    public bool CancelRepetition() => _scans.CancelRepetition();
  }

  // Builds Contracts.MsScanData (the wire message) directly from raw IAPI data -- no intermediate
  // host-local snapshot/CentroidBlock DTO in between. That intermediate type used to exist purely
  // so IInstrumentGateway's abstraction stayed proto-free, but it meant every scan's centroid
  // arrays and four dictionaries were built once into the DTO and then copied again into the proto
  // message in Services/ScanStreamServiceImpl -- a second full pass over the highest-frequency data
  // in the system, on top of the cost already paid once here. Deliberately traded that clean
  // layering away for this one hot path (see MsScanEventArgs in Models.cs) once the centroid-flag
  // exception storm below was fixed and real Fusion throughput was still a stated concern; see
  // HISTORY.md's Core8Speed entry.
  internal sealed class HeliosMsScanChannelAdapter : IMsScanChannel
  {
    private readonly IMsScanContainer _container;
    private Contracts.MsScanData? _lastScan;

    public HeliosMsScanChannelAdapter(IMsScanContainer container)
    {
      _container = container;
      DetectorClass = container.DetectorClass;
      _container.MsScanArrived += (s, e) => CallbackGuard.Run("MsScanContainer.MsScanArrived", () =>
      {
        using var scan = e.GetScan();
        _lastScan = ToProto(scan, DetectorClass);
        MsScanArrived?.Invoke(this, new MsScanEventArgs(_lastScan));
      });
    }

    public string DetectorClass { get; }

    public event EventHandler<MsScanEventArgs>? MsScanArrived;

    public Contracts.MsScanData? GetLastMsScan()
    {
      if (_lastScan is not null) return _lastScan;
      using var scan = _container.GetLastMsScan();
      return scan is null ? null : ToProto(scan, DetectorClass);
    }

    // HeliosMsScanVMS (Corona) never assigns DetectorName either -- same gap as StatusLog/TuneData
    // above, same reason (its constructors just don't set it). A null here isn't cosmetic: the
    // proto layer rejects a null string field outright (MsScanData.DetectorName threw
    // ArgumentNullException, taking the scan out the same way the StatusLog null once did before
    // CallbackGuard existed). Falls back to the already-known detector class name rather than an
    // empty string, since that's more useful and we have it on hand regardless.
    // Helios.dll's generic Centroid implementation (used by Fusion and VMS/Corona -- Exploris has
    // its own) stubs IsExceptional/IsFragmented/IsMerged/IsReferenced with `throw new
    // NotImplementedException()`. Reading them inside a per-peak try/catch (as this used to do)
    // is not just a marshal cost -- IsExceptional throws immediately, so the other three reads
    // never even execute, meaning every peak on a real Fusion instrument threw and caught one live
    // .NET exception, which is dramatically more expensive than a normal property read or even a
    // cross-process marshal (stack capture/unwind). At real centroid counts (thousands per scan)
    // and real scan rates, this alone was enough to throttle this method below real-time -- found
    // as the dominant contributor to Helios.Bridge.Host falling behind a live Fusion instrument
    // (see HISTORY.md). One instrument family per process for this process's whole lifetime (see
    // Program.cs's CreateGateway), so probing once and caching the result -- rather than per-peak,
    // per-scan, or per-adapter -- is both correct and enough: after the very first peak of the
    // very first scan proves these are unsupported, every later peak skips the try/catch (and the
    // exception it would throw) entirely instead of paying for it thousands of times a second.
    // Shared across every HeliosMsScanChannelAdapter instance (one per detector set) rather than
    // per-instance, since they all talk to the same instrument. Deliberately unsynchronized: a
    // race during the first few overlapping calls (if multiple detector sets' callbacks land on
    // different threads before this settles) just means a handful of redundant probes, not a
    // correctness bug -- not worth a lock on this hot a path for that.
    private static bool? _centroidFlagsSupported;

    private static Contracts.MsScanData ToProto(IMsScan scan, string fallbackDetectorName)
    {
      int count = scan.CentroidCount ?? 0;

      var centroids = new Contracts.CentroidBlock();
      // Pre-sizing avoids the repeated backing-array doubling RepeatedField<T> would otherwise do
      // as each .Add() below grows it from empty -- the same benefit the old fixed-size double[]
      // arrays gave for free, without needing those arrays as a separate allocation to copy from.
      centroids.Mz.Capacity = count;
      centroids.Intensity.Capacity = count;
      centroids.Charge.Capacity = count;
      centroids.Resolution.Capacity = count;
      centroids.IsExceptional.Capacity = count;
      centroids.IsFragmented.Capacity = count;
      centroids.IsMerged.Capacity = count;
      centroids.IsReferenced.Capacity = count;

      int i = 0;
      foreach (var c in scan.Centroids)
      {
        if (i >= count) break; // CentroidCount is advisory per IAPI docs; guard against a mismatch.
        centroids.Mz.Add(c.Mz);
        centroids.Intensity.Add(c.Intensity);
        centroids.Charge.Add(c.Charge ?? -1);
        centroids.Resolution.Add(c.Resolution ?? 0);

        // Unlike the old fixed-size bool[] arrays (which defaulted every slot to false for free),
        // RepeatedField<T> only ever has as many entries as were .Add()'d -- skipping the four
        // flag adds entirely once known unsupported would leave those four fields shorter than
        // Mz/Intensity/etc., breaking the parallel-arrays wire contract. wroteFlags tracks whether
        // the try below actually added this peak's four values, so the fallback always adds
        // exactly one (false) entry per field when it didn't.
        bool wroteFlags = false;
        if (_centroidFlagsSupported != false)
        {
          try
          {
            centroids.IsExceptional.Add(c.IsExceptional ?? false);
            centroids.IsFragmented.Add(c.IsFragmented ?? false);
            centroids.IsMerged.Add(c.IsMerged ?? false);
            centroids.IsReferenced.Add(c.IsReferenced ?? false);
            _centroidFlagsSupported = true;
            wroteFlags = true;
          }
          catch (NotImplementedException)
          {
            _centroidFlagsSupported = false;
          }
        }
        if (!wroteFlags)
        {
          centroids.IsExceptional.Add(false);
          centroids.IsFragmented.Add(false);
          centroids.IsMerged.Add(false);
          centroids.IsReferenced.Add(false);
        }

        i++;
      }

      var proto = new Contracts.MsScanData
      {
        DetectorName = scan.DetectorName ?? fallbackDetectorName,
        ArrivalTimeUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        HasProfileInformation = false,
        Centroids = centroids,
      };

      CopyInto(proto.Header, scan.Header);
      ResolveCanonicalTerms(proto.Header, CanonicalHeaderIds, scan.TryHeader);
      CopyInto(proto.Trailer, scan.Trailer);
      ResolveCanonicalTerms(proto.Trailer, CanonicalTrailerIds, scan.TryTrailer);
      CopyInto(proto.StatusLog, scan.StatusLog);
      CopyInto(proto.TuneData, scan.TuneData);

      return proto;
    }

    private delegate bool TryGet(string id, out string value);

    // scan.Header/scan.Trailer hold whatever raw key spelling the connected instrument family
    // uses (e.g. Exploris spells the scan's low m/z bound "LowMass"; Fusion and this bridge's own
    // canonical name for it are both "FirstMass"). IMsScan.TryHeader/TryTrailer resolve a
    // family-independent canonical name to the right raw key via Helios's internal
    // HeliosDictionary -- not reachable directly from outside Helios.dll (internal), so the known
    // canonical ID list is reproduced here from Helios's own HeliosDictionary static constructor
    // and resolved one ID at a time instead. Consumers (like ScanSpy) that ask for "FirstMass"
    // get the right value regardless of which family answered, without needing to know Exploris
    // calls it "LowMass".
    private static readonly string[] CanonicalHeaderIds =
    {
      "Scan", "StartTime", "MassAnalyzer", "IonizationMode", "ScanRate", "ScanMode", "TIC",
      "BasePeakIntensity", "BasePeakMass", "CycleNumber", "Polarity", "Microscans", "InjectTime",
      "ScanData", "Segments", "Monoisotopic", "MasterScan", "FirstMass", "LastMass", "Checksum",
      "MSOrder", "Average", "Dependent", "MSX", "SourceFragmentation", "SourceFragmentationEnergy",
      "RawOvFtT", "Injection t0", "CollisionEnergy[0]",
    };

    private static readonly string[] CanonicalTrailerIds =
    {
      "Access ID", "Charge State", "FAIMS Voltage On", "FAIMS CV", "Master Index",
      "Master Scan Number", "Monoisotopic M/Z", "Scan Description",
    };

    // A canonical id can resolve to a "pure name" entry with a null value (see the CopyInto
    // comment below) -- skipped rather than assigned, since MapField (unlike Dictionary) throws
    // on a null value instead of just storing it.
    private static void ResolveCanonicalTerms(MapField<string, string> target, string[] canonicalIds, TryGet tryGet)
    {
      foreach (var id in canonicalIds)
      {
        if (tryGet(id, out var value) && value is not null) target[id] = value;
      }
    }

    // scan.Header is already a plain, already-materialized dictionary-like source -- straight copy,
    // except a null value (IMsScan.Header's own doc: "a pure name has a value of null") is skipped
    // rather than assigned -- MapField, unlike Dictionary, throws ArgumentNullException on a null
    // value instead of just storing it. Confirmed live on real Exploris hardware: Tribrid/Fusion
    // scans never seem to carry a null-valued pure name in practice, Exploris scans do -- see
    // HISTORY.md's 2026-08-25 entry. If skipping ever turns out to lose information a caller
    // needed, map to "" instead of skipping -- but skipping was the deliberate first choice here.
    private static void CopyInto(MapField<string, string> target, IEnumerable<KeyValuePair<string, string>> source)
    {
      foreach (var kv in source)
      {
        if (kv.Value is not null) target[kv.Key] = kv.Value;
      }
    }

    // scan.Trailer/StatusLog/TuneData are IInformationSourceAccess instead -- a live accessor, not
    // a plain dictionary. HeliosMsScanVMS (Corona) never assigns StatusLog/TuneData at all -- null
    // for every VMS scan, unlike Exploris/Fusion which always wrap a real
    // (possibly-unavailable-but-non-null) source. The null check here isn't defensive filler:
    // without it this throws on literally the first scan Corona ever sends, synchronously inside
    // Corona's own pipe-message dispatch chain (ReceiveScan -> MsScanArrived -> this method), which
    // kills that dispatch thread -- explains why no scan data arrived AND why
    // AcquisitionStreamClosing/the next StreamOpening never fired either, not just this one scan.
    //
    // TryGetValue can also return true with a null value for a real, connected Exploris instrument
    // (confirmed live -- see HISTORY.md's 2026-08-25 entry): some Trailer/StatusLog/TuneData item
    // is present by name before its value is populated. MapField throws ArgumentNullException on a
    // null value where Dictionary would just store it, and that exception was getting caught and
    // swallowed by CallbackGuard on literally every scan, so no scan data ever reached ScanSpy
    // against real Exploris hardware even though AcquisitionStreamOpening/StateChanged (a separate
    // event chain) worked fine. Skipping the null value is the deliberate first fix; if that ever
    // turns out to lose information a caller needed, map to "" instead of skipping.
    private static void CopyInto(MapField<string, string> target, IInformationSourceAccess source)
    {
      if (source is null || !source.Available) return;
      foreach (var name in source.ItemNames)
      {
        if (source.TryGetValue(name, out var value) && value is not null) target[name] = value;
      }
    }
  }

  internal sealed class HeliosSyringePumpAdapter : ISyringePumpChannel
  {
    private readonly ISyringePumpControl _pump;

    public HeliosSyringePumpAdapter(ISyringePumpControl pump)
    {
      _pump = pump;
      _pump.StatusChanged += (s, e) => CallbackGuard.Run("SyringePump.StatusChanged", () =>
        StatusChanged?.Invoke(this, EventArgs.Empty));
      _pump.ParameterValueChanged += (s, e) => CallbackGuard.Run("SyringePump.ParameterValueChanged", () =>
        ParameterValueChanged?.Invoke(this, EventArgs.Empty));
    }

    public double Diameter => _pump.Diameter;
    public double Volume => _pump.Volume;
    public double FlowRate => _pump.FlowRate;
    public SyringePumpStatus Status => (SyringePumpStatus)Enum.Parse(typeof(SyringePumpStatus), _pump.Status.ToString());

    public event EventHandler? StatusChanged;
    public event EventHandler? ParameterValueChanged;

    public void Start() => _pump.Start();
    public void Stop() => _pump.Stop();
    public void Toggle() => _pump.Toggle();
    public void SetDiameter(double diameter) => _pump.SetDiameter(diameter);
    public void SetVolume(double volume) => _pump.SetVolume(volume);
    public void SetFlowRate(double flowRate) => _pump.SetFlowRate(flowRate);
  }

  internal sealed class HeliosAnalogTraceAdapter : IAnalogTraceChannel
  {
    private readonly IAnalogTraceContainer _container;

    public HeliosAnalogTraceAdapter(IAnalogTraceContainer container)
    {
      _container = container;
      Info = new AnalogTraceInfo
      {
        DetectorClass = container.DetectorClass,
        Minimum = container.Minimum,
        Maximum = container.Maximum,
        UpdateFrequencyHz = container.UpdateFrequencyHz,
      };
      _container.AnalogTracePointArrived += (s, e) => CallbackGuard.Run("AnalogTraceContainer.AnalogTracePointArrived", () =>
        AnalogTracePointArrived?.Invoke(this, new AnalogTracePointEventArgs { Value = e.TracePoint.Value, Occurrence = e.TracePoint.Occurrence }));
    }

    public AnalogTraceInfo Info { get; }

    public event EventHandler<AnalogTracePointEventArgs>? AnalogTracePointArrived;
  }
}
