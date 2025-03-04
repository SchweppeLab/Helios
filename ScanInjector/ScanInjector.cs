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

    bool connected = false;
    DataTable scanProperties;


    public ScanInjector()
    {
      InitializeComponent();
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

    void OnPossibleParametersChanged(object sender, EventArgs e)
    {
      Log("Possible parameters changed.");
      if(msScans!=null) PopulateCustomScanParams();
      Log("Possible parameters updated.");
    }

    void PopulateCustomScanParams()
    {
      scanProperties = new DataTable();
      scanProperties.Columns.Add("Property", typeof(string)).ReadOnly = true;
      scanProperties.Columns.Add("Value", typeof(string));
      scanProperties.Columns.Add("Range", typeof(string)).ReadOnly = true;
      scanProperties.Columns.Add("Help", typeof(string)).ReadOnly = true;

      BindingSource source = new BindingSource();
      source.DataSource = scanProperties;

      dgvScan.DataSource = source;

      foreach (var property in msScans.PossibleParameters)
      {
        scanProperties.LoadDataRow(new string[] { property.Name, property.DefaultValue, property.Selection, property.Help }, LoadOption.OverwriteChanges);
      }

      Invoke((MethodInvoker)delegate
      {
        dgvScan.ScrollBars = ScrollBars.Both;
      });
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

            msControl = msIA.Control;
            msMSSC = msIA.GetMsScanContainer(0);
            //msMSSC.MsScanArrived += MsScanArrived;

            msAcquisition = msControl.Acquisition;
            //msAcquisition.StateChanged += StateChanged;
            msAcquisition.AcquisitionStreamOpening += AcquisitionStreamOpening;
            msAcquisition.AcquisitionStreamClosing += AcquisitionStreamClosing;

            msScans = msControl.GetScans(false);
            msScans.PossibleParametersChanged += OnPossibleParametersChanged;
          }
          catch (Exception ex)
          {
            Log("Failed to get instrument access: " + ex.Message);
          }

        }
      }

      UpdateConnection();

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
    }

    private void buttonConnect_Click(object sender, EventArgs e)
    {
      //Step #1, disable the buttons so that they can't be clicked while this
      //function is executing.
      buttonConnect.Enabled = false;

      //Step #2, if connected, then start the disconnect procedure.
      if (connected)
      {
        Log("Attempting graceful disconnect...");
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

  }
}
