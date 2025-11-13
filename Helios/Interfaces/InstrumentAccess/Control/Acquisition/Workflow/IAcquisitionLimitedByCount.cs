using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition.Workflow
{
  public interface IAcquisitionLimitedByCount : IAcquisitionWorkflow
  {
    int NumberOfScans { get; set; }
  }

  internal class FusionAcquisitionLimitedByCount : Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionLimitedByCount
  {
    public int NumberOfScans { get; set; }
    public string RawFileName { get; set; }
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation Continuation { get; set; }
    public string SampleName { get; set; }
    public string Comment { get; set; }
    public double SingleProcessingDelay { get; set; }
    public bool WaitForContactClosure { get; set; }
    public FusionAcquisitionLimitedByCount(IAcquisitionLimitedByCount workflow)
    {
      NumberOfScans = workflow.NumberOfScans;
      RawFileName = workflow.RawFileName;
      Continuation = (Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation)workflow.Continuation;
      SampleName = workflow.SampleName;
      Comment = workflow.Comment;
      SingleProcessingDelay = workflow.SingleProcessingDelay;
      WaitForContactClosure = workflow.WaitForContactClosure;
    }
  }

  internal class HeliosAcquisitionLimitedByCount : IAcquisitionLimitedByCount
  {
    public int NumberOfScans { get; set; }
    public string RawFileName { get; set; }
    public AcquisitionContinuation Continuation { get; set; }
    public string SampleName { get; set; }
    public string Comment { get; set; }
    public double SingleProcessingDelay { get; set; }
    public bool WaitForContactClosure { get; set; }

    public HeliosAcquisitionLimitedByCount()
    {

    }
    public HeliosAcquisitionLimitedByCount(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionLimitedByCount m)
    {
      NumberOfScans = m.NumberOfScans;
      RawFileName = m.RawFileName;
      Continuation = (AcquisitionContinuation)m.Continuation;
      SampleName = m.SampleName;
      Comment = m.Comment;
      SingleProcessingDelay = m.SingleProcessingDelay;
      WaitForContactClosure = m.WaitForContactClosure;
    }
  }
}
