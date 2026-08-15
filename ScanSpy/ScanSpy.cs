using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ScottPlot.Plottables;

using Helios.Client;

namespace ScanSpy
{
#if USE_LANDMINE_UI
  public partial class ScanSpy : LandmineUI.WinForms.SharpWindow
#else
  public partial class ScanSpy : Form
#endif
  {
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

    private int curSimCount = 0;
    private double curSimRT = 0;
    private TimeSpan allSimTime = TimeSpan.Zero;
    readonly System.Windows.Forms.Timer UpdatePlotTimer = new System.Windows.Forms.Timer();

    bool bTemp = false;

    public ScanSpy()
    {
      InitializeComponent();

#if USE_LANDMINE_UI
      // SharpWindow themes ContentArea.BackColor from the active theme but leaves ForeColor at
      // its WinForms default (near-black SystemColors.ControlText) -- every plain WinForms control
      // still living in this window (Label, GroupBox, ...) reads BackColor/ForeColor ambiently
      // from its parent, so without this, their text stays dark-on-dark against the now-dark
      // background. Setting it once here cascades to all of them.
      var landmineTheme = LandmineUI.WinForms.Theming.ThemeManager.Current;
      this.ContentArea.ForeColor = landmineTheme.TextPrimary;
#endif

      UpdatePlotTimer.Interval = 100;
      UpdatePlotTimer.Enabled = true;

      ScottPlot.Coordinates[] co = new ScottPlot.Coordinates[1];
      co[0].X = 0;
      co[0].Y = 0;

      plotSpec = plotSpectrum.Plot.Add.Scatter(co);
      plotSpec.MarkerSize = 0;
      plotSpectrum.Plot.Axes.SetLimitsY(0, 100);
      plotSpectrum.Plot.YLabel("Relative Intensity");
      plotSpectrum.Plot.XLabel("m/z");
      plotSpectrum.Plot.HideGrid();
#if USE_LANDMINE_UI
      // ScottPlot has no awareness of LandmineUI's theme system at all (it's a separate library),
      // so without this the plot stays permanently white/light regardless of which LandmineUI
      // theme is active.
      plotSpectrum.Plot.FigureBackground.Color = ScottPlot.Color.FromSDColor(landmineTheme.Surface);
      plotSpectrum.Plot.DataBackground.Color = ScottPlot.Color.FromSDColor(landmineTheme.Background);
      plotSpectrum.Plot.Axes.Color(ScottPlot.Color.FromSDColor(landmineTheme.TextSecondary));
      plotSpectrum.Plot.Axes.FrameColor(ScottPlot.Color.FromSDColor(landmineTheme.Border));
#endif
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

    // Marshals onto the UI thread when needed -- IInstrumentAccess's events (unlike the
    // in-process Helios API ScanSpy used to talk to directly) are always raised from a background
    // Task draining a gRPC stream, never the UI thread.
    private void UiInvoke(Action action)
    {
      if (IsHandleCreated && InvokeRequired) BeginInvoke(action);
      else action();
    }

    private async void buttonConnect_Click(object sender, EventArgs e)
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
          msIA.ConnectionChanged -= ConnectionChanged;
          msIA.ContactClosureChanged -= ContactClosureChanged;
          msIA.MessagesArrived -= MessagesArrived;
          await msIA.DisposeAsync();
          msIA = null;
        }
        connected = false;
        Log("Disconnected");
      }

      //Step #2b, not connected, so start the connection procedure.
      else
      {
        try
        {
          Log("Attempting to connect to Helios.Bridge.Host at 127.0.0.1:50100.");

          msIA = await HeliosClient.ConnectAsync();
          Log("Instrument family: " + msIA.Family);

          msIA.ConnectionChanged += ConnectionChanged;
          msIA.ContactClosureChanged += ContactClosureChanged;
          msIA.MessagesArrived += MessagesArrived;

          connected = true;
          Log("Connected to " + msIA.InstrumentName);
        }
        catch (Exception ex)
        {
          Log("HeliosClient.ConnectAsync() " + ex.Message);
          msIA = null;
          connected = false;
        }
      }

      //Step #3: Update the user iterface (with whatever results happened here).
      UpdateConnection();
    }

    private void AcquisitionStreamClosing(object sender, EventArgs e)
    {
      UiInvoke(() =>
      {
        Log("AcquisitionStreamClosing.");
        if (listener && cbOnAcquisition.Checked)
        {
          listenIndicatorOn.BackColor = Color.Gray;
          listenIndicatorWait.BackColor = Color.Yellow;
          listenIndicatorOff.BackColor = Color.Gray;
          ignoreScan = true;
        }
      });
    }

    private void AcquisitionStreamOpening(object sender, AcquisitionStreamOpeningEventArgs e)
    {
      UiInvoke(() =>
      {
        ignoreScan = false;
        listenIndicatorOn.BackColor = Color.Lime;
        listenIndicatorWait.BackColor = Color.Gray;
        listenIndicatorOff.BackColor = Color.Gray;

        string str = "AcquisitionStreamOpening: ";
        foreach (var de in e.StartingInformation)
        {
          if (de.Key.CompareTo("RawFileName") == 0 || de.Key.CompareTo("RawFile") == 0)
          {
            str += de.Value;
          }
        }
        Log(str);
      });
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
      Log("Contact Closure changed: RisingEdges=" + e.RisingEdges + " FallingEdges=" + e.FallingEdges);
    }

    void Log(string s)
    {
#if USE_LANDMINE_UI
      // SharpTextArea has no AppendText -- it's a themed control built around a plain Text
      // setter, not RichTextBox's incremental-append API.
      UiInvoke(() => rtbLog.Text += s + System.Environment.NewLine);
#else
      UiInvoke(() => rtbLog.AppendText(s + System.Environment.NewLine));
#endif
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    private const int EM_GETFIRSTVISIBLELINE = 0x00CE;
    private const int EM_LINESCROLL = 0x00B6;

    // Replacing rtbHeader.Text wholesale on every refresh (~10Hz) resets the scroll position to
    // the top, so scrolling down to read a field mid-run was pointless -- the very next scan
    // yanked the view back. The header block has the same line count every refresh (same fixed
    // set of fields), so restoring by line number exactly undoes that, unlike SelectionStart+
    // ScrollToCaret which only guarantees the caret is *somewhere* visible.
    //
    // SharpTextArea (the LandmineUI equivalent, USE_LANDMINE_UI build) had no scroll accessors at
    // all as of v1.1.0 -- filed upstream as a wish, granted in v1.2.0 as
    // SetTextPreservingScroll(string?), a thin wrapper over the same
    // capture-line/replace-text/restore-line approach used below for the plain RichTextBox.
    void SetHeaderText(string text)
    {
#if USE_LANDMINE_UI
      rtbHeader.SetTextPreservingScroll(text);
#else
      int firstVisibleLine = 0;
      if (rtbHeader.IsHandleCreated)
        firstVisibleLine = (int)SendMessage(rtbHeader.Handle, EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero);

      rtbHeader.Text = text;

      // Scroll by the delta between where we ended up and where we want to be, not by the
      // pre-change line number directly -- WM_SETTEXT doesn't reliably reset the view to line 0,
      // so assuming it did (scrolling forward by the old absolute line number every time) came up
      // one line short whenever the post-Text-set position wasn't exactly 0, and that shortfall
      // compounded every refresh into a slow drift back to the top.
      if (firstVisibleLine > 0 && rtbHeader.IsHandleCreated)
      {
        int newFirstVisibleLine = (int)SendMessage(rtbHeader.Handle, EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero);
        int delta = firstVisibleLine - newFirstVisibleLine;
        if (delta != 0)
          SendMessage(rtbHeader.Handle, EM_LINESCROLL, IntPtr.Zero, (IntPtr)delta);
      }
#endif
    }

    void MessagesArrived(object sender, MessagesArrivedEventArgs e)
    {
      StringBuilder sb = new StringBuilder();
      foreach (var message in e.Messages)
      {
        string msg = string.Format("[{0}] ID: {1} Status: {2} Msg: {3}",
            message.CreationTimeUtc,
            message.MessageId,
            message.Status,
            string.Format(message.Message, message.MessageArgs.ToArray()));

        sb.AppendLine(msg);
      }
      Log(sb.ToString());
    }

    private void MsScanArrived(object sender, MsScanEventArgs e)
    {
      if (ignoreScan) return;

      string tmp;
      IMsScan msScan = e.Scan;

      double rt = 0;
      if (msScan.Header.TryGetValue("StartTime", out tmp)) rt = Convert.ToDouble(tmp);
      scanHistory.Enqueue(rt);
      while (scanHistory.Count() > 0 && rt - scanHistory.First() > 0.1)
      {
        scanHistory.Dequeue();
      }
      double hz = scanHistory.Count() * 0.1 / (rt - scanHistory.First()) / 6;

      long curTicks = DateTime.Now.Ticks;
      long tickDif = curTicks - lastTicks;
      bool refreshNow = tickDif > 1e6; //only refresh at intervals

      int scanNumber = 0;
      double firstMass = 0;
      double lastMass = 100;
      double basePeakIntensity = 0;
      bool isCentroid = false;
      double[] x = null;
      double[] y = null;
      string scanFilter = null;
      string header = null;
      string msOrderForColor = null;

      if (refreshNow)
      {
        msScan.Header.TryGetValue("MSOrder", out msOrderForColor);
        if (msScan.Header.TryGetValue("Scan", out tmp)) scanNumber = Convert.ToInt32(tmp);
        if (msScan.Header.TryGetValue("FirstMass", out tmp)) firstMass = Convert.ToDouble(tmp);
        if (msScan.Header.TryGetValue("LastMass", out tmp)) lastMass = Convert.ToDouble(tmp);
        if (msScan.Header.TryGetValue("BasePeakIntensity", out tmp)) basePeakIntensity = Convert.ToDouble(tmp);
        if (msScan.Header.TryGetValue("ScanData", out tmp))
        {
          if (tmp == "Centroid") isCentroid = true;
        }

        scanFilter = ProcessScanHeader(msScan, out header);

        var centroids = msScan.Centroids;
        int n = centroids.Mz.Length;
        int a = 0;
        if (isCentroid)
        {
          x = new double[n * 3 + 2];
          y = new double[n * 3 + 2];
          x[a] = firstMass;
          y[a++] = 0;
          for (int j = 0; j < n; j++)
          {
            double cmz = centroids.Mz[j];
            double cintensity = centroids.Intensity[j];
            //centroided peaks need to be plotted with 3 points
            x[a] = cmz;
            y[a++] = 0;
            x[a] = cmz;
            y[a++] = basePeakIntensity == 0 ? 0 : cintensity / basePeakIntensity * 100;
            x[a] = cmz;
            y[a++] = 0;
          }
          x[a] = lastMass;
          y[a] = 0;
        }
        else
        {
          x = new double[n + 2];
          y = new double[n + 2];
          x[a] = firstMass;
          y[a++] = 0;
          for (int j = 0; j < n; j++)
          {
            x[a] = centroids.Mz[j];
            y[a] = basePeakIntensity == 0 ? 0 : centroids.Intensity[j] / basePeakIntensity * 100;
            a++;
          }
          x[a] = lastMass;
          y[a] = 0;
        }
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

      // Only queue a UI update for the scans that pass the refreshNow throttle above (~10Hz), not
      // every single scan -- BeginInvoke doesn't block this (background) thread, but it does queue
      // onto the UI thread's own message loop, which is just as unbounded and un-droppable as
      // everything else in this pipeline. At real instrument scan rates well above 10Hz (e.g. DDA
      // with many MS2 events per MS1), invoking on every scan queued UI work faster than the UI
      // thread could drain it, so displayed values (including the Hz readout meant to show the
      // current rate) fell progressively further behind rather than settling at ~10Hz like the plot.
      if (refreshNow)
      {
        UiInvoke(() =>
        {
          labelScanSpeed.Text = hz.ToString("F1") + " Hz";

          lock (plotSpectrum.Plot.Sync)
          {
            plotSpectrum.Plot.Clear();
            var scat = plotSpectrum.Plot.Add.Scatter(x, y);
            // No markers -- with the centroid zero/peak/zero triples, a marker at every vertex
            // reads as a haze of dots along the baseline once more than a handful of peaks are in
            // view. The line alone already draws a clean spike per peak.
            scat.MarkerSize = 0;
            scat.LineWidth = 1.5f;
            scat.LineColor = ColorForMsOrder(msOrderForColor);
            plotSpectrum.Plot.Axes.SetLimitsX(firstMass, lastMass);
          }

          SetHeaderText(header);
          lblScanFilter.Text = scanFilter;
          lblScanInfo.Text = "Scan #" + scanNumber.ToString() + "  RT:" + rt.ToString("#.00") + "  NL:" + basePeakIntensity.ToString("E2");
          refreshSpectrum = true;
          lastTicks = curTicks;

          RefreshStats();
        });
      }
    }

    // Unlike Helios's own IMsScan.TryHeader, Helios.Client's IMsScan exposes plain resolved
    // dictionaries -- Helios.Bridge.Host's HeliosMsScanChannelAdapter already resolves the
    // canonical ("universal dictionary") IDs used below into Header/Trailer regardless of which
    // instrument family answered, so a straight TryGetValue by canonical name is enough here.
    string ProcessScanHeader(IMsScan scan, out string header)
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
      string firstMass;
      string lastMass;

      string h = "==== SCAN HEADER ====" + System.Environment.NewLine;

      if (!scan.Header.TryGetValue("Scan", out scanNumber)) scanNumber = "";
      if (!scan.Header.TryGetValue("StartTime", out startTime)) startTime = "";
      if (!scan.Header.TryGetValue("MassAnalyzer", out tmp)) tmp = "";
      if (tmp.Contains("FTMS")) massAnalyzer = "FTMS";
      else if (tmp.Contains("I")) massAnalyzer = "ITMS";
      if (!scan.Header.TryGetValue("Polarity", out polarity)) polarity = "";
      if (!scan.Header.TryGetValue("MSOrder", out msOrder)) msOrder = "";
      if (!scan.Header.TryGetValue("ScanMode", out scanMode)) scanMode = "";
      if (!scan.Header.TryGetValue("IonizationMode", out ionizationMode)) ionizationMode = "";
      if (!scan.Header.TryGetValue("ScanData", out scanData)) scanData = "";
      if (!scan.Header.TryGetValue("InjectTime", out injectTime)) injectTime = "";
      if (!scan.Header.TryGetValue("TIC", out tic)) tic = "";
      if (!scan.Header.TryGetValue("BasePeakIntensity", out basePeakIntensity)) basePeakIntensity = "";
      if (!scan.Header.TryGetValue("FirstMass", out firstMass)) firstMass = "";
      if (!scan.Header.TryGetValue("LastMass", out lastMass)) lastMass = "";

      h += "Scan Number: " + scanNumber + System.Environment.NewLine;
      h += "Start Time: " + startTime + System.Environment.NewLine;
      if (massAnalyzer == "FTMS") h += "Mass Analyzer: Orbitrap" + System.Environment.NewLine;
      else if (massAnalyzer == "ITMS") h += "Mass Analyzer: Ion Trap" + System.Environment.NewLine;
      else h += "Mass Analyzer: Unknown" + System.Environment.NewLine;
      h += "Polarity: " + polarity + System.Environment.NewLine;
      h += "Scan Level: " + msOrder + System.Environment.NewLine;
      h += "Scan Mode: " + scanMode + System.Environment.NewLine;
      h += "Data Type: " + scanData + System.Environment.NewLine;
      h += "M/Z Range: " + firstMass + " - " + lastMass + System.Environment.NewLine;
      h += System.Environment.NewLine + "---- Scan Statistics ----" + System.Environment.NewLine;
      h += "Injection Time: " + injectTime + " ms" + System.Environment.NewLine;
      h += "Total Ion Current: " + tic + System.Environment.NewLine;
      h += "Base Peak Intensity: " + basePeakIntensity + System.Environment.NewLine;

      // Prefer the authentic Thermo filter string when the connected instrument provides one.
      // Corona/VMS plays back real .raw files through RawFileReader, which computes this exactly
      // (Nova's ThermoRawReader sets Spectrum.ScanFilter = RawFile.GetFilterForScanNumber(...).ToString()
      // -- see Nova\NovaIO\Io\Read\ThermoRawReader.cs -- and HeliosMsScanVMS carries it through
      // under the raw "Filter" key). Live Exploris/Fusion acquisition via IAPI has no equivalent --
      // the real-time interface never exposes a filter string, only the individual fields below --
      // so those connections fall back to a reconstruction from what canonical header fields are
      // actually available. (The previous reconstruction here always ended in a literal "ESI?"
      // placeholder because VMS scans never populate "IonizationMode" at all, which is also why it
      // looked static regardless of scan -- using the real filter text sidesteps that entirely.)
      if (scan.Header.TryGetValue("Filter", out var rawFilter) && !string.IsNullOrEmpty(rawFilter))
      {
        filter = rawFilter;
      }
      else
      {
        filter = massAnalyzer;
        if (polarity == "Positive") filter += " +";
        else if (polarity == "Negative") filter += " -";
        if (scanData == "Profile") filter += " p";
        else if (scanData == "Centroid") filter += " c";
        if (!string.IsNullOrEmpty(ionizationMode)) filter += " " + ionizationMode;
        string scanModeText = string.IsNullOrEmpty(scanMode) ? "Full" : scanMode;
        filter += " " + scanModeText + " ms" + MsOrderSuffix(msOrder);
        if (!string.IsNullOrEmpty(firstMass) && !string.IsNullOrEmpty(lastMass))
          filter += " [" + firstMass + "-" + lastMass + "]";
      }

      header = h;
      return filter;
    }

    // Maps Helios's canonical MSOrder header values ("MS"/"1" for MS1, "MS2"/"2" for MS2, "3" for
    // MS3, etc. -- instrument families spell these differently, see HeliosDictionary) to the
    // suffix used in real Thermo filter text ("" for MS1, "2" for MS2, "3" for MS3, ...).
    static string MsOrderSuffix(string msOrder)
    {
      if (string.IsNullOrEmpty(msOrder) || msOrder.Equals("MS", StringComparison.OrdinalIgnoreCase)) return "";
      if (int.TryParse(msOrder, out int n)) return n <= 1 ? "" : n.ToString();
      if (msOrder.StartsWith("MS", StringComparison.OrdinalIgnoreCase) && msOrder.Length > 2) return msOrder.Substring(2);
      return "";
    }

    // Colors the trace by MS level so a glance at the plot tells you what kind of scan you're
    // looking at, without having to read the filter text above it.
    private static ScottPlot.Color ColorForMsOrder(string msOrder)
    {
      if (msOrder == "MS" || msOrder == "1") return ScottPlot.Colors.SteelBlue;
      if (msOrder == "MS2" || msOrder == "2") return ScottPlot.Colors.OrangeRed;
      if (msOrder == "3") return ScottPlot.Colors.MediumPurple;
      return ScottPlot.Colors.Gray;
    }

    private void RefreshStats()
    {
      string a = String.Format("{0}{1,10}", scanCount[0], totalScanCount[0]);
      string b = String.Format("{0}{1,10}", scanCount[1], totalScanCount[1]);
      string c = String.Format("{0}{1,10}", scanCount[2], totalScanCount[2]);
      string d = String.Format("{0}{1,10}", scanCount[3], totalScanCount[3]);
      labelStats.Text = String.Format("{0}\n{1}\n{2}\n{3}", a, b, c, d);
    }

    private void StateChanged(object sender, StateChangedEventArgs e)
    {
      Log("Acquisition State Changed: mode=" + e.State.Mode + " state=" + e.State.State);
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

          msScans = msControl.Scans;

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
      }
      listener = false;
    }

    // Abstracts stock StatusStrip's two ToolStripStatusLabel items vs. SharpStatusBar's flat
    // LeftText/RightText strings, so call sites don't need their own #if.
    void SetStatusLeft(string s)
    {
#if USE_LANDMINE_UI
      statusStrip1.LeftText = s;
#else
      toolStripStatusLabel1.Text = s;
#endif
    }

    void SetStatusRight(string s)
    {
#if USE_LANDMINE_UI
      statusStrip1.RightText = s;
#else
      toolStripStatusLabel2.Text = s;
#endif
    }

    /// <summary>
    /// Updates the connection button and status to whatever state the application
    /// is in when this is called. Useful if ever having to temporarily disable
    /// the connection button at any point, and need it to be later re-enabled regardless
    /// of current state.
    /// </summary>
    private void UpdateConnection()
    {
      if (msIA != null)
      {
        connectionIndicator.BackColor = msIA.Connected ? Color.Lime : Color.Gray;
        disconnectionIndicator.BackColor = msIA.Connected ? Color.Gray : Color.Red;
        buttonConnect.Text = "Disconnect";
        SetStatusLeft("Connected: " + msIA.InstrumentName);
        connected = true;
      }
      else
      {
        connectionIndicator.BackColor = Color.Gray;
        disconnectionIndicator.BackColor = Color.Red;
        buttonConnect.Text = "Connect";
        SetStatusLeft("Not Connected");
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
      if (msIA == null)
      {
        buttonListen.Text = "Activate";
        buttonListen.Enabled = false;
        listenIndicatorOn.BackColor = Color.Gray;
        listenIndicatorWait.BackColor = Color.Gray;
        listenIndicatorOff.BackColor = Color.Red;
        SetStatusRight("Status: Idle");
        labelScanSpeed.Text = "0 Hz";
      }
      else
      {
        buttonListen.Enabled = true;
        if (listener)
        {
          buttonListen.Text = "Pause";
          listenIndicatorOn.BackColor = cbOnAcquisition.Checked ? Color.Gray : Color.Lime;
          listenIndicatorWait.BackColor = cbOnAcquisition.Checked ? Color.Yellow : Color.Gray;
          listenIndicatorOff.BackColor = Color.Gray;
          ignoreScan = cbOnAcquisition.Checked ? true : false;
          SetStatusRight("Status: Spying");
        }
        else
        {
          buttonListen.Text = "Activate";
          listenIndicatorOn.BackColor = Color.Gray;
          listenIndicatorWait.BackColor = Color.Gray;
          listenIndicatorOff.BackColor = Color.Red;
          SetStatusRight("Status: Idle");
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
