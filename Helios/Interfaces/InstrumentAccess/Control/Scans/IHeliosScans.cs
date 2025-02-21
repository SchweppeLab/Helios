extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Interfaces.InstrumentAccess.Control;

namespace Helios.Interfaces.InstrumentAccess.Control.Scans
{
  public interface IHeliosScans //: exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IScans, fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionScans
  {
    bool CancelCustomScan();
    //bool CancelRepetition();
    IHeliosCustomScan CreateCustomScan();
    //IURepeatingScan CreateRepeatingScan();
    bool SetCustomScan(IHeliosCustomScan scan);
    //bool SetRepetitionScan(IURepeatingScan scan);

    /// <summary>
    /// From IAPI: All possible parameters of a scan will be listed here.
    /// </summary>
    IHeliosParameterDescription[] PossibleParameters { get; }

    event EventHandler<EventArgs> CanAcceptNextCustomScan;
    event EventHandler<EventArgs> PossibleParametersChanged;
  }

  class HeliosScansExploris: IHeliosScans 
  {
    exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IScans scans;

    public IHeliosParameterDescription[] PossibleParameters { get; }

    public event EventHandler<EventArgs> CanAcceptNextCustomScan;
    public event EventHandler<EventArgs> PossibleParametersChanged;
    public HeliosScansExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl c, bool exclusiveAccess)
    {
      scans = c.GetScans(exclusiveAccess);
      PossibleParameters = (IHeliosParameterDescription[])scans.PossibleParameters;
      scans.CanAcceptNextCustomScan += CanAcceptNextCustomScanExploris;
      scans.PossibleParametersChanged += PossibleParametersChangedExploris;
    }
    void CanAcceptNextCustomScanExploris(object sender, EventArgs e)
    {
      OnCanAcceptNextCustomScan(e);
    }

    public bool CancelCustomScan()
    {
      return scans.CancelCustomScan();
    }

    public IHeliosCustomScan CreateCustomScan()
    {
      return new HeliosCustomScan();
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

    public bool SetCustomScan(IHeliosCustomScan customScan)
    {
      return scans.SetCustomScan((exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.ICustomScan)customScan);
    }
  }

  class HeliosScansFusion : IHeliosScans
  {
    fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionScans scans;
    
    public IHeliosParameterDescription[] PossibleParameters { get; }

    public event EventHandler<EventArgs> CanAcceptNextCustomScan;
    public event EventHandler<EventArgs> PossibleParametersChanged;
    public HeliosScansFusion(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c, bool exclusiveAccess)
    {
      scans = (fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionScans)c.GetScans(exclusiveAccess);
      PossibleParameters = (IHeliosParameterDescription[])scans.PossibleParameters;
      scans.CanAcceptNextCustomScan += CanAcceptNextCustomScanFusion;
      scans.PossibleParametersChanged += PossibleParametersChangedFusion;
    }

    void CanAcceptNextCustomScanFusion(object sender, EventArgs e)
    {
      OnCanAcceptNextCustomScan(e);
    }

    public bool CancelCustomScan()
    {
      return scans.CancelCustomScan();
    }

    public IHeliosCustomScan CreateCustomScan()
    {
      return new HeliosCustomScan();
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

    public bool SetCustomScan(IHeliosCustomScan customScan)
    {
      return scans.SetCustomScan((fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionCustomScan)customScan);
    }
  }

  class HeliosScansVMS : IHeliosScans
  {

    public IHeliosParameterDescription[] PossibleParameters { get; } = new HeliosParameterDescription[1];

    public event EventHandler<EventArgs> CanAcceptNextCustomScan;
    public event EventHandler<EventArgs> PossibleParametersChanged;

    public HeliosScansVMS()
    {
      PossibleParameters[0] = new HeliosParameterDescription("ScanRate", "Selection", "Normal", "ScanRate Help");
    }

    public bool CancelCustomScan()
    {
      return true;
    }

    public IHeliosCustomScan CreateCustomScan()
    {
      return new HeliosCustomScan();
    }

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

    /// <summary>
    /// It gets interesting here...how should we send a custom scan to the VMS?
    /// </summary>
    /// <param name="customScan"></param>
    /// <returns></returns>
    public bool SetCustomScan(IHeliosCustomScan customScan)
    {
      return true;
    }

  }
}
