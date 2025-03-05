using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Helios.Interfaces;
using Helios.Interfaces.InstrumentAccess;
using Helios.Interfaces.InstrumentAccess.Control;
using Helios.Interfaces.InstrumentAccess.Control.Acquisition;
using Helios.Interfaces.InstrumentAccess.Control.Scans;
using Helios.Interfaces.InstrumentAccess.MsScanContainer;
using ScottPlot;
using ScottPlot.Plottables;

namespace ScanInjector
{

  public partial class ScanInjector : Form
  {

    private IInstrumentAccessContainer msIAC;
    private IInstrumentAccess msIA;
    private IControl msControl;
    private IAcquisition msAcquisition;
    private IMsScanContainer msMSSC;
    private IScans msScans;

    readonly Scatter plotSpec;

    bool connected = false;
    bool scanInjectorActive = false;

    private List<QuickScan> myScans = new List<QuickScan>();

    public ScanInjector()
    {
      InitializeComponent();

      Coordinates[] co = new Coordinates[1];
      co[0].X = 0;
      co[0].Y = 0;

      plotSpec = plotSpectrum.Plot.Add.Scatter(co);
      plotSpec.MarkerSize = 1;
      plotSpectrum.Plot.Axes.SetLimitsY(0, 100);
      plotSpectrum.Plot.YLabel("Relative Intensity");
      plotSpectrum.Plot.XLabel("m/z");
      plotSpectrum.Plot.HideGrid();
      plotSpectrum.Refresh();

      UpdateConnection();
    }

    void AcquisitionErrorsArrived(object sender, AcquisitionErrorsArrivedEventArgs e)
    {
      Log("Acquisition Error: " + e.ToString());
    }

    private void AcquisitionStreamClosing(object sender, EventArgs e)
    {
      Log("AcquisitionStreamClosing.");
    }

    private void AcquisitionStreamOpening(object sender, AcquisitionOpeningEventArgs e)
    {
      string str = "AcquisitionStreamOpening: ";
      Invoke(new Action(() =>
      {
        foreach (var de in e.StartingInformation)
        {
          //For documentation
          //Log("Key: '" + de.Key + "'");
          if (de.Key.CompareTo("RawFileName") == 0 || de.Key.CompareTo("RawFile") == 0)
          {
            str += de.Value;
          }
        }
      }));
      Log(str);
    }

    void ActivateScanInjector()
    {
      if (!scanInjectorActive)
      {
        msControl = msIA.Control;
        msMSSC = msIA.GetMsScanContainer(0);
        msMSSC.MsScanArrived += MsScanArrived;

        msAcquisition = msControl.Acquisition;
        msAcquisition.StateChanged += StateChanged;
        msAcquisition.AcquisitionStreamOpening += AcquisitionStreamOpening;
        msAcquisition.AcquisitionStreamClosing += AcquisitionStreamClosing;

        msScans = msControl.GetScans(false);
        //msScans.CanAcceptNextCustomScan += CanAcceptNextCustomScan;
        msScans.PossibleParametersChanged += OnPossibleParametersChanged;

        buttonListen.Text = "Deactivate";

        scanInjectorActive = true;
        CheckScanSettings();
      }
      
    }

    void CheckScanSettings()
    {
      buttonCustomScan.Enabled = false;
      if (!scanInjectorActive) return;
      if (comScanType.Text.Equals("MS2"))
      {
        if (comTargets.Items.Count == 0) return;
      }
      buttonCustomScan.Enabled = true;
    }

    void ConnectionChanged(object sender, EventArgs e)
    {
      if (msIA != null)
      {
        Log(msIA.Connected.ToString() + " :: " + e.ToString());
      }
    }

    void ContactClosureChanged(object sender, ContactClosureEventArgs e)
    {
      Log("Contact Closure changed: " + e.ToString() + " " + e.DidRise.ToString() + " " + e.DidFall.ToString());
    }

    void DeactivateScanInjector()
    {
      if (scanInjectorActive)
      {
        //msScans.CanAcceptNextCustomScan -= CanAcceptNextCustomScan;
        msScans.PossibleParametersChanged -= OnPossibleParametersChanged;
        msScans = null;

        msAcquisition.StateChanged -= StateChanged;
        msAcquisition.AcquisitionStreamOpening -= AcquisitionStreamOpening;
        msAcquisition.AcquisitionStreamClosing -= AcquisitionStreamClosing;
        msAcquisition = null;
        
        msMSSC.MsScanArrived -= MsScanArrived;
        msMSSC = null;
        msControl = null;
        scanInjectorActive = false;

        buttonListen.Text = "Activate";
      }
    }

    void Log(string s)
    {
      rtbLog.AppendText(s + System.Environment.NewLine);
    }

    void MessagesArrived(object sender, MessagesArrivedEventArgs e)
    {
      StringBuilder sb = new StringBuilder();
      foreach (var message in e.Messages)
      {
        string msg = string.Format("[{0}] ID: {1} Status: {2} Msg: {3}",
            message.CreationTime,
            message.MessageId,
            message.Status,
            string.Format(message.Message, message.MessageArgs));

        sb.AppendLine(msg);
      }
      Log(sb.ToString());
    }

    void MsScanArrived(object sender, MsScanEventArgs e)
    {
      using (IMsScan scan = e.GetScan())
      {
        //Skip any scans that are not the one we injected
        string tmp;
        if (!scan.TryTrailer("Access ID", out tmp)) return;
        if (Convert.ToInt32(tmp) != 99) return;

        int scanNumber = 0;
        double firstMass = 0;
        double lastMass = 100;
        if (scan.TryHeader("Scan", out tmp)) scanNumber = Convert.ToInt32(tmp);
        if (scan.TryHeader("FirstMass", out tmp)) firstMass = Convert.ToDouble(tmp);
        if (scan.TryHeader("LastMass", out tmp)) lastMass = Convert.ToDouble(tmp);
        Log("Injected Scan: " + scanNumber);

        QuickScan qs = new QuickScan(scanNumber);
        qs.firstMass = firstMass;
        qs.lastMass = lastMass;
        foreach (var centroid in scan.Centroids)
        {
          qs.Add(centroid.Mz, centroid.Intensity);
        }
        myScans.Add(qs);
        comScanCollection.Items.Add(scanNumber.ToString());
        comScanCollection.SelectedItem = comScanCollection.Items[comScanCollection.Items.Count - 1];
      }
    }

    void OnPossibleParametersChanged(object sender, EventArgs e)
    {
      Log("Possible parameters changed.");
    }


    void ServiceConnectionChanged(object sender, EventArgs e)
    {
      if (msIAC != null)
      {
        connected = msIAC.ServiceConnected;
        string st = msIAC.ServiceConnected ? "connected" : "disconnected";
        Log("ServiceConnectionChanged: Service is now " + st);

        if (msIAC.ServiceConnected)
        {

          try
          {
            msIA = msIAC.Get(1);
            msIA.AcquisitionErrorsArrived += AcquisitionErrorsArrived;
            msIA.ConnectionChanged += ConnectionChanged;
            msIA.ContactClosureChanged += ContactClosureChanged;
            Log("Connected to " + msIA.InstrumentName);

            //Update the available analyzers
            UpdateAnalyzer(msIA.InstrumentName);
          }
          catch (Exception ex)
          {
            Log("Failed to get instrument access: " + ex.Message);
          }

        }
      }

      UpdateConnection();

    }

    private void StateChanged(object sender, StateChangedEventArgs e)
    {

    }

    private void UpdateAnalyzer(string Analyzer)
    {
      comRes.Items.Clear();
      comAnalyzer.Items.Clear();
      comAnalyzer.Items.Add("Orbitrap");
      if (Analyzer.Contains("Exploris"))
      {
        comRes.Items.Add("7500");
        comRes.Items.Add("11250");
        comRes.Items.Add("15000");
        comRes.Items.Add("22500");
        comRes.Items.Add("30000");
        comRes.Items.Add("45000");
        comRes.Items.Add("60000");
        comRes.Items.Add("75000");
        comRes.Items.Add("90000");
        comRes.Items.Add("120000");
        comRes.Items.Add("180000");
        comRes.Items.Add("240000");
        comRes.Items.Add("480000");
      }
      else
      {
        comAnalyzer.Items.Add("IonTrap");
        comRes.Items.Add("7500");
        comRes.Items.Add("15000");
        comRes.Items.Add("30000");
        comRes.Items.Add("50000");
        comRes.Items.Add("60000");
        comRes.Items.Add("120000");
        comRes.Items.Add("240000");
      }
      comAnalyzer.SelectedIndex = 0;
      comRes.SelectedIndex = 0;
    }

    private void UpdateConnection()
    {
      if (msIAC != null)
      {
        connectionIndicator.BackColor = msIAC.ServiceConnected ? System.Drawing.Color.Lime : System.Drawing.Color.Gray;
        disconnectionIndicator.BackColor = msIAC.ServiceConnected ? System.Drawing.Color.Gray : System.Drawing.Color.Red;
        buttonConnect.Text = msIAC.ServiceConnected ? "Disconnect" : "Connect";
        connected = msIAC.ServiceConnected ? true : false;
      }
      else
      {
        connectionIndicator.BackColor = System.Drawing.Color.Gray;
        disconnectionIndicator.BackColor = System.Drawing.Color.Red;
        buttonConnect.Text = "Connect";
        connected = false;
      }
      buttonConnect.Enabled = true;

      if (scanInjectorActive)
      {
        listenIndicatorOff.BackColor = System.Drawing.Color.Gray;
        listenIndicatorOn.BackColor = System.Drawing.Color.Lime;
      }
      else
      {
        listenIndicatorOff.BackColor = System.Drawing.Color.Red;
        listenIndicatorOn.BackColor = System.Drawing.Color.Gray;
      }
      buttonListen.Enabled = true;

      CheckScanSettings();
    }

    private void buttonConnect_Click(object sender, EventArgs e)
    {
      //Step #1, disable the buttons so that they can't be clicked while this
      //function is executing.
      buttonConnect.Enabled = false;
      buttonListen.Enabled = false;

      //Step #2, if connected, then start the disconnect procedure.
      if (connected)
      {
        Log("Attempting graceful disconnect...");
        if (scanInjectorActive) DeactivateScanInjector();
        if (msIA != null)
        {
          msIA.AcquisitionErrorsArrived -= AcquisitionErrorsArrived;
          msIA.ConnectionChanged -= ConnectionChanged;
          msIA.ContactClosureChanged -= ContactClosureChanged;
          msIA = null;
        }
        if (msIAC != null)
        {
          msIAC.Dispose();
          msIAC = null;
        }
        Log("Disconnected");
      }

      //Step #2b, not connected, so start the connection procedure.
      else
      {
        try
        {
          Log("Attempting to connect to instrument or VMS.");

          msIAC = InstrumentAccessContainerFactory.Create();
          if (msIAC == null) Log("Failed to connect to instrument or VMS.");
          else
          {
            try
            {
              msIAC.ServiceConnectionChanged += ServiceConnectionChanged;
              msIAC.MessagesArrived += MessagesArrived;
              msIAC.StartOnlineAccess();
            }
            catch (Exception exx)
            {
              Log(exx.Message);
            }
          }
        }
        catch (Exception ex)
        {
          Log("InstrumentAccessContainerFactory.Get() " + ex.Message);
        }
      }

      //Step #3: Update the user iterface (with whatever results happened here).
      UpdateConnection();
    }

    private void buttonListen_Click(object sender, EventArgs e)
    {
      buttonListen.Enabled = false;
      if (scanInjectorActive)
      {
        DeactivateScanInjector();
      }
      else
      {
        ActivateScanInjector();
      }
      UpdateConnection();
    }

    private void comAnalyzer_SelectedValueChanged(object sender, EventArgs e)
    {
      if (comAnalyzer.Text.Equals("Orbitrap")){
        comRes.Enabled = true;
      } else comRes.Enabled = false;
      CheckScanSettings();
    }

    private void comScanType_SelectedValueChanged(object sender, EventArgs e)
    {
      if (comScanType.Text.Equals("MS1"))
      {
        comTargets.Enabled = false;
        comAddTarget.Enabled = false;
        comRemoveTarget.Enabled = false;
        comClearTarget.Enabled = false;
        comNCE.Enabled = true;
      } else
      {
        comTargets.Enabled = true;
        comAddTarget.Enabled = true;
        comRemoveTarget.Enabled = true;
        comClearTarget.Enabled = true;
        comNCE.Enabled = false;
      }
      CheckScanSettings();
    }

    private void comAddTarget_Click(object sender, EventArgs e)
    {
      TargetMZ targetMZ = new TargetMZ();
      DialogResult res = targetMZ.ShowDialog();
      if (res == DialogResult.OK)
      {
        comTargets.Items.Add(targetMZ.value);
      }
      CheckScanSettings();
    }

    private void comRemoveTarget_Click(object sender, EventArgs e)
    {
      if (comTargets.SelectedItem == null) return;
      comTargets.Items.Remove(comTargets.SelectedItem);
      CheckScanSettings();
    }

    private void comClearTarget_Click(object sender, EventArgs e)
    {
      comTargets.Items.Clear();
      CheckScanSettings();
    }

    private void buttonCustomScan_Click(object sender, EventArgs e)
    {
      ICustomScan cs = msScans.CreateCustomScan();
      cs.SingleProcessingDelay = 0;
      cs.RunningNumber = 99;

      if (comScanType.Text.Equals("MS1"))
      {
        cs.Values["ScanType"] = "Full";
        cs.Values["CollisionEnergy"] = "0";
        cs.Values["NCE"] = "0";
        cs.Values["AGCTarget"]= comAGC.Value.ToString();
      }
      else
      {
        cs.Values["CollisonEnergy"]=comNCE.Value.ToString();
        cs.Values["ScanType"] = "MSn";
        cs.Values["IsolationWidth"] = "1.5";
        cs.Values["PrecursorMass"] = comTargets.Items[0].ToString();
        cs.Values["ChargeStates"] = "0";
        cs.Values["MSXTargets"] = comAGC.Value.ToString();
        cs.Values["IsolationRangeLow"] = (Convert.ToDouble(comTargets.Items[0])-0.75).ToString();
        cs.Values["IsolationRangeHigh"] = (Convert.ToDouble(comTargets.Items[0]) + 0.75).ToString();
        if (comTargets.Items.Count > 1)
        {
          cs.Values["MsxInjectRanges"] = ProcessRangesEclipse();
          cs.Values["MsxInjectTargets"] = ProcessInjectTargetsEclipse();
          cs.Values["MsxInjectMaxITs"] = ProcessMaxITEclipse();
          cs.Values["MsxInjectNCEs"] = ProcessNCEEclipse();
          for(int i = 1;i< comTargets.Items.Count; i++)
          {
            cs.Values["PrecursorMass"] += ("," + comTargets.Items[i].ToString());
            cs.Values["IsolationWidth"] += ",1.5";
            cs.Values["ChargeStates"] += ",0";
            cs.Values["MSXTargets"] += ("," + comAGC.Value.ToString());

          }
        }
      }
      cs.Values["Resolution"] = comRes.Text.ToString();
      if (comAnalyzer.Text.Equals("Orbitrap"))
      {
        cs.Values["Analyzer"] = "Orbitrap";
        cs.Values["OrbitrapResolution"] = comRes.Text.ToString();
      }
      else cs.Values["Analyzer"] = "IonTrap";

      cs.Values["ActivationType"] = "HCD";
      cs.Values["FirstMass"]=comMassLow.Value.ToString();
      cs.Values["LastMass"]=comMassHigh.Value.ToString();
      //cs.Values["MaxIT"]=comIT.Value.ToString();

      foreach (KeyValuePair<string, string> pair in cs.Values)
      {
        Log(pair.Key + " = " + pair.Value);
      }
      if (!msScans.SetCustomScan(cs))
      {
        Log("Failed to submit custom scan");
      }
    }

    private string ProcessRangesEclipse()
    {
      string st = "[";
      bool first = true;
      foreach (var item in comTargets.Items)
      {
        if (first) first = false;
        else st += ",";
        st += "(";
        st += (Convert.ToDouble(item) - 0.75).ToString();
        st += ",";
        st += (Convert.ToDouble(item) + 0.75).ToString();
        st += ")";
      }
      st += "]";
      return st;
    }

    private string ProcessInjectTargetsEclipse()
    {
      string st = "[";
      bool first = true;
      foreach (var item in comTargets.Items)
      {
        if (first) first = false;
        else st += ",";
        st += comAGC.Value.ToString();
      }
      st += "]";
      return st;
    }

    private string ProcessMaxITEclipse()
    {
      string st = "[";
      bool first = true;
      foreach (var item in comTargets.Items)
      {
        if (first) first = false;
        else st += ",";
        st += comIT.Value.ToString();
      }
      st += "]";
      return st;
    }

    private string ProcessNCEEclipse()
    {
      string st = "[";
      bool first = true;
      foreach (var item in comTargets.Items)
      {
        if (first) first = false;
        else st += ",";
        st += comNCE.Value.ToString();
      }
      st += "]";
      return st;
    }

    private void comScanCollection_SelectedIndexChanged(object sender, EventArgs e)
    {
      QuickScan qs = myScans[comScanCollection.SelectedIndex];

      double[] x;
      double[] y;
      int a = 0;

      x = new double[qs.mz.Count() * 3 + 2];
      y = new double[qs.intensity.Count() * 3 + 2];
      x[a] = qs.firstMass;
      y[a++] = 0;
      for(int i = 0; i < qs.mz.Count(); i++)
      {
        //Log(qs.mz[i].ToString()+" " + (qs.intensity[i] / qs.maxIntensity * 100).ToString());
        //centroided peaks need to be plotted with 3 points 
        x[a] = qs.mz[i];
        y[a++] = 0;
        x[a] = qs.mz[i];
        y[a++] = qs.intensity[i] / qs.maxIntensity * 100;
        x[a] = qs.mz[i];
        y[a++] = 0;
      }
      x[a] = qs.lastMass;
      y[a] = 0;

      lock (plotSpectrum.Plot.Sync)
      {
        plotSpectrum.Plot.Clear();
        var scat = plotSpectrum.Plot.Add.Scatter(x, y);
        scat.MarkerSize = 1;
        plotSpectrum.Plot.Axes.SetLimitsX(qs.firstMass, qs.lastMass);
      }
      plotSpectrum.Refresh();
    }
  }
}
