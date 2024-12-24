using System.Text;
using UIAPI.Interfaces;
using UIAPI.Interfaces.InstrumentAccess;
using UIAPI.Interfaces.InstrumentAccess.Control;
using UIAPI.Interfaces.InstrumentAccess.Control.Acquisition;
using UIAPI.Interfaces.InstrumentAccess.MsScanContainer;

namespace Spy_IAPI
{
  public partial class Form1 : Form
  {

    IUInstrumentAccessContainer msIAC;
    IUInstrumentAccess msIA;
    IInstControl msControl;
    IInstAcquisition msAcquisition;
    IInstMsScanContainer msMSSC;

    int[] scanCount = new int[4];
    bool connected = false;
    bool listener = false;

    public Form1()
    {
      InitializeComponent();
    }

    private void buttonConnect_Click(object sender, EventArgs e)
    {
      buttonConnect.Enabled = false;
      buttonListen.Enabled = false;
      if (connected)
      {
        if (listener) StopListening();
        msIA.AcquisitionErrorsArrived -= AcquisitionErrorsArrived;
        msIA.ConnectionChanged -= ConnectionChanged;
        msIA.ContactClosureChanged -= ContactClosureChanged;
        msIA = null;
        msIAC.Dispose();
        msIAC = null;
        Log("Disconnected");
      }
      else
      {
        try
        {
          msIAC = InstrumentAccessContainerFactory.Get();
          if (msIAC == null)
          {
            Log("Failed to connect to instrument.");
          }
          else
          {
            Log("Instrument ID: " + msIAC.InstrumentType());
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
      UpdateConnection();
    }

    void AcquisitionErrorsArrived(object sender, AcquisitionErrorsEventArgs e)
    {
      Log("Acquisition Error: " + e.ToString());
    }

    private void AcquisitionStreamClosing(object sender, EventArgs e)
    {
      Log("AcquisitionStreamClosing.");
    }

    private void AcquisitionStreamOpening(object sender, AOEventArgs e)
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

    void ConnectionChanged(object sender, EventArgs e)
    {
      Log(msIA.Connected().ToString() + " :: " + e.ToString());
    }

    void ContactClosureChanged(object sender, CCEventArgs e)
    {
      Log("Contact Closure changed: " + e.ToString() + " " + e.DidRise.ToString() + " " + e.DidFall.ToString());
    }

    void Log(string s)
    {
      rtbLog.AppendText(s + System.Environment.NewLine);
    }

    void MessagesArrived(object sender, MessageEventArgs e)
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

    private void MsScanArrived(object sender, IMSEventArgs e)
    {
      //richTextBox1.AppendText("MSSC_MsScanArrived." + System.Environment.NewLine);
      using (IUMsScan msScan = e.GetScan())
      {
        string tmp;
        msScan.Header.TryGetValue("MSOrder", out tmp);
        //richTextBox1.AppendText("MSOrder: " + tmp + System.Environment.NewLine);
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

    void ServiceConnectionChanged(object sender, EventArgs e)
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

      UpdateConnection();

    }

    private void StateChanged(object sender, SCEventArgs e)
    {
      Log("Acquisition State Chaged: " + e.State);
    }

    void StopListening()
    {
      if (listener)
      {
        msAcquisition.AcquisitionStreamClosing -= AcquisitionStreamClosing;
        msAcquisition.AcquisitionStreamOpening -= AcquisitionStreamOpening;
        msAcquisition.StateChanged -= StateChanged;
        //msAcquisition = null;

        msMSSC.MsScanArrived -= MsScanArrived;
        //msMSSC = null;

        //msControl = null;
      }
      listener = false;
    }

    private void UpdateConnection()
    {
      buttonConnect.Text = connected ? "Disconnect" : "Connect";
      buttonConnect.Enabled = true;  //maybe disallow connection button while listener is activated?

      UpdateListener();
    }

    private void UpdateListener()
    {
      buttonListen.Enabled = connected ? true : false;
    }

  }
}
