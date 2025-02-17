extern alias fusion;
extern alias exploris;

using UIAPI.Interfaces.InstrumentAccess.Control.Acquisition;
using UIAPI.Interfaces.InstrumentAccess.Control.Scans;
using UIAPI.Interfaces.InstrumentAccess.Control.InstrumentValues;

namespace UIAPI.Interfaces.InstrumentAccess.Control
{
  /// <summary>
  /// This interface wraps the extensions to IControl by Exploris and Fusion based instruments.
  /// </summary>    
  public interface IUControl
  {

    /// <summary>
    /// Get access to the scans interface. This interface allows to execute repeating and custum scans during a method or any other execution.
    ///
    /// Calling this method will lock that interface until it is disposed.
    /// The instrument will not stop its current operation for the first.
    /// If exclusive access is requested it will be guaranteed that no other Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IScans interface is in use and that any further request for such an interface no matter whether exclusive or cooperative will be rejected.
    /// If non-exclusive access is requested it will not be satisfied if an exclusive access is already granted.
    /// </summary>
    /// <param name="exclusiveAccess">: request for exclusive access(true) or cooperative access (false)</param>
    /// <returns>IUScans interface being capable to receive user commands.</returns>
    IUScans GetScans(bool exclusiveAccess);

    /// <summary>
    /// Get access to the acquisition interface.
    /// This property is the instrument-specific implementation for Fusion and Exploris-based instruments.
    /// </summary>  
    IUAcquisition Acquisition { get; }

    IUInstrumentValues InstrumentValues { get; }
  }

  internal class UControlExploris : IUControl
  {
    exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl control;
    public UIAPI.Interfaces.InstrumentAccess.Control.Acquisition.IUAcquisition Acquisition { get; }
    public IUInstrumentValues InstrumentValues { get; }
    public UControlExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccess ia)
    {
      control = ia.Control;
      Acquisition = new UAcquisitionExploris(control);
      InstrumentValues = new UInstrumentValues(control);
    }
    public UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans GetScans(bool exclusiveAccess)
    {
      return new UIAPI.Interfaces.InstrumentAccess.Control.Scans.UScansExploris(control,exclusiveAccess);
    }
  }

  internal class UControlFusion : IUControl
  {
    fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl control;
    public IUAcquisition Acquisition { get; }
    public IUInstrumentValues InstrumentValues { get; }
    public UControlFusion(fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccess ia)
    {
      control = ia.Control;
      Acquisition = new UAcquisitionFusion(control);
      InstrumentValues = new UInstrumentValues(control);
    }

    public UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans GetScans(bool exclusiveAccess)
    {
      return new UIAPI.Interfaces.InstrumentAccess.Control.Scans.UScansFusion(control, exclusiveAccess);
    }
  }

  internal class UControlVMS : IUControl
  {
    public IUAcquisition Acquisition { get; } = new UAcquisitionVMS();
    public IUInstrumentValues InstrumentValues { get; } = new UInstrumentValues();

    public UIAPI.Interfaces.InstrumentAccess.Control.Scans.IUScans GetScans(bool exclusiveAccess)
    {
      return new UIAPI.Interfaces.InstrumentAccess.Control.Scans.UScansVMS();
    }
  }

}
