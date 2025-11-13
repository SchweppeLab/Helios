extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Interfaces.InstrumentAccess.Control.Acquisition.Workflow
{
  public interface IAcquisitionWorkflow
  {
    //
    // Summary:
    //     Access to the raw file name to be used during an acquisition. Setting this value
    //     to null lets the instrument take acquisitions without storing in a separate file.
    //
    //
    //     An evaluation of this value will happen on submission to Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition.StartAcquisition(Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionWorkflow).
    string RawFileName { get; set; }

    //
    // Summary:
    //     This property defines what shappens when the acquisition stops.
    AcquisitionContinuation Continuation { get; set; }

    //
    // Summary:
    //     This property defines the arbitrary sample name.
    string SampleName { get; set; }

    //
    // Summary:
    //     This property defines an arbitrary comment.
    string Comment { get; set; }

    //
    // Summary:
    //     The instrument will not execute any scan if this property is positive until the
    //     delay has expired or a new custom scan has been defined.
    //
    //     It will be quaranteed that after the event Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.IAcquisition.AcquisitionStreamOpening
    //     no Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer.IMsScanContainer.MsScanArrived
    //     will be populated or written to the rawfile if this delay is pending and no custom
    //     scan has been placed.
    //
    //     The unit of this property is seconds and possible values are between 0 and 600
    //     inclusively. The default value is 0.
    double SingleProcessingDelay { get; set; }

    //
    // Summary:
    //     This flag decides whether the instruments waits for contact closure signal before
    //     starting acquisition.
    bool WaitForContactClosure { get; set; }
  }

  internal class FusionAcquisitionWorkflow : Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.IAcquisitionWorkflow
  {
    public string RawFileName { get; set; }
    public Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation Continuation { get; set; }
    public string SampleName { get; set; }
    public string Comment { get; set; }
    public double SingleProcessingDelay { get; set; }
    public bool WaitForContactClosure { get; set; }
    public FusionAcquisitionWorkflow(IAcquisitionWorkflow workflow)
    {
      RawFileName = workflow.RawFileName;
      Continuation = (Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Workflow.AcquisitionContinuation)workflow.Continuation;
      SampleName = workflow.SampleName;
      Comment = workflow.Comment;
      SingleProcessingDelay = workflow.SingleProcessingDelay;
      WaitForContactClosure = workflow.WaitForContactClosure;
    }
  }

  internal class HeliosAcquisitionWorkflow : IAcquisitionWorkflow
  {
    public string RawFileName { get; set; }
    public AcquisitionContinuation Continuation { get; set; }
    public string SampleName { get; set; }
    public string Comment { get; set; }
    public double SingleProcessingDelay { get; set; }
    public bool WaitForContactClosure { get; set; }

    public HeliosAcquisitionWorkflow()
    {

    }
  }
}
