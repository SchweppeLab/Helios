using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Bridge.Host.Instruments
{
  // The seam between the gRPC service layer and whichever backend is active: HeliosInstrumentGateway
  // (real Fusion/Exploris hardware, via Helios.dll) or SimulatedInstrumentGateway (synthetic, no
  // hardware/license needed). Shaped to flatten IAPI/Helios's live-object-with-events pattern into
  // plain snapshot DTOs + events, which is what a gRPC wire boundary needs.
  public interface IInstrumentGateway : IDisposable
  {
    string InstrumentFamily { get; }

    bool ServiceConnected { get; }
    bool InstrumentConnected { get; }

    int InstrumentId { get; }
    string InstrumentName { get; }
    string[] DetectorClasses { get; }
    int CountMsDetectors { get; }
    int CountAnalogChannels { get; }
    bool HasSyringePump { get; }

    Task ConnectAsync(CancellationToken cancellationToken);

    event EventHandler<EventArgs> ServiceConnectionChanged;
    event EventHandler<EventArgs> InstrumentConnectionChanged;
    event EventHandler<MessagesArrivedEventArgs> MessagesArrived;
    event EventHandler<ContactClosureChangedEventArgs> ContactClosureChanged;

    IAcquisitionControl Acquisition { get; }
    IScanControl Scans { get; }
    ISyringePumpChannel? SyringePump { get; }

    IMsScanChannel GetMsScanContainer(int msDetectorSet);
    IAnalogTraceChannel? GetAnalogTraceContainer(int analogDetectorSet);

    IReadOnlyDictionary<string, string> GetInstrumentValues(IReadOnlyList<string> names);

    void RequestNumOpenCustomScanSlots();
    event EventHandler<NumOpenCustomScanSlotsEventArgs> NumOpenCustomScanSlotsReceived;
  }

  public interface IAcquisitionControl
  {
    AcquisitionStateSnapshot State { get; }

    event EventHandler<AcquisitionStateChangedEventArgs> StateChanged;
    event EventHandler<AcquisitionStreamOpeningEventArgs> AcquisitionStreamOpening;
    event EventHandler AcquisitionStreamClosing;

    void SetMode(bool on);
    void StartAcquisition(AcquisitionWorkflowRequest request);
    void CancelAcquisition();
  }

  public interface IScanControl
  {
    ScanParameterDescriptor[] PossibleParameters { get; }

    event EventHandler CanAcceptNextCustomScan;
    event EventHandler<EventArgs> PossibleParametersChanged;

    bool SubmitCustomScan(CustomScan scan);
    bool SubmitRepeatingScan(RepeatingScan scan);
    bool CancelCustomScan();
    bool CancelRepetition();
  }

  public interface IMsScanChannel
  {
    string DetectorClass { get; }

    event EventHandler<MsScanEventArgs> MsScanArrived;

    MsScanSnapshot? GetLastMsScan();
  }

  public interface ISyringePumpChannel
  {
    double Diameter { get; }
    double Volume { get; }
    double FlowRate { get; }
    SyringePumpStatus Status { get; }

    event EventHandler StatusChanged;
    event EventHandler ParameterValueChanged;

    void Start();
    void Stop();
    void Toggle();
    void SetDiameter(double diameter);
    void SetVolume(double volume);
    void SetFlowRate(double flowRate);
  }

  public interface IAnalogTraceChannel
  {
    AnalogTraceInfo Info { get; }

    event EventHandler<AnalogTracePointEventArgs> AnalogTracePointArrived;
  }
}
