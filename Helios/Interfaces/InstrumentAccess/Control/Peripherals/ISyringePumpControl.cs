extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fusion::Thermo.Interfaces.FusionAccess_V1.Control.Peripherals;

namespace Helios.Interfaces.InstrumentAccess.Control.Peripherals
{
  public interface ISyringePumpControl
  {
    //
    // Summary:
    //     Get the currently set volume of the syringe in µL.
    double Volume { get; }

    //
    // Summary:
    //     Get the currently set flow rate of the pump in µL/min.
    double FlowRate { get; }

    //
    // Summary:
    //     Get the currently diameter of the syringe.
    double Diameter { get; }

    //
    // Summary:
    //     Get the current status of the syringe pump.
    SyringePumpStatus Status { get; }

    //
    // Summary:
    //     Occurs whenever the pumping state changes.
    event EventHandler StatusChanged;

    //
    // Summary:
    //     Occurs whenever a parameter value changes.
    event EventHandler ParameterValueChanged;

    //
    // Summary:
    //     Starts the pumping if connected. If it was already pumping, it applies any cached
    //     parameters and leaves it in a running state.
    void Start();

    //
    // Summary:
    //     Stops the pumping if connected. If it was already stopped, it does nothing.
    void Stop();

    //
    // Summary:
    //     Toggles the Start/Stop of the pump if connected.
    void Toggle();

    //
    // Summary:
    //     This attempts to set the volume. The next call to Start (or Toggle) will attempt
    //     to apply this value. If the pump successfully sets the value, the Volume property
    //     will be updated accordingly.
    //
    // Parameters:
    //   volume:
    //     The volume to set (µL)
    void SetVolume(double volume);

    //
    // Summary:
    //     This attempts to set the flow rate. The next call to Start (or Toggle) will attempt
    //     to apply this value. If the pump successfully sets the value, the FlowRate property
    //     will be updated accordingly.
    //
    // Parameters:
    //   flowrate:
    //     The flow rate to set (µL/min)
    void SetFlowRate(double flowrate);

    //
    // Summary:
    //     This attempts to set the syringe diameter. The next call to Start (or Toggle)
    //     will attempt to apply this value. If the pump successfully sets the value, the
    //     Diameter property will be updated accordingly.
    //
    // Parameters:
    //   diameter:
    //     The diameter to set
    void SetDiameter(double diameter);
  }

  internal class HeliosSyringePumpControlFusion : ISyringePumpControl
  {
    fusion.Thermo.Interfaces.FusionAccess_V1.Control.Peripherals.ISyringePumpControl syringePumpControl;
    public double Volume =>syringePumpControl.Volume;
    public double FlowRate => syringePumpControl.FlowRate;
    public double Diameter => syringePumpControl.Diameter;
    public SyringePumpStatus Status => (SyringePumpStatus)syringePumpControl.Status;

    public event EventHandler StatusChanged;
    public event EventHandler ParameterValueChanged;

    public HeliosSyringePumpControlFusion(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c)
    {
      syringePumpControl = c.SyringePumpControl;
      syringePumpControl.StatusChanged += OnStatusChanged;
      syringePumpControl.ParameterValueChanged += OnParameterValueChanged;
    }

    protected virtual void OnStatusChanged(object sender, EventArgs e)
    {
      StatusChanged?.Invoke(this, e);
    }

    protected virtual void OnParameterValueChanged(object sender, EventArgs e)
    {
      ParameterValueChanged?.Invoke(this, e);
    }

    public void Start()
    {
      syringePumpControl?.Start();
    }

    public void Stop()
    {
      syringePumpControl?.Stop();
    }
    public void Toggle()
    {
      syringePumpControl?.Toggle();
    }

    public void SetVolume(double volume)
    {
      syringePumpControl?.SetVolume(volume);
    }

    public void SetFlowRate(double flowRate)
    {
      syringePumpControl?.SetFlowRate(flowRate);
    }

    public void SetDiameter(double diameter)
    {
      syringePumpControl?.SetDiameter(diameter);
    }
  }
}
