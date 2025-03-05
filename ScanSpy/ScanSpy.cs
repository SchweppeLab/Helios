using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScottPlot.Plottables;
using ScottPlot;

using Helios.Interfaces;
using Helios.Interfaces.InstrumentAccess;
using Helios.Interfaces.InstrumentAccess.Control;
using Helios.Interfaces.InstrumentAccess.Control.Acquisition;
using Helios.Interfaces.InstrumentAccess.Control.Scans;
using Helios.Interfaces.InstrumentAccess.MsScanContainer;
using ScottPlot.Colormaps;


namespace ScanSpy
{
  public partial class ScanSpy: Form
  {
    private IInstrumentAccessContainer msIAC;
    private IInstrumentAccess msIA;
    private IControl msControl;
    private IAcquisition msAcquisition;
    private IMsScanContainer msMSSC;
    private IScans msScans;

    int[] scanCount = new int[4];
    int[] totalScanCount = new int[4];
    bool connected = false;
    bool listener = false;
    bool ignoreScan = false;

    Queue<double> scanHistory = new Queue<double>();

    readonly Scatter plotSpec;
    private long lastTicks = 0;
    private volatile bool refreshSpectrum = false;
    //private VMsStats stats = new VMsStats();

    private int curSimCount = 0;
    private double curSimRT = 0;
    private TimeSpan allSimTime = TimeSpan.Zero;
    readonly System.Windows.Forms.Timer UpdatePlotTimer = new System.Windows.Forms.Timer();

    bool bTemp = false;

    public ScanSpy()
    {
      InitializeComponent();

      UpdatePlotTimer.Interval = 100;
      UpdatePlotTimer.Enabled = true;

      Coordinates[] co = new Coordinates[1];
      co[0].X = 0;
      co[0].Y = 0;

      plotSpec = plotSpectrum.Plot.Add.Scatter(co);
      plotSpec.MarkerSize = 1;
      plotSpectrum.Plot.Axes.SetLimitsY(0,100);
      plotSpectrum.Plot.YLabel("Relative Intensity");
      plotSpectrum.Plot.XLabel("m/z");
      plotSpectrum.Plot.HideGrid();
      plotSpectrum.Refresh();

      UpdateConnection();
      Log(RuntimeInformation.FrameworkDescription);

      UpdatePlotTimer.Tick += (s, e) =>
      {
        if (refreshSpectrum)
        {
          plotSpectrum.Refresh();
          refreshSpectrum = false;
        }
      };

      lastTicks = DateTime.Now.Ticks;
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
        if (listener) StopListening();
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
            Log("Instrument ID: " + msIAC.InstrumentType());
            try
            {
              msIAC.ServiceConnectionChanged += ServiceConnectionChanged;
              msIAC.MessagesArrived += MessagesArrived;
              Log("Attempting to start online access, already at " + msIAC.ServiceConnected.ToString());
              msIAC.StartOnlineAccess();
              Log("StartOnlineAccess() happened. This message appears in lieu of a more natural message.");
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

    void AcquisitionErrorsArrived(object sender, AcquisitionErrorsArrivedEventArgs e)
    {
      Log("Acquisition Error: " + e.ToString());
    }

    private void AcquisitionStreamClosing(object sender, EventArgs e)
    {
      Log("AcquisitionStreamClosing.");
      if (listener && cbOnAcquisition.Checked)
      {
        listenIndicatorOn.BackColor = System.Drawing.Color.Gray;
        listenIndicatorWait.BackColor = System.Drawing.Color.Yellow;
        listenIndicatorOff.BackColor = System.Drawing.Color.Gray;
        ignoreScan = true;
      }
    }

    private void AcquisitionStreamOpening(object sender, AcquisitionOpeningEventArgs e)
    {
      ignoreScan = false;
      listenIndicatorOn.BackColor = System.Drawing.Color.Lime;
      listenIndicatorWait.BackColor = System.Drawing.Color.Gray;
      listenIndicatorOff.BackColor = System.Drawing.Color.Gray;

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

    private void CheckDictionary(string key, string value, int msLevel, DataGridView dgv)
    {
      bool match = false;
      foreach (DataGridViewRow row in dgv.Rows)
      {
        if (row.Cells[0].Value.ToString() == key)
        {
          match = true;
          row.Cells[1].Value += "," + msLevel.ToString();
          if (msLevel > 0 && msLevel < 4) row.Cells[msLevel + 1].Value = value;
          break;
        }
      }

      if (!match)
      {
        int index = dgv.Rows.Add();
        dgv.Rows[index].Cells[0].Value = key;
        dgv.Rows[index].Cells[1].Value = msLevel.ToString();
        if (msLevel > 0 && msLevel < 4) dgv.Rows[index].Cells[msLevel + 1].Value = value;
      }
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

    private void MsScanArrived(object sender, MsScanEventArgs e)
    {
      if (ignoreScan) return;

      string tmp;
      using (IMsScan msScan = e.GetScan())
      {
        double rt = 0;
        if (msScan.TryHeader("StartTime", out tmp)) rt = Convert.ToDouble(tmp);
        scanHistory.Enqueue(rt);
        while (scanHistory.Count()>0 && rt - scanHistory.First() > 0.1)
        {
          scanHistory.Dequeue();
        }
        double hz = scanHistory.Count() * 0.1/(rt - scanHistory.First()) / 6;
        labelScanSpeed.Text = hz.ToString() + " Hz";

        long curTicks = DateTime.Now.Ticks;
        long tickDif = curTicks - lastTicks;
        if (tickDif > 1e6) //only refresh at intervals
        {
          int scanNumber = 0;
          double firstMass = 0;
          double lastMass = 100;
          double basePeakIntensity = 0;
          bool isCentroid = false;
          if (msScan.TryHeader("Scan", out tmp)) scanNumber = Convert.ToInt32(tmp);
          if (msScan.TryHeader("FirstMass", out tmp)) firstMass = Convert.ToDouble(tmp);
          if (msScan.TryHeader("LastMass", out tmp)) lastMass = Convert.ToDouble(tmp);
          if (msScan.TryHeader("BasePeakIntensity", out tmp)) basePeakIntensity = Convert.ToDouble(tmp);
          if (msScan.TryHeader("ScanData", out tmp))
          {
            if (tmp == "Centroid") isCentroid = true;
          }

          string scanFilter = ProcessScanHeader(msScan);          

          double[] x;
          double[] y;
          int a = 0;
          if (isCentroid)
          {
            x = new double[msScan.Centroids.Count()*3+2];
            y = new double[msScan.Centroids.Count()*3+2];
            x[a] = firstMass;
            y[a++] = 0;
            foreach (var centroid in msScan.Centroids)
            {
              //centroided peaks need to be plotted with 3 points 
              x[a] = centroid.Mz;
              y[a++] = 0;
              x[a] = centroid.Mz;
              y[a++] = centroid.Intensity/basePeakIntensity*100;
              x[a] = centroid.Mz;
              y[a++] = 0;
            }
            x[a] = lastMass;
            y[a] = 0;
          } 
          else
          {
            x = new double[msScan.Centroids.Count()+2];
            y = new double[msScan.Centroids.Count()+2];
            x[a] = firstMass;
            y[a++] = 0;
            foreach (var centroid in msScan.Centroids)
            {
              x[a] = centroid.Mz;
              y[a] = centroid.Intensity/basePeakIntensity*100;
              //if (centroid.IsMonoisotopic != null)
              //{
              //  Log("IsMonoisotopic is not null: " + centroid.IsMonoisotopic.ToString() + " mz: " + centroid.Mz.ToString() + " in scan " + scanNumber.ToString());
              //} 
              a++;
            }
            x[a]=lastMass;
            y[a] = 0;
          }
          
          lock (plotSpectrum.Plot.Sync)
          {
            plotSpectrum.Plot.Clear();
            var scat = plotSpectrum.Plot.Add.Scatter(x, y);
            scat.MarkerSize = 1;
            //plotSpectrum.Plot.Axes.AutoScaleY();
            plotSpectrum.Plot.Axes.SetLimitsX(firstMass, lastMass);
          }

          lblScanFilter.Text = scanFilter;
          lblScanInfo.Text = "Scan #" + scanNumber.ToString() + "  RT:" + rt.ToString("#.00") + "  NL:" + basePeakIntensity.ToString("E2"); ;
          refreshSpectrum = true;
          lastTicks = curTicks;
        }

        //Count run statistics
        scanCount[0]++;
        totalScanCount[0]++;
        msScan.Header.TryGetValue("MSOrder", out tmp);
        if (tmp != null)
        {
          if (tmp.Equals("MS2") || tmp.Equals("2"))
          {
            scanCount[2]++;
            totalScanCount[2]++;
          }
          else if (tmp.Equals("MS") || tmp.Equals("1"))
          {
            scanCount[1]++;
            totalScanCount[1]++;
          } 
          else if (tmp.Equals("3"))
          {
            scanCount[3]++;
            totalScanCount[3]++;
          }
        }
      }

      RefreshStats();
    }

    string ProcessScanHeader(IMsScan scan)
    {
      string tmp;
      string filter;

      string scanNumber;
      string startTime;
      string massAnalyzer = "Unknown";
      string polarity;
      string msOrder;
      string scanMode;
      string ionizationMode;
      string scanData;
      string injectTime;
      string tic;
      string basePeakIntensity;

      string header = "==== SCAN HEADER ====" + System.Environment.NewLine;

      if(!scan.TryHeader("Scan", out scanNumber)) scanNumber="";
      if (!scan.TryHeader("StartTime", out startTime)) startTime="";
      if (!scan.TryHeader("MassAnalyzer", out tmp)) tmp="";
      if (tmp.Contains("FTMS")) massAnalyzer = "FTMS";
      else if (tmp.Contains("I")) massAnalyzer = "ITMS";
      if (!scan.TryHeader("Polarity", out polarity)) polarity="";
      if (!scan.TryHeader("MSOrder", out msOrder)) msOrder="";
      if (!scan.TryHeader("ScanMode", out scanMode)) scanMode="";
      if (!scan.TryHeader("IonizationMode", out ionizationMode)) ionizationMode="";
      if (!scan.TryHeader("ScanData", out scanData)) scanData="";
      if (!scan.TryHeader("InjectTime", out injectTime)) injectTime="";
      if (!scan.TryHeader("TIC", out tic)) tic="";
      if (!scan.TryHeader("BasePeakIntensity", out basePeakIntensity)) basePeakIntensity="";

      header += "Scan Number: " + scanNumber + System.Environment.NewLine;
      header += "Start Time: " + startTime + System.Environment.NewLine;
      if(massAnalyzer=="FTMS") header += "Mass Analyzer: Orbitrap" + System.Environment.NewLine;
      else if(massAnalyzer=="ITMS") header += "Mass Analyzer: Ion Trap" + System.Environment.NewLine;
      else header += "Mass Analyzer: Unknown" + System.Environment.NewLine;   
      header += "Polarity: " + polarity + System.Environment.NewLine;
      header += "Scan Level: " + msOrder + System.Environment.NewLine;
      header += "Scan Mode: " + scanMode + System.Environment.NewLine;
      header += "Data Type: " + scanData + System.Environment.NewLine;
      header += System.Environment.NewLine + "---- Scan Statistics ----" + System.Environment.NewLine;
      header += "Injection Time: " + injectTime + " ms" + System.Environment.NewLine;
      header += "Total Ion Current: " + tic + System.Environment.NewLine;
      header += "Base Peak Intensity: " + basePeakIntensity + System.Environment.NewLine;

      filter = massAnalyzer;
      if (polarity == "Positive") filter += " +";
      else filter += " -";     
      if (scanData == "Profile") filter += " p";
      else filter += " c";
      if (ionizationMode.Contains('N')) filter += " NSI";
      else filter += " ESI?";

      rtbHeader.Text = header;
      return filter;
    }

    private void RefreshStats()
    { 
      string a = String.Format("{0}{1,10}", scanCount[0], totalScanCount[0]);
      string b = String.Format("{0}{1,10}", scanCount[1], totalScanCount[1]);
      string c = String.Format("{0}{1,10}", scanCount[2], totalScanCount[2]);
      string d = String.Format("{0}{1,10}", scanCount[3], totalScanCount[3]);
      labelStats.Text = String.Format("{0}\n{1}\n{2}\n{3}", a, b, c, d);
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
      Log("Acquisition State Chaged: " + e.State);
    }

    /// <summary>
    /// Start listening for spectra.
    /// </summary>
    /// <returns>True on success or if listener is already running.</returns>
    bool StartListening()
    {
      if (!listener)
      {
        if (msIA != null)
        {
          msControl = msIA.Control;
          msMSSC = msIA.GetMsScanContainer(0);
          msMSSC.MsScanArrived += MsScanArrived;

          msAcquisition = msControl.Acquisition;
          msAcquisition.StateChanged += StateChanged;
          msAcquisition.AcquisitionStreamOpening += AcquisitionStreamOpening;
          msAcquisition.AcquisitionStreamClosing += AcquisitionStreamClosing;

          msScans = msControl.GetScans(false);

          return true;
        }
        return false;
      }
      return listener;
    }

    void StopListening()
    {
      if (listener)
      {
        if (msAcquisition != null)
        {
          msAcquisition.AcquisitionStreamClosing -= AcquisitionStreamClosing;
          msAcquisition.AcquisitionStreamOpening -= AcquisitionStreamOpening;
          msAcquisition.StateChanged -= StateChanged;
          msAcquisition = null;
        }

        if (msMSSC != null)
        {
          msMSSC.MsScanArrived -= MsScanArrived;
          msMSSC = null;
        }

        //msControl = null;
      }
      listener = false;
    }

    /// <summary>
    /// Updates the connection button and status to whatever state the application
    /// is in when this is called. Useful if ever having to temporarily disable
    /// the connection button at any point, and need it to be later re-enabled regardless
    /// of current state.
    /// </summary>
    private void UpdateConnection()
    {
      if (msIAC != null)
      {
        connectionIndicator.BackColor = msIAC.ServiceConnected ? System.Drawing.Color.Lime : System.Drawing.Color.Gray;
        disconnectionIndicator.BackColor = msIAC.ServiceConnected ? System.Drawing.Color.Gray : System.Drawing.Color.Red;
        buttonConnect.Text = msIAC.ServiceConnected ? "Disconnect" : "Connect";
        if (msIAC.ServiceConnected)
        {
          toolStripStatusLabel1.Text = "Connected";
          if (msIA != null) toolStripStatusLabel1.Text += ": " + msIA.InstrumentName;
        }
        else
        {
          toolStripStatusLabel1.Text = "Not Connected";
        }
        connected = msIAC.ServiceConnected ? true : false;
      }
      else
      {
        connectionIndicator.BackColor = System.Drawing.Color.Gray;
        disconnectionIndicator.BackColor = System.Drawing.Color.Red;
        buttonConnect.Text = "Connect";
        toolStripStatusLabel1.Text = "Not Connected";
        connected = false;
      }
      buttonConnect.Enabled = true;  //maybe disallow connection button while listener is activated?

      //do the same with the Listener button, which is inherently tied to the Connection button.
      UpdateListener();
    }

    /// <summary>
    /// Updates the listen button and status to whatever state the application
    /// is in when this is called. Useful if ever having to temporarily disable
    /// the listen button at any point, and need it to be later re-enabled regardless
    /// of current state. Note that a call to UpdateConnection() automatically calls
    /// this function.
    /// </summary>
    private void UpdateListener()
    {
      if (msIAC == null || !msIAC.ServiceConnected)
      {
        buttonListen.Text = "Activate";
        buttonListen.Enabled = false;
        listenIndicatorOn.BackColor = System.Drawing.Color.Gray;
        listenIndicatorWait.BackColor = System.Drawing.Color.Gray;
        listenIndicatorOff.BackColor = System.Drawing.Color.Red;
        toolStripStatusLabel2.Text = "Status: Idle";
        labelScanSpeed.Text = "0 Hz";
      }
      else
      {
        buttonListen.Enabled = true;
        if (listener)
        {
          buttonListen.Text = "Pause";
          listenIndicatorOn.BackColor = cbOnAcquisition.Checked ? System.Drawing.Color.Gray : System.Drawing.Color.Lime;
          listenIndicatorWait.BackColor = cbOnAcquisition.Checked ? System.Drawing.Color.Yellow : System.Drawing.Color.Gray;
          listenIndicatorOff.BackColor = System.Drawing.Color.Gray;
          ignoreScan = cbOnAcquisition.Checked ? true : false;
          toolStripStatusLabel2.Text = "Status: Spying";
        }
        else
        {
          buttonListen.Text = "Activate";
          listenIndicatorOn.BackColor = System.Drawing.Color.Gray;
          listenIndicatorWait.BackColor = System.Drawing.Color.Gray;
          listenIndicatorOff.BackColor = System.Drawing.Color.Red;
          toolStripStatusLabel2.Text = "Status: Idle";
          labelScanSpeed.Text = "0 Hz";
        }
      }
    }

    private void buttonListen_Click(object sender, EventArgs e)
    {
      buttonListen.Enabled = false;

      //spy is not active, so start spying
      if (!listener)
      {
        listener = StartListening();
      }

      //spy is active, so stop spying
      else
      {
        StopListening();
      }

      UpdateListener();
    }

    private void cbOnAcquisition_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void groupBox2_Enter(object sender, EventArgs e)
    {

    }

    private void label5_Click(object sender, EventArgs e)
    {

    }

    private void label4_Click(object sender, EventArgs e)
    {

    }

    private void label3_Click(object sender, EventArgs e)
    {

    }

    private void listenIndicatorOn_Click(object sender, EventArgs e)
    {

    }

    private void listenIndicatorWait_Click(object sender, EventArgs e)
    {

    }

    private void listenIndicatorOff_Click(object sender, EventArgs e)
    {

    }
  }

}
