using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts = Helios.Bridge.Contracts;

namespace Helios.Bridge.Host.Instruments.Simulated
{
  // A fake instrument backend that needs no hardware, license, Helios.dll, or IAPI DLL. It
  // exercises the full Fusion-tier surface (syringe pump, analog trace, PAGC scan flags) on a
  // timer, so the whole gRPC pipeline -- including Helios.Client.Demo on the .NET 8 side -- can be
  // built and run on any dev machine.
  public sealed class SimulatedInstrumentGateway : IInstrumentGateway
  {
    private readonly object _sync = new();
    private readonly Random _random = new();
    private readonly SimulatedOptions _options;

    private PrecisionPeriodicClock? _scanClock;
    private Timer? _analogTimer;
    private bool _disposed;
    private long _scanCounter;
    private readonly System.Diagnostics.Stopwatch _sinceConnect = new();

    public SimulatedInstrumentGateway(SimulatedOptions? options = null)
    {
      _options = options ?? new SimulatedOptions();
      Acquisition = new SimulatedAcquisitionControl();
      Scans = new SimulatedScanControl();
      SyringePump = new SimulatedSyringePumpChannel();
      _msScanChannel = new SimulatedMsScanChannel(DetectorClasses[0]);
      _analogTraceChannel = new SimulatedAnalogTraceChannel(DetectorClasses.Length > 1 ? DetectorClasses[1] : "Analog 1");
    }

    public string InstrumentFamily => "Simulated";

    public bool ServiceConnected { get; private set; }
    public bool InstrumentConnected { get; private set; }

    public int InstrumentId => 1;
    public string InstrumentName => "Simulated Orbitrap";
    public string[] DetectorClasses { get; } = { "Orbitrap", "UV" };
    public int CountMsDetectors => 1;
    public int CountAnalogChannels => 1;
    public bool HasSyringePump => true;

    public IAcquisitionControl Acquisition { get; }
    public IScanControl Scans { get; }
    public ISyringePumpChannel? SyringePump { get; }

    private readonly SimulatedMsScanChannel _msScanChannel;
    private readonly SimulatedAnalogTraceChannel _analogTraceChannel;

    public event EventHandler<EventArgs>? ServiceConnectionChanged;
    public event EventHandler<EventArgs>? InstrumentConnectionChanged;
    public event EventHandler<MessagesArrivedEventArgs>? MessagesArrived;
#pragma warning disable CS0067 // required by IInstrumentGateway; the simulated instrument has no contact closure hardware to raise this from.
    public event EventHandler<ContactClosureChangedEventArgs>? ContactClosureChanged;
#pragma warning restore CS0067
    public event EventHandler<NumOpenCustomScanSlotsEventArgs>? NumOpenCustomScanSlotsReceived;

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
      if (ServiceConnected) return; // idempotent -- a second call must not spin up a second scan clock.

      await Task.Delay(_options.ConnectDelay, cancellationToken).ConfigureAwait(false);

      ServiceConnected = true;
      ServiceConnectionChanged?.Invoke(this, EventArgs.Empty);

      InstrumentConnected = true;
      InstrumentConnectionChanged?.Invoke(this, EventArgs.Empty);

      MessagesArrived?.Invoke(this, new MessagesArrivedEventArgs(new[]
      {
        new InstrumentMessage
        {
          InstrumentId = InstrumentId,
          InstrumentName = InstrumentName,
          MessageId = 1,
          CreationTimeUtc = DateTime.UtcNow,
          Status = 2,
          Message = "Simulated instrument online.",
          MessageArgs = Array.Empty<string>(),
        },
      }));

      HighResolutionTimerPeriod.Begin();

      _sinceConnect.Restart();
      _scanClock = new PrecisionPeriodicClock(_options.ScanInterval, EmitScan);
      _analogTimer = new Timer(_ => _analogTraceChannel.Emit(NextDouble(0, 100)), null, TimeSpan.Zero, _options.AnalogInterval);
    }

    public IMsScanChannel GetMsScanContainer(int msDetectorSet) => _msScanChannel;

    public IAnalogTraceChannel? GetAnalogTraceContainer(int analogDetectorSet) =>
      analogDetectorSet == 0 ? _analogTraceChannel : null;

    public IReadOnlyDictionary<string, string> GetInstrumentValues(IReadOnlyList<string> names)
    {
      var all = new Dictionary<string, string>
      {
        ["Ion Gauge Pressure"] = NextDouble(1e-9, 5e-9).ToString("E3"),
        ["Source Temperature"] = NextDouble(250, 300).ToString("F1"),
      };
      if (names.Count == 0) return all;

      var result = new Dictionary<string, string>();
      for (int i = 0; i < names.Count; i++)
      {
        if (all.TryGetValue(names[i], out var value)) result[names[i]] = value;
      }
      return result;
    }

    public void RequestNumOpenCustomScanSlots() =>
      NumOpenCustomScanSlotsReceived?.Invoke(this, new NumOpenCustomScanSlotsEventArgs { NumOpenCustomScanSlots = 5 });

    public void Dispose()
    {
      if (_disposed) return;
      _disposed = true;
      _scanClock?.Dispose();
      _analogTimer?.Dispose();
      if (ServiceConnected) HighResolutionTimerPeriod.End();
    }

    private void EmitScan()
    {
      int peakCount = _options.CentroidsPerScan;
      var mz = new double[peakCount];
      var intensity = new double[peakCount];
      var charge = new int[peakCount];
      var resolution = new double[peakCount];
      var exceptional = new bool[peakCount];
      var fragmented = new bool[peakCount];
      var merged = new bool[peakCount];
      var referenced = new bool[peakCount];

      lock (_sync)
      {
        double baseMz = 300 + _random.NextDouble() * 1600;
        for (int i = 0; i < peakCount; i++)
        {
          mz[i] = baseMz + i * 0.5 + _random.NextDouble() * 0.05;
          intensity[i] = _random.NextDouble() * 1_000_000;
          charge[i] = _random.Next(1, 4);
          resolution[i] = 120_000;
        }
      }

      long scanNumber = Interlocked.Increment(ref _scanCounter);
      double firstMass = mz.Length > 0 ? mz[0] : 0;
      double lastMass = mz.Length > 0 ? mz[mz.Length - 1] : 0;
      double basePeakIntensity = 0;
      for (int i = 0; i < intensity.Length; i++)
      {
        if (intensity[i] > basePeakIntensity) basePeakIntensity = intensity[i];
      }

      var centroids = new Contracts.CentroidBlock();
      centroids.Mz.Add(mz);
      centroids.Intensity.Add(intensity);
      centroids.Charge.Add(charge);
      centroids.Resolution.Add(resolution);
      centroids.IsExceptional.Add(exceptional);
      centroids.IsFragmented.Add(fragmented);
      centroids.IsMerged.Add(merged);
      centroids.IsReferenced.Add(referenced);

      var scan = new Contracts.MsScanData
      {
        DetectorName = DetectorClasses[0],
        ArrivalTimeUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        HasProfileInformation = false,
        Centroids = centroids,
      };

      // Header keys use the same canonical (Helios "universal dictionary") spellings
      // HeliosMsScanChannelAdapter.CanonicalHeaderIds resolves on real Fusion/Exploris hardware --
      // this backend has no raw instrument-specific spelling to translate from, so it can just
      // emit the canonical form directly.
      scan.Header["Scan"] = scanNumber.ToString();
      scan.Header["MSOrder"] = "1";
      scan.Header["StartTime"] = _sinceConnect.Elapsed.TotalMinutes.ToString("F4");
      scan.Header["FirstMass"] = firstMass.ToString("F4");
      scan.Header["LastMass"] = lastMass.ToString("F4");
      scan.Header["BasePeakIntensity"] = basePeakIntensity.ToString("F1");
      scan.Header["ScanData"] = "Centroid";
      scan.Header["MassAnalyzer"] = "FTMS";
      scan.Header["Polarity"] = "Positive";
      scan.Header["ScanMode"] = "Full";
      scan.Header["IonizationMode"] = "NSI";
      scan.Header["InjectTime"] = "50";
      scan.Header["TIC"] = (basePeakIntensity * peakCount * 0.4).ToString("F1");
      scan.Trailer["Access ID"] = scanNumber.ToString();

      _msScanChannel.Emit(scan);
    }

    private double NextDouble(double min, double max)
    {
      lock (_sync) return min + _random.NextDouble() * (max - min);
    }
  }

  public sealed class SimulatedOptions
  {
    public TimeSpan ConnectDelay { get; init; } = TimeSpan.FromMilliseconds(200);
    public TimeSpan ScanInterval { get; init; } = TimeSpan.FromMilliseconds(200);
    public TimeSpan AnalogInterval { get; init; } = TimeSpan.FromSeconds(1);
    public int CentroidsPerScan { get; init; } = 400;
  }

  internal sealed class SimulatedMsScanChannel : IMsScanChannel
  {
    private Contracts.MsScanData? _lastScan;

    public SimulatedMsScanChannel(string detectorClass) => DetectorClass = detectorClass;

    public string DetectorClass { get; }

    public event EventHandler<MsScanEventArgs>? MsScanArrived;

    public Contracts.MsScanData? GetLastMsScan() => _lastScan;

    public void Emit(Contracts.MsScanData scan)
    {
      _lastScan = scan;
      MsScanArrived?.Invoke(this, new MsScanEventArgs(scan));
    }
  }

  internal sealed class SimulatedAcquisitionControl : IAcquisitionControl
  {
    public AcquisitionStateSnapshot State { get; private set; } =
      new() { Mode = SystemMode.Standby, State = InstrumentState.ReadyForRun };

    public event EventHandler<AcquisitionStateChangedEventArgs>? StateChanged;
    public event EventHandler<AcquisitionStreamOpeningEventArgs>? AcquisitionStreamOpening;
    public event EventHandler? AcquisitionStreamClosing;

    public void SetMode(bool on)
    {
      State = new AcquisitionStateSnapshot
      {
        Mode = on ? SystemMode.On : SystemMode.Standby,
        State = on ? InstrumentState.Running : InstrumentState.ReadyForRun,
      };
      StateChanged?.Invoke(this, new AcquisitionStateChangedEventArgs(State));
    }

    public void StartAcquisition(AcquisitionWorkflowRequest request)
    {
      AcquisitionStreamOpening?.Invoke(this, new AcquisitionStreamOpeningEventArgs
      {
        StartingInformation = new Dictionary<string, string>
        {
          ["RawFile"] = request.RawFileName,
          ["Comment"] = request.Comment,
        },
      });
      State = new AcquisitionStateSnapshot { Mode = SystemMode.On, State = InstrumentState.Running };
      StateChanged?.Invoke(this, new AcquisitionStateChangedEventArgs(State));
    }

    public void CancelAcquisition()
    {
      AcquisitionStreamClosing?.Invoke(this, EventArgs.Empty);
      State = new AcquisitionStateSnapshot { Mode = SystemMode.Standby, State = InstrumentState.ReadyForRun };
      StateChanged?.Invoke(this, new AcquisitionStateChangedEventArgs(State));
    }
  }

  internal sealed class SimulatedScanControl : IScanControl
  {
    public ScanParameterDescriptor[] PossibleParameters { get; } =
    {
      new() { Name = "FirstMass", Selection = "string (50;2000)", DefaultValue = "300", Help = "First mass of the scan range." },
      new() { Name = "LastMass", Selection = "string (50;2000)", DefaultValue = "1500", Help = "Last mass of the scan range." },
      new() { Name = "ScanType", Selection = "Full,SIM,MSn", DefaultValue = "Full", Help = "The type of scan to perform." },
      new() { Name = "Analyzer", Selection = "IonTrap,Orbitrap", DefaultValue = "Orbitrap", Help = "The mass analyzer." },
      new() { Name = "OrbitrapResolution", Selection = "7500,15000,30000,60000,120000,240000", DefaultValue = "120000", Help = "The Orbitrap resolution." },
      new() { Name = "AGCTarget", Selection = "string", DefaultValue = "100000", Help = "The AGC target." },
    };

    public event EventHandler? CanAcceptNextCustomScan;
#pragma warning disable CS0067 // required by IScanControl; PossibleParameters is fixed for the simulated instrument, so this never changes.
    public event EventHandler<EventArgs>? PossibleParametersChanged;
#pragma warning restore CS0067

    public bool SubmitCustomScan(CustomScan scan)
    {
      CanAcceptNextCustomScan?.Invoke(this, EventArgs.Empty);
      return true;
    }

    public bool SubmitRepeatingScan(RepeatingScan scan) => true;

    public bool CancelCustomScan() => true;

    public bool CancelRepetition() => true;
  }

  internal sealed class SimulatedSyringePumpChannel : ISyringePumpChannel
  {
    public double Diameter { get; private set; } = 4.61;
    public double Volume { get; private set; } = 250;
    public double FlowRate { get; private set; } = 5;
    public SyringePumpStatus Status { get; private set; } = SyringePumpStatus.Off;

    public event EventHandler? StatusChanged;
    public event EventHandler? ParameterValueChanged;

    public void Start()
    {
      Status = SyringePumpStatus.On;
      StatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
      Status = SyringePumpStatus.Off;
      StatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Toggle()
    {
      if (Status == SyringePumpStatus.On) Stop();
      else Start();
    }

    public void SetDiameter(double diameter)
    {
      Diameter = diameter;
      ParameterValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetVolume(double volume)
    {
      Volume = volume;
      ParameterValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetFlowRate(double flowRate)
    {
      FlowRate = flowRate;
      ParameterValueChanged?.Invoke(this, EventArgs.Empty);
    }
  }

  internal sealed class SimulatedAnalogTraceChannel : IAnalogTraceChannel
  {
    private readonly System.Diagnostics.Stopwatch _sinceStart = System.Diagnostics.Stopwatch.StartNew();

    public SimulatedAnalogTraceChannel(string detectorClass) =>
      Info = new AnalogTraceInfo { DetectorClass = detectorClass, Minimum = 0, Maximum = 1000, UpdateFrequencyHz = 1 };

    public AnalogTraceInfo Info { get; }

    public event EventHandler<AnalogTracePointEventArgs>? AnalogTracePointArrived;

    public void Emit(double value) =>
      AnalogTracePointArrived?.Invoke(this, new AnalogTracePointEventArgs { Value = value, Occurrence = _sinceStart.Elapsed });
  }
}
