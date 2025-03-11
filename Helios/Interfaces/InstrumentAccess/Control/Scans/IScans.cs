extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Interfaces.InstrumentAccess.Control;
using Pipes;
using Thermo.Interfaces.InstrumentAccess_V1.Control;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;

namespace Helios.Interfaces.InstrumentAccess.Control.Scans
{
  public interface IScans : IDisposable//: exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IScans, fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionScans
  {
    bool CancelCustomScan();
    bool CancelRepetition();
    ICustomScan CreateCustomScan();
    IRepeatingScan CreateRepeatingScan();
    bool SetCustomScan(ICustomScan scan);
    bool SetRepetitionScan(IRepeatingScan scan);

    /// <summary>
    /// All possible parameters of a scan specific to the currently connected instrument
    /// </summary>
    IParameterDescription[] PossibleParameters { get; }
    IParameterDescription[] HeliosPossibleParameters { get; }

    event EventHandler<EventArgs> CanAcceptNextCustomScan;
    event EventHandler<EventArgs> PossibleParametersChanged;
  }

  class HeliosScansExploris: IScans 
  {
    exploris.Thermo.Interfaces.InstrumentAccess_V1.Control.Scans.IScans scans;
    private bool _disposedValue;

    public IParameterDescription[] PossibleParameters { get; private set; }
    public IParameterDescription[] HeliosPossibleParameters { get; } = new IParameterDescription[HeliosCustomDictionary.HeliosLexicon.Count];

    public event EventHandler<EventArgs> CanAcceptNextCustomScan;
    public event EventHandler<EventArgs> PossibleParametersChanged;
    public HeliosScansExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.Control.IExplorisControl c, bool exclusiveAccess)
    {
      scans = c.GetScans(exclusiveAccess);

      //I found this code in CustomScansTandemByCallbacks in the IAPI\Exploris\5_PlacingScans demo.
      //Without it, the custom scan dictionary might not be built and available before the user
      //attempts a custom scan.
      ManualResetEvent wait = new ManualResetEvent(false);
      EventHandler handler = (sender, e) => { wait.Set(); };
      scans.PossibleParametersChanged += handler;
      if ((scans.PossibleParameters != null) && (scans.PossibleParameters.Length > 0))
      {
        wait.Set();
      }
      wait.WaitOne(20000);
      scans.PossibleParametersChanged -= handler;

      if ((scans.PossibleParameters == null) || (scans.PossibleParameters.Length == 0))
      {
        //TODO: see if the exception is the best way, or if raising an event is better.
        throw new TimeoutException("Not connected to the instrument or something else happened.");
      }
      //end IAPI\Exploris\5_PlacingScans demo code block

      PossibleParameters = new IParameterDescription[scans.PossibleParameters.Length];
      for (int i = 0; i < scans.PossibleParameters.Length; i++)
      {
        PossibleParameters[i] = new HeliosParameterDescription(scans.PossibleParameters[i].Name, scans.PossibleParameters[i].Selection, scans.PossibleParameters[i].DefaultValue, scans.PossibleParameters[i].Help);
        HeliosCustomDictionary.AddExplorisParam(scans.PossibleParameters[i].Name);
      }

      for(int i = 0; i<HeliosCustomDictionary.HeliosLexicon.Count; i++)
      {
        HeliosPossibleParameters[i] = new HeliosParameterDescription(HeliosCustomDictionary.HeliosLexicon[i].HeliosID, "","",HeliosCustomDictionary.GetDescription(HeliosCustomDictionary.HeliosLexicon[i].HeliosID));
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

    public bool CancelRepetition()
    {
      return scans.CancelRepetition();
    }

    public ICustomScan CreateCustomScan()
    {
      return new HeliosCustomScan();
    }

    public IRepeatingScan CreateRepeatingScan()
    {
      return new HeliosRepeatingScan();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          scans?.Dispose();
          scans = null;
        }
        // free native resources if there are any.
        _disposedValue = true;
      }
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
      PossibleParameters = new IParameterDescription[scans.PossibleParameters.Length];
      for (int i = 0; i < scans.PossibleParameters.Length; i++)
      {
        PossibleParameters[i] = new HeliosParameterDescription(scans.PossibleParameters[i].Name, scans.PossibleParameters[i].Selection, scans.PossibleParameters[i].DefaultValue, scans.PossibleParameters[i].Help);
        HeliosCustomDictionary.AddExplorisParam(scans.PossibleParameters[i].Name);
      }
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

    public bool SetCustomScan(ICustomScan scan)
    {
      return scans.SetCustomScan(new ExplorisCustomScan(scan));
    }

    public bool SetRepetitionScan(IRepeatingScan scan)
    {
      return scans.SetRepetitionScan(new ExplorisRepeatingScan(scan));
    }
  }

  class HeliosScansFusion : IScans
  {
    fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionScans scans;

    private bool _disposedValue;

    public IParameterDescription[] PossibleParameters { get; }
    public IParameterDescription[] HeliosPossibleParameters { get; } = new IParameterDescription[HeliosCustomDictionary.HeliosLexicon.Count];

    public event EventHandler<EventArgs> CanAcceptNextCustomScan;
    public event EventHandler<EventArgs> PossibleParametersChanged;
    public HeliosScansFusion(fusion.Thermo.Interfaces.FusionAccess_V1.Control.IFusionControl c, bool exclusiveAccess)
    {
      scans = (fusion.Thermo.Interfaces.FusionAccess_V1.Control.Scans.IFusionScans)c.GetScans(exclusiveAccess);
      
      PossibleParameters = new IParameterDescription[scans.PossibleParameters.Length];
      for (int i = 0; i < scans.PossibleParameters.Length; i++)
      {
        PossibleParameters[i] = new HeliosParameterDescription(scans.PossibleParameters[i].Name,scans.PossibleParameters[i].Selection,scans.PossibleParameters[i].DefaultValue,scans.PossibleParameters[i].Help);
        HeliosCustomDictionary.AddFusionParam(scans.PossibleParameters[i].Name);
      }

      for (int i = 0; i < HeliosCustomDictionary.HeliosLexicon.Count; i++)
      {
        HeliosPossibleParameters[i] = new HeliosParameterDescription(HeliosCustomDictionary.HeliosLexicon[i].HeliosID, "", "", HeliosCustomDictionary.GetDescription(HeliosCustomDictionary.HeliosLexicon[i].HeliosID));
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
    public bool CancelRepetition()
    {
      return scans.CancelRepetition();
    }

    public ICustomScan CreateCustomScan()
    {
      return new HeliosCustomScan();
    }

    public IRepeatingScan CreateRepeatingScan()
    {
      return new HeliosRepeatingScan();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          scans?.Dispose();
          scans = null;
        }
        // free native resources if there are any.
        _disposedValue = true;
      }
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

    public bool SetCustomScan(ICustomScan scan)
    {
      return scans.SetCustomScan(new FusionCustomScan(scan));
    }

    public bool SetRepetitionScan(IRepeatingScan scan)
    {
      return scans.SetRepetitionScan(new FusionRepeatingScan(scan));
    }
  }

  class HeliosScansVMS : IScans
  {
    private bool _disposedValue;

    public IParameterDescription[] PossibleParameters { get; } = new HeliosParameterDescription[10];
    public IParameterDescription[] HeliosPossibleParameters { get; } = new IParameterDescription[HeliosCustomDictionary.HeliosLexicon.Count];

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

      for (int i = 0; i < HeliosCustomDictionary.HeliosLexicon.Count; i++)
      {
        HeliosPossibleParameters[i] = new HeliosParameterDescription(HeliosCustomDictionary.HeliosLexicon[i].HeliosID, "", "", HeliosCustomDictionary.GetDescription(HeliosCustomDictionary.HeliosLexicon[i].HeliosID));
      }
    }

    public bool CancelCustomScan()
    {
      return true;
    }

    public bool CancelRepetition()
    {
      return true;
    }

    public ICustomScan CreateCustomScan()
    {
      return new HeliosCustomScan();
    }

    public IRepeatingScan CreateRepeatingScan()
    {
      return new HeliosRepeatingScan();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
        }
        // free native resources if there are any.
        _disposedValue = true;
      }
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

    private byte[] Serialize(ICustomScan customScan)
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
    public bool SetCustomScan(ICustomScan customScan)
    {
      PipeMessage pm = new PipeMessage();
      pm.MsgCode = 'A';
      pm.MsgData = Serialize(customScan);
      pipesClient.Send(pm);
      return true;
    }

    public bool SetRepetitionScan(IRepeatingScan scan)
    {
      return false; //disabled for now
    }

  }
}
