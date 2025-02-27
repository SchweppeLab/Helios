extern alias fusion;
extern alias exploris;

using Helios.Interfaces.InstrumentAccess.Control.Acquisition;
using Helios.Interfaces.InstrumentAccess.Control.Scans;
using Helios.Interfaces.InstrumentAccess.Control.InstrumentValues;
using Pipes;

namespace Helios.Interfaces.InstrumentAccess.Control
{
  /// <summary>
  /// This interface wraps the extensions to IControl by Exploris and Fusion based instruments.
  /// </summary>    
  public interface IHeliosControl
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
    IHeliosScans GetScans(bool exclusiveAccess);

    /// <summary>
    /// Get access to the acquisition interface.
    /// This property is the instrument-specific implementation for Fusion and Exploris-based instruments.
    /// </summary>  
    IHeliosAcquisition Acquisition { get; }

    IHeliosInstrumentValues InstrumentValues { get; }
  }

  internal class HeliosControlExploris : IHeliosControl
  {
    exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl control;
    public IHeliosAcquisition Acquisition { get; }
    public IHeliosInstrumentValues InstrumentValues { get; }
    public HeliosControlExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccess ia)
    {
      control = ia.Control;
      Acquisition = new HeliosAcquisitionExploris(control);
      InstrumentValues = new HeliosInstrumentValues(control);
    }
    public IHeliosScans GetScans(bool exclusiveAccess)
    {
      return new HeliosScansExploris(control,exclusiveAccess);
    }
  }

  internal class HeliosControlFusion : IHeliosControl
  {
    fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl control;
    public IHeliosAcquisition Acquisition { get; }
    public IHeliosInstrumentValues InstrumentValues { get; }
    public HeliosControlFusion(fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccess ia)
    {
      control = ia.Control;
      Acquisition = new HeliosAcquisitionFusion(control);
      InstrumentValues = new HeliosInstrumentValues(control);
    }

    public IHeliosScans GetScans(bool exclusiveAccess)
    {
      return new HeliosScansFusion(control, exclusiveAccess);
    }
  }

  internal class HeliosControlVMS : IHeliosControl
  {
    public IHeliosAcquisition Acquisition { get; } = new HeliosAcquisitionVMS();
    public IHeliosInstrumentValues InstrumentValues { get; } = new HeliosInstrumentValues();

    private readonly PipesClient pipesClient = null;

    public HeliosControlVMS(PipesClient pc = null)
    {
      pipesClient = pc;

    }

    public IHeliosScans GetScans(bool exclusiveAccess)
    {
      return new HeliosScansVMS(pipesClient);
    }
  }

}
