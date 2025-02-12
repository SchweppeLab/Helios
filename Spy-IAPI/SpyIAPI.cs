using System;
using System.Runtime.InteropServices;
using System.Text;
using ScottPlot;
using ScottPlot.Plottables;
using UIAPI.Interfaces;
using UIAPI.Interfaces.InstrumentAccess;
using UIAPI.Interfaces.InstrumentAccess.Control;
using UIAPI.Interfaces.InstrumentAccess.Control.Acquisition;
using UIAPI.Interfaces.InstrumentAccess.MsScanContainer;


namespace Spy_IAPI
{
  //public static IntPtr FindWindow(string windowName)
  //{
  //  var hWnd = FindWindow(windowName, null);
  //  return hWnd;
  //}

  public partial class SpyIAPI : Form
  {

    private IUInstrumentAccessContainer? msIAC;
    private IUInstrumentAccess? msIA;
    private IUControl? msControl;
    private IUAcquisition? msAcquisition;
    private IUMsScanContainer? msMSSC;

    int[] scanCount = new int[4];
    bool connected = false;
    bool listener = false;
    bool ignoreScan = false;

    readonly Scatter plotSpec;
    private long lastTicks = 0;
    private volatile bool refreshSpectrum = false;
    //private VMsStats stats = new VMsStats();

    private int curSimCount = 0;
    private double curSimRT = 0;
    private TimeSpan allSimTime = TimeSpan.Zero;
    readonly System.Windows.Forms.Timer UpdatePlotTimer = new() { Interval = 100, Enabled = true };

    bool bTemp = false;

    public SpyIAPI()
    {
      InitializeComponent();

      Coordinates[] co = { new (0, 0) };
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

    void AcquisitionErrorsArrived(object? sender, AcquisitionErrorsArrivedEventArgs e)
    {
      Log("Acquisition Error: " + e.ToString());
    }

    private void AcquisitionStreamClosing(object? sender, EventArgs e)
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

    private void AcquisitionStreamOpening(object? sender, AcquisitionOpeningEventArgs e)
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

    void ConnectionChanged(object? sender, EventArgs e)
    {
      if (msIA != null)
      {
        Log(msIA.Connected.ToString() + " :: " + e.ToString());
      }
    }

    void ContactClosureChanged(object? sender, ContactClosureEventArgs e)
    {
      Log("Contact Closure changed: " + e.ToString() + " " + e.DidRise.ToString() + " " + e.DidFall.ToString());
    }

    void Log(string s)
    {
      rtbLog.AppendText(s + System.Environment.NewLine);
    }

    void MessagesArrived(object? sender, MessagesArrivedEventArgs e)
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

    private void MsScanArrived(object? sender, MsScanEventArgs e)
    {
      if (ignoreScan) return;

      using (IUMsScan msScan = e.GetScan())
      {
        if (!bTemp)
        {
          bTemp = true;
          foreach(string s in msScan.Trailer.ItemNames)
          {
            Log(s);
            string tmp;
            msScan.Trailer.TryGetValue(s, out tmp);
            if(tmp!=null) Log(tmp);
          }
        }
        //string str = "Scan ";
        //string? tmp;
        //msScan.Header.TryGetValue("ScanNumber", out tmp);
        //if (tmp != null) str += tmp;
        //str += " MsLevel: ";
        //msScan.Header.TryGetValue("MSOrder", out tmp);
        //if (tmp != null) str += tmp;
        //Log(str);

        long curTicks = DateTime.Now.Ticks;
        long tickDif = curTicks - lastTicks;
        if (tickDif > 1e6)
        {
          double rt = 0;
          int scanNumber = 0;
          string scanFilter = "";
          string? tmp;
          msScan.Header.TryGetValue("ScanNumber", out tmp);
          if (tmp != null) scanNumber = Convert.ToInt32(tmp);
          msScan.Header.TryGetValue("RetentionTime", out tmp);
          if (tmp != null) rt = Convert.ToDouble(tmp);
          msScan.Header.TryGetValue("Filter", out tmp);
          if (tmp != null) scanFilter = tmp;

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
      using (IUMsScan msScan = e.GetScan())
      {
        string? tmp;
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

    void ServiceConnectionChanged(object? sender, EventArgs e)
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

    private void StateChanged(object? sender, StateChangedEventArgs e)
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
  }
}
