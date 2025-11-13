extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.AnalogTraceContainer
{
  public interface IAnalogTraceContainer
  {
    //
    // Summary:
    //     Get access to the detector class.
    //
    //     Example: The detector name may be "PDA", but "Analog Input Channel 1" or something
    //     similar is also possible.
    string DetectorClass { get; }

    //
    // Summary:
    //     Get access to the lowest possible value of the detector or analog input.
    double Minimum { get; }

    //
    // Summary:
    //     Get access to the highest possible value of the detector or analog input.
    double Maximum { get; }

    //
    // Summary:
    //     Get access to acquisition frequency of the values. null is returned if the system
    //     has no specific frequency. The value is returned in 1/s (Hertz).
    double? UpdateFrequencyHz { get; }

    //
    // Summary:
    //     Get access to the last value seen in the system. The value can be null initially.
    IAnalogTracePoint LastValue { get; }

    //
    // Summary:
    //     This event will be fired when a new analog value has been emitted by the instrument.
    //     There may be no specific update frequency.
    event EventHandler<AnalogTracePointEventArgs> AnalogTracePointArrived;
  }

  internal class HeliosAnalogTraceContainerExploris : IAnalogTraceContainer
  {
    private exploris.Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.IAnalogTraceContainer atc;
    public string DetectorClass => atc.DetectorClass;
    public double Minimum =>atc.Minimum;
    public double Maximum => atc.Maximum;
    public double? UpdateFrequencyHz => atc.UpdateFrequencyHz;
    public IAnalogTracePoint LastValue { get; private set; } = null;
    public event EventHandler<AnalogTracePointEventArgs> AnalogTracePointArrived;
    public HeliosAnalogTraceContainerExploris(exploris.Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccess instAcc, int analogDetectorSet)
    {
      atc = instAcc.GetAnalogTraceContainer(analogDetectorSet);
      if(atc.LastValue!=null) LastValue = new HeliosAnalogTracePoint(atc.LastValue);
      atc.AnalogTracePointArrived += OnAnalogTracePointArrived;
    }

    protected virtual void OnAnalogTracePointArrived(object sender, exploris.Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.AnalogTracePointEventArgs e)
    {
      LastValue = new HeliosAnalogTracePoint(e.TracePoint);
      AnalogTracePointArrived?.Invoke(this, new AnalogTrancePointEventArgsExploris(e));
    }
  }

  internal class HeliosAnalogTraceContainerFusion : IAnalogTraceContainer
  {
    private Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.IAnalogTraceContainer atc;
    public string DetectorClass => atc.DetectorClass;
    public double Minimum => atc.Minimum;
    public double Maximum => atc.Maximum;
    public double? UpdateFrequencyHz => atc.UpdateFrequencyHz;
    public IAnalogTracePoint LastValue { get; private set; } = null;
    public event EventHandler<AnalogTracePointEventArgs> AnalogTracePointArrived;
    public HeliosAnalogTraceContainerFusion(Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccess instAcc, int analogDetectorSet)
    {
      atc = instAcc.GetAnalogTraceContainer(analogDetectorSet);
      if (atc.LastValue != null) LastValue = new HeliosAnalogTracePoint(atc.LastValue);
      atc.AnalogTracePointArrived += OnAnalogTracePointArrived;
    }

    protected virtual void OnAnalogTracePointArrived(object sender, Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer.AnalogTracePointEventArgs e)
    {
      LastValue = new HeliosAnalogTracePoint(e.TracePoint);
      AnalogTracePointArrived?.Invoke(this, new AnalogTrancePointEventArgsFusion(e));
    }
  }

  internal class HeliosAnalogTraceContainerVMS : IAnalogTraceContainer
  {
    public string DetectorClass { get; }
    public double Minimum { get; }
    public double Maximum { get; }
    public double? UpdateFrequencyHz { get; }
    public IAnalogTracePoint LastValue { get; private set; } = null;
    public event EventHandler<AnalogTracePointEventArgs> AnalogTracePointArrived;
    public HeliosAnalogTraceContainerVMS()
    {
      //atc = instAcc.GetAnalogTraceContainer(analogDetectorSet);
      //LastValue = new HeliosAnalogTracePoint(atc.LastValue);
      //atc.AnalogTracePointArrived += OnAnalogTracePointArrived;
    }

    //protected virtual void OnAnalogTracePointArrived(object sender, AnalogTracePointEventArgs e)
    //{
    //  LastValue = new HeliosAnalogTracePoint(e.TracePoint);
    //  AnalogTracePointArrived?.Invoke(this, e);
    //}
  }
}
