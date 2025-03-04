extern alias fusion;
extern alias exploris;

using Helios.Interfaces.InstrumentAccess.Control.Acquisition;
using Helios.Interfaces.InstrumentAccess.Control.Scans;
using Helios.Interfaces.InstrumentAccess.Control.InstrumentValues;
using Helios.Interfaces.InstrumentAccess.Control.Peripherals;
using Pipes;

namespace Helios.Interfaces.InstrumentAccess.Control
{
  /// <summary>
  /// This interface wraps the extensions to IControl by Exploris and Fusion based instruments.
  /// </summary>    
  public interface IControl
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
    IScans GetScans(bool exclusiveAccess);

    /// <summary>
    /// Get access to the acquisition interface.
    /// This property is the instrument-specific implementation for Fusion and Exploris-based instruments.
    /// </summary>  
    IAcquisition Acquisition { get; }

    IInstrumentValues InstrumentValues { get; }

    /// <summary>
    /// Only the Tribrid IAPI has access to this. Therefore it can be null when connected to an Exploris.
    /// </summary>
    ISyringePumpControl SyringePumpControl { get; }
  }

  internal class HeliosControlExploris : IControl
  {
    exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl control;
    public IAcquisition Acquisition { get; }
    public IInstrumentValues InstrumentValues { get; }
    public ISyringePumpControl SyringePumpControl { get; } = null;

    public HeliosControlExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccess ia)
    {
      control = ia.Control;
      Acquisition = new HeliosAcquisitionExploris(control);
      InstrumentValues = new HeliosInstrumentValues(control);
    }
    public IScans GetScans(bool exclusiveAccess)
    {
      return new HeliosScansExploris(control,exclusiveAccess);
    }
  }

  internal class HeliosControlFusion : IControl
  {
    fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl control;
    public IAcquisition Acquisition { get; }
    public IInstrumentValues InstrumentValues { get; }
    public ISyringePumpControl SyringePumpControl { get; }
    public HeliosControlFusion(fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccess ia)
    {
      control = ia.Control;
      Acquisition = new HeliosAcquisitionFusion(control);
      InstrumentValues = new HeliosInstrumentValues(control);
      SyringePumpControl = new HeliosSyringePumpControlFusion(control);
    }

    public IScans GetScans(bool exclusiveAccess)
    {
      return new HeliosScansFusion(control, exclusiveAccess);
    }
  }

  internal class HeliosControlVMS : IControl
  {
    public IAcquisition Acquisition { get; } = new HeliosAcquisitionVMS();
    public IInstrumentValues InstrumentValues { get; } = new HeliosInstrumentValues();
    public ISyringePumpControl SyringePumpControl { get; } = null;

    private readonly PipesClient pipesClient = null;

    public HeliosControlVMS(PipesClient pc = null)
    {
      pipesClient = pc;

    }

    public IScans GetScans(bool exclusiveAccess)
    {
      return new HeliosScansVMS(pipesClient);
    }
  }

}
