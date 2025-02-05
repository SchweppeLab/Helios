extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess.Control.Scans
{
  public interface IUScans //: exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IScans, fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionScans
  {
    //bool CancelCustomScan();
    //bool CancelRepetition();
    //IUCustomScan CreateCustomScan();
    //IURepeatingScan CreateRepeatingScan();
    //bool SetCustomScan(IUCustomScan scan);
    //bool SetRepetitionScan(IURepeatingScan scan);
    //IUParameterDescription[] PossibleParameters { get; }

    event EventHandler<EventArgs> CanAcceptNextCustomScan;
    event EventHandler<EventArgs> PossibleParametersChanged;
  }

  class UScansExploris: IUScans 
  {
    exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IScans scans;
    public event EventHandler<EventArgs> CanAcceptNextCustomScan;
    public event EventHandler<EventArgs> PossibleParametersChanged;
    public UScansExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl c, bool exclusiveAccess)
    {
      scans = c.GetScans(exclusiveAccess);
      scans.CanAcceptNextCustomScan += CanAcceptNextCustomScanExploris;
      scans.PossibleParametersChanged += PossibleParametersChangedExploris;
    }
    void CanAcceptNextCustomScanExploris(object sender, EventArgs e)
    {
      OnCanAcceptNextCustomScan(e);
    }

    protected virtual void OnCanAcceptNextCustomScan(EventArgs e)
    {
      EventHandler<EventArgs> handler = CanAcceptNextCustomScan;
      if (handler != null)
      {
        handler(this, e);
      }
    }

    void PossibleParametersChangedExploris(object sender, EventArgs e)
    {
      OnPossibleParametersChanged(e);
    }

    protected virtual void OnPossibleParametersChanged(EventArgs e)
    {
      EventHandler<EventArgs> handler = PossibleParametersChanged;
      if (handler != null)
      {
        handler(this, e);
      }
    }
  }

  class UScansFusion : IUScans
  {
    fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionScans scans;
    public event EventHandler<EventArgs> CanAcceptNextCustomScan;
    public event EventHandler<EventArgs> PossibleParametersChanged;
    public UScansFusion(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c, bool exclusiveAccess)
    {
      scans = (fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionScans)c.GetScans(exclusiveAccess);
      scans.CanAcceptNextCustomScan += CanAcceptNextCustomScanFusion;
      scans.PossibleParametersChanged += PossibleParametersChangedFusion;
    }

    void CanAcceptNextCustomScanFusion(object sender, EventArgs e)
    {
      OnCanAcceptNextCustomScan(e);
    }

    protected virtual void OnCanAcceptNextCustomScan(EventArgs e)
    {
      EventHandler<EventArgs> handler = CanAcceptNextCustomScan;
      if (handler != null)
      {
        handler(this, e);
      }
    }

    void PossibleParametersChangedFusion(object sender, EventArgs e)
    {
      OnPossibleParametersChanged(e);
    }

    protected virtual void OnPossibleParametersChanged(EventArgs e)
    {
      EventHandler<EventArgs> handler = PossibleParametersChanged;
      if (handler != null)
      {
        handler(this, e);
      }
    }
  }

  class UScansVMS : IUScans
  {
    public event EventHandler<EventArgs> CanAcceptNextCustomScan;
    public event EventHandler<EventArgs> PossibleParametersChanged;

    protected virtual void OnCanAcceptNextCustomScan(EventArgs e)
    {
      EventHandler<EventArgs> handler = CanAcceptNextCustomScan;
      if (handler != null)
      {
        handler(this, e);
      }
    }

    protected virtual void OnPossibleParametersChanged(EventArgs e)
    {
      EventHandler<EventArgs> handler = PossibleParametersChanged;
      if (handler != null)
      {
        handler(this, e);
      }
    }

  }
}
