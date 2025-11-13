using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition.Workflow
{
  public interface IAcquisitionLimitedByTime:IAcquisitionWorkflow
  {
    //
    // Summary:
    //     Access to the duration of the acquisition. The lower limit is 0.01 min and the
    //     maximum 15000 min.
    TimeSpan Duration { get; set; }
  }

  internal class FusionAcquisitionLimitedByTime: Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionLimitedByTime
  {
    public TimeSpan Duration { get; set; }
    public string RawFileName { get; set; }
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation Continuation { get; set; }
    public string SampleName { get; set; }
    public string Comment { get; set; }
    public double SingleProcessingDelay { get; set; }
    public bool WaitForContactClosure { get; set; }
    public FusionAcquisitionLimitedByTime(IAcquisitionLimitedByTime workflow)
    {
      Duration = workflow.Duration;
      RawFileName = workflow.RawFileName;
      Continuation = (Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation)workflow.Continuation;
      SampleName = workflow.SampleName;
      Comment = workflow.Comment;
      SingleProcessingDelay = workflow.SingleProcessingDelay;
      WaitForContactClosure = workflow.WaitForContactClosure;
    }
  }

  internal class HeliosAcquisitionLimitedByTime : IAcquisitionLimitedByTime
  {
    public TimeSpan Duration { get; set; }
    public string RawFileName { get; set; }
    public AcquisitionContinuation Continuation { get; set; }
    public string SampleName { get; set; }
    public string Comment { get; set; }
    public double SingleProcessingDelay { get; set; }
    public bool WaitForContactClosure { get; set; }

    public HeliosAcquisitionLimitedByTime()
    {

    }

    public HeliosAcquisitionLimitedByTime(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionLimitedByTime m)
    {
      Duration = m.Duration;
      RawFileName = m.RawFileName;
      Continuation = (AcquisitionContinuation)m.Continuation;
      SampleName = m.SampleName;
      Comment = m.Comment;
      SingleProcessingDelay = m.SingleProcessingDelay;
      WaitForContactClosure = m.WaitForContactClosure;
    }
  }

}
