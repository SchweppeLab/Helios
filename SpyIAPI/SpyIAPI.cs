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


namespace SpyIAPI
{
  public partial class SpyIAPI : Form
  {
    private IHeliosInstrumentAccessContainer msIAC;
    private IHeliosInstrumentAccess msIA;
    private IHeliosControl msControl;
    private IHeliosAcquisition msAcquisition;
    private IHeliosMsScanContainer msMSSC;
    private IHeliosScans msScans;

    int[] scanCount = new int[4];
    bool connected = false;
    bool listener = false;
    bool ignoreScan = false;

    bool[] headers = new bool[3];

    readonly Scatter plotSpec;
    private long lastTicks = 0;
    private volatile bool refreshSpectrum = false;
    //private VMsStats stats = new VMsStats();

    private int curSimCount = 0;
    private double curSimRT = 0;
    private TimeSpan allSimTime = TimeSpan.Zero;
    readonly System.Windows.Forms.Timer UpdatePlotTimer = new System.Windows.Forms.Timer();

    bool bTemp = false;

    public SpyIAPI()
    {
      InitializeComponent();

      UpdatePlotTimer.Interval = 100;
      UpdatePlotTimer.Enabled = true;

      Coordinates[] co = new Coordinates[1];
      co[0].X = 0;
      co[0].Y = 0;

      plotSpec = plotSpectrum.Plot.Add.Scatter(co);
      plotSpec.MarkerSize = 1;
      plotSpectrum.Plot.Axes.SetupMultiplierNotation(plotSpectrum.Plot.Axes.Left);
      plotSpectrum.Plot.YLabel("Intensity");
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
          break;
        }
      }

      if (!match)
      {
        int index = dgv.Rows.Add();
        dgv.Rows[index].Cells[0].Value = key;
        dgv.Rows[index].Cells[1].Value = msLevel.ToString();
        dgv.Rows[index].Cells[2].Value = value;
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
      //StringBuilder sb = new StringBuilder();
      //foreach (var message in e.Messages)
      //{
      //  string msg = string.Format("[{0}] ID: {1} Status: {2} Msg: {3}",
      //      message.CreationTime,
      //      message.MessageId,
      //      message.Status,
      //      string.Format(message.Message, message.MessageArgs));

      //  sb.AppendLine(msg);
      //}
      //Log(sb.ToString());
    }

    private void MsScanArrived(object sender, MsScanEventArgs e)
    {
      if (ignoreScan) return;

      using (IHeliosMsScan msScan = e.GetScan())
      {
        string tmp;
        int msLevel=0;
        if (msScan.TryHeader("MSOrder", out tmp)) msLevel = Convert.ToInt32(tmp);
        if (msLevel>0 && !headers[msLevel]) //first time we've seen this scan level, so process the headers available to the user
        {
          headers[msLevel] = true;
          foreach (var x in msScan.Header) CheckDictionary(x.Key.ToString(), x.Value.ToString(), msLevel, dgvHeaders);
          foreach (string x in msScan.Trailer.ItemNames)
          {
            if (msScan.Trailer.TryGetValue(x, out tmp))
            {
              CheckDictionary(x, tmp, msLevel, dgvTrailers);
            }
          }
        }

        long curTicks = DateTime.Now.Ticks;
        long tickDif = curTicks - lastTicks;
        if (tickDif > 1e6)
        {
          double rt = 0;
          int scanNumber = 0;
          string scanFilter = "";

          if(msScan.TryHeader("ScanNumber", out tmp)) scanNumber =Convert.ToInt32(tmp);
          if(msScan.TryHeader("RetentionTime", out tmp)) rt = Convert.ToDouble(tmp);

          msScan.Header.TryGetValue("Filter", out tmp);
          if (tmp != null) scanFilter = tmp;


          scanFilter = msScan.Centroids.Count().ToString() + " or " + msScan.CentroidCount.ToString();
          double[] x = new double[msScan.Centroids.Count()];
          double[] y = new double[msScan.Centroids.Count()];
          int a = 0;
          foreach (var centroid in msScan.Centroids)
          {
            x[a] = centroid.Mz;
            y[a] = centroid.Intensity;
            a++;
          }
          lock (plotSpectrum.Plot.Sync)
          {
            plotSpectrum.Plot.Clear();
            if (msScan.CentroidCount != null)
            {
              var bar = plotSpectrum.Plot.Add.Bars(x, y);
            }
            else
            {
              var scat = plotSpectrum.Plot.Add.Scatter(x, y);
              scat.MarkerSize = 1;
            }

            plotSpectrum.Plot.Axes.AutoScale();
          }

          lblScanFilter.Text = scanFilter;
          lblScanInfo.Text = "Scan #" + scanNumber.ToString() + "  RT:" + rt.ToString();
          refreshSpectrum = true;
          lastTicks = curTicks;
        }

      }
      return;
      using (IHeliosMsScan msScan = e.GetScan())
      {
        string tmp;
        msScan.Header.TryGetValue("MSOrder", out tmp);
        //richTextBox1.AppendText("MSOrder: " + tmp + System.Environment.NewLine);
        if (tmp != null)
        {
          if (tmp.Equals("MS2") || tmp.Equals("2"))
          {
            scanCount[2]++;
            //lblMSMSCount.Text = ms2Count.ToString();
          }
          else if (tmp.Equals("MS") || tmp.Equals("1"))
          {
            scanCount[1]++;
            //lblMSCount.Text = msCount.ToString();
          }
        }
      }

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
        connected = msIAC.ServiceConnected ? true : false;
      }
      else
      {
        connectionIndicator.BackColor = System.Drawing.Color.Gray;
        disconnectionIndicator.BackColor = System.Drawing.Color.Red;
        buttonConnect.Text = "Connect";
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
        }
        else
        {
          buttonListen.Text = "Activate";
          listenIndicatorOn.BackColor = System.Drawing.Color.Gray;
          listenIndicatorWait.BackColor = System.Drawing.Color.Gray;
          listenIndicatorOff.BackColor = System.Drawing.Color.Red;
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

    private void button1_Click(object sender, EventArgs e)
    {
      IHeliosCustomScan ecs = msScans.CreateCustomScan();

      ecs.SingleProcessingDelay = 0;
      ecs.RunningNumber = 1;
      foreach(var property in msScans.PossibleParameters)
      {
        Log("Params: '" + property.Name + "' = " + property.Help + " Default=" + property.DefaultValue + " Selection=" + property.Selection);
      }
    }
  }

}
