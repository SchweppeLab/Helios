using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition.Workflow
{
  public interface IAcquisitionMethodRun : IAcquisitionWorkflow
  {
    //
    // Summary:
    //     Access to the method name to be used during an acquisition.
    string MethodName { get; set; }

    //
    // Summary:
    //     Access to the duration of the acquisition. Setting this to null choses the duration
    //     chosen in the method itself. The lower limit is 0.01 min and the maximum 15000
    //     min.
    TimeSpan? Duration { get; set; }
  }

  internal class FusionAcquisitionMethodRun : Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionMethodRun
  {
    public string MethodName { get; set; }
    public TimeSpan? Duration { get; set; }
    public string RawFileName { get; set; }
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation Continuation { get; set; }
    public string SampleName { get; set; }
    public string Comment { get; set; }
    public double SingleProcessingDelay { get; set; }
    public bool WaitForContactClosure { get; set; }
    public FusionAcquisitionMethodRun(IAcquisitionMethodRun workflow)
    {
      MethodName = workflow.MethodName;
      Duration = workflow.Duration;
      RawFileName = workflow.RawFileName;
      Continuation = (Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation)workflow.Continuation;
      SampleName = workflow.SampleName;
      Comment = workflow.Comment;
      SingleProcessingDelay = workflow.SingleProcessingDelay;
      WaitForContactClosure = workflow.WaitForContactClosure;
    }
  }

  internal class HeliosAcquisitionMethodRun : IAcquisitionMethodRun
  {
    public string MethodName { get; set; }
    public TimeSpan? Duration { get; set; }
    public string RawFileName { get; set; }
    public AcquisitionContinuation Continuation { get; set; }
    public string SampleName { get; set; }
    public string Comment { get; set; }
    public double SingleProcessingDelay { get; set; }
    public bool WaitForContactClosure { get; set; }

    public HeliosAcquisitionMethodRun()
    {

    }

    public HeliosAcquisitionMethodRun(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionMethodRun m)
    {
      MethodName = m.MethodName;
      Duration = m.Duration;
      RawFileName = m.RawFileName;
      Continuation = (AcquisitionContinuation)m.Continuation;
      SampleName = m.SampleName;
      Comment= m.Comment;
      SingleProcessingDelay= m.SingleProcessingDelay;
      WaitForContactClosure= m.WaitForContactClosure;
    }
  }
}
