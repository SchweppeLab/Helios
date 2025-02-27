extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Interfaces.InstrumentAccess.Control;
using Pipes;
using Thermo.Interfaces.InstrumentAccess_V1.Control;

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
      PossibleParameters = new IHeliosParameterDescription[scans.PossibleParameters.Length];
      for (int i = 0; i < scans.PossibleParameters.Length; i++)
      {
        PossibleParameters[i] = new HeliosParameterDescription(scans.PossibleParameters[i].Name, scans.PossibleParameters[i].Selection, scans.PossibleParameters[i].DefaultValue, scans.PossibleParameters[i].Help);
      }
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
      PossibleParameters = new IHeliosParameterDescription[scans.PossibleParameters.Length];
      for (int i = 0; i < scans.PossibleParameters.Length; i++)
      {
        PossibleParameters[i] = new HeliosParameterDescription(scans.PossibleParameters[i].Name,scans.PossibleParameters[i].Selection,scans.PossibleParameters[i].DefaultValue,scans.PossibleParameters[i].Help);
      }
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
      return scans.SetCustomScan(customScan.ToFusion());
    }
  }

  class HeliosScansVMS : IHeliosScans
  {

    public IHeliosParameterDescription[] PossibleParameters { get; } = new HeliosParameterDescription[10];

    public event EventHandler<EventArgs> CanAcceptNextCustomScan;
    public event EventHandler<EventArgs> PossibleParametersChanged;

    private readonly PipesClient pipesClient = null;

    public HeliosScansVMS(PipesClient pc = null)
    {
      pipesClient = pc;

      string multiVal = "It is expressed as a string of values, with each value separated by a ';' delimiter. A maximum of 10 values can be defined.";
      PossibleParameters[0] = new HeliosParameterDescription("CollisionEnergy", "string (0:200)", "", "The normalized collision energy (NCE). " + multiVal);
      PossibleParameters[1] = new HeliosParameterDescription("ScanRate", "Normal,Rapid,Turbo,Zoom,Enhanced", "Normal", "The scan rate of the ion trap.");
      PossibleParameters[2] = new HeliosParameterDescription("FirstMass", "string (50;2000)", "150", "The first mass of the scan range. "+ multiVal);
      PossibleParameters[3] = new HeliosParameterDescription("LastMass", "string (50;2000)", "2000", "The last mass of the scan range. " + multiVal);
      PossibleParameters[4] = new HeliosParameterDescription("Analyzer", "IonTrap,Orbitrap", "IonTrap", "The mass analyzer.");
      PossibleParameters[5] = new HeliosParameterDescription("ScanType", "Full,SIM,MSn", "Full", "The type of scan to perform.");
      PossibleParameters[6] = new HeliosParameterDescription("Polarity", "Positive,Negative,Both", "Positive", "The polarity of the scan.");
      PossibleParameters[7] = new HeliosParameterDescription("DataType", "Centroid,Profile", "Centroid", "The data type to collect the scan in.");
      PossibleParameters[8] = new HeliosParameterDescription("SrcRFLens", "string (0;150)", "60", "The RF Lens (%) for the source. " + multiVal);
      PossibleParameters[9] = new HeliosParameterDescription("SourceCIDEnergy", "0-100", "0", "Source CID Energy (0 = off).");
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

    private byte[] Serialize(IHeliosCustomScan customScan)
    {
      using (MemoryStream m = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(m, System.Text.Encoding.Unicode))
      {
        writer.Write(customScan.Values.Count);
        foreach(KeyValuePair<string,string> kvp in customScan.Values)
        {
          writer.Write(kvp.Key);
          writer.Write(kvp.Value);
        }
        return m.ToArray();
      }
    }

    /// <summary>
    /// It gets interesting here...how should we send a custom scan to the VMS?
    /// </summary>
    /// <param name="customScan"></param>
    /// <returns></returns>
    public bool SetCustomScan(IHeliosCustomScan customScan)
    {
      using (StreamWriter writer = new StreamWriter("uiapiLog.txt", true))
      {
        writer.WriteLine("Custom scan with " + customScan.Values.Count.ToString() + " Values.");
      }
      PipeMessage pm = new PipeMessage();
      pm.MsgCode = 'A';
      pm.MsgData = Serialize(customScan);
      pipesClient.Send(pm);
      return true;
    }

  }
}
