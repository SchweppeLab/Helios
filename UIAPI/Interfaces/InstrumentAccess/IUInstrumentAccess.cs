extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIAPI.Interfaces.InstrumentAccess.Control;
using UIAPI.Interfaces.InstrumentAccess.MsScanContainer;

namespace UIAPI.Interfaces.InstrumentAccess
{

    /// <summary>
    /// Wrapper around IAPI IInstrumentAccess. IAPI Docs:<br/>
    /// This interface describes the access to one particular instrument both for reading data as for managing access or behaviour of the instrument.
    /// </summary>
    public interface IUInstrumentAccess
    {
        /// <summary>
        /// From IAPI Docs:<br/>
        /// Get access to the interface covering all control functionality of an instrument.<br/>
        /// This property is accessible offline.
        /// </summary>
        UIAPI.Interfaces.InstrumentAccess.Control.IUControl Control { get; }

        /// <summary>
        /// From IAPI Docs:<br/>
        /// This event will be thrown when at least one error arrived from the instrument during an acquisition. This event handler will not be used for status reports or messages of the transport layer.<br/>
        /// For regular messages see Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.MessagesArrived<br/>
        /// This event handler is accessible offline.
        /// </summary>
        event EventHandler<AcquisitionErrorsArrivedEventArgs> AcquisitionErrorsArrived;

        /// <summary>
        /// From IAPI Docs:<br/>
        /// This event will be fired when the connection to the instrument changes. Note that the initial connection state will be disconnected until a connection has been established to the instrument.<br/>
        /// This property is accessible offline.
        /// </summary>
        event EventHandler<EventArgs> ConnectionChanged;

        /// <summary>
        /// From IAPI Docs:<br/>
        /// Occurs whenever the contact closure detects a rising and/or falling edge.<br/>
        /// Only applies to Fusion instruments. Exploris instruments will not raise these events.
        /// </summary>
        event EventHandler<ContactClosureEventArgs> ContactClosureChanged;

        /// <summary>
        /// From IAPI Docs:<br/>
        /// Get access to the connection state of the instrument and of this driver to the communication layer. Most functionality is not available if the instrument is not connected.<br/>
        /// This property is accessible offline.<br/>
        /// See Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.StartOnlineAccess for initiating online access.
        /// </summary>
        /// <returns>true/false status of the underlying connection state of the instrument.</returns>
        bool Connected { get; }

        UIAPI.Interfaces.InstrumentAccess.MsScanContainer.IUMsScanContainer GetMsScanContainer(int msDetectorSet);

        int CountAnalogChannels { get; }
        int CountMsDetectors { get; }
        string[] DetectorClasses { get; }
        int InstrumentId { get; }
        string InstrumentName { get; }
    }

    class UInstrumentAccessExploris : IUInstrumentAccess
    {
        exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccess instAcc;
        public bool Connected { get; }
        public UIAPI.Interfaces.InstrumentAccess.Control.IUControl Control { get; }
        public int CountAnalogChannels { get; }
        public int CountMsDetectors { get; }
        public string[] DetectorClasses { get; }
        public int InstrumentId { get; }
        public string InstrumentName { get; }
        public event EventHandler<AcquisitionErrorsArrivedEventArgs> AcquisitionErrorsArrived;
        public event EventHandler<EventArgs> ConnectionChanged;
        public event EventHandler<ContactClosureEventArgs> ContactClosureChanged;

        public UInstrumentAccessExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccessContainer iac, int index)
        {
            instAcc = iac.Get(index);
            instAcc.AcquisitionErrorsArrived += AcquisitionErrorsArrivedExploris;
            instAcc.ConnectionChanged += ConnectionChangedExploris;
            //instAcc.ContactClosureChanged += ContactClosureChangedFusion; //not implemented in Exploris
            //Control = InstControlFactory.Get(instAcc);
            Connected = instAcc.Connected;
            Control = new UIAPI.Interfaces.InstrumentAccess.Control.UControlExploris(instAcc);
            CountAnalogChannels = instAcc.CountAnalogChannels;
            CountMsDetectors = instAcc.CountMsDetectors;
            DetectorClasses = instAcc.DetectorClasses;
            InstrumentId = instAcc.InstrumentId;
            InstrumentName = instAcc.InstrumentName;
        }

        void AcquisitionErrorsArrivedExploris(object sender, exploris.Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs e)
        {
            AcquisitionErrorsArrivedEventArgs args = new AcquisitionErrorsArrivedEventArgs(e);
            OnAcquisitionErrorsArrived(args);
        }

        void ConnectionChangedExploris(object sender, EventArgs e)
        {
            OnConnectionChanged(e);
        }

        public UIAPI.Interfaces.InstrumentAccess.MsScanContainer.IUMsScanContainer GetMsScanContainer(int msDetectorSet)
        {
            UIAPI.Interfaces.InstrumentAccess.MsScanContainer.IUMsScanContainer msCont = new UIAPI.Interfaces.InstrumentAccess.MsScanContainer.UMsScanContainerExploris(instAcc,msDetectorSet);
            return msCont;
        }

        protected virtual void OnAcquisitionErrorsArrived(AcquisitionErrorsArrivedEventArgs e)
        {
            EventHandler<AcquisitionErrorsArrivedEventArgs> handler = AcquisitionErrorsArrived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnConnectionChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = ConnectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnContactClosureChanged(ContactClosureEventArgs e)
        {
            EventHandler<ContactClosureEventArgs> handler = ContactClosureChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    class UInstrumentAccessFusion : IUInstrumentAccess
    {
        fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccess instAcc;
    public bool Connected { get; }
    public UIAPI.Interfaces.InstrumentAccess.Control.IUControl Control { get; }
        public int CountAnalogChannels { get; }
        public int CountMsDetectors { get; }
        public string[] DetectorClasses { get; }
        public int InstrumentId { get; }
        public string InstrumentName { get; }
        public event EventHandler<AcquisitionErrorsArrivedEventArgs> AcquisitionErrorsArrived;
        public event EventHandler<EventArgs> ConnectionChanged;
        public event EventHandler<ContactClosureEventArgs> ContactClosureChanged;

        public UInstrumentAccessFusion(fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccessContainer iac, int index)
        {
            instAcc = iac.Get(index);
            instAcc.AcquisitionErrorsArrived += AcquisitionErrorsArrivedFusion;
            instAcc.ConnectionChanged += ConnectionChangedFusion;
            instAcc.ContactClosureChanged += ContactClosureChangedFusion;
            //Control = InstControlFactory.Get(instAcc);
            Connected = instAcc.Connected;
            Control = new UIAPI.Interfaces.InstrumentAccess.Control.UControlFusion(instAcc);
            CountAnalogChannels = instAcc.CountAnalogChannels;
            CountMsDetectors = instAcc.CountMsDetectors;
            DetectorClasses = instAcc.DetectorClasses;
            InstrumentId = instAcc.InstrumentId;
            InstrumentName = instAcc.InstrumentName;
        }

        void AcquisitionErrorsArrivedFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs e)
        {
            AcquisitionErrorsArrivedEventArgs args = new AcquisitionErrorsArrivedEventArgs(e);
            OnAcquisitionErrorsArrived(args);
        }

        void ConnectionChangedFusion(object sender, EventArgs e)
        {
            OnConnectionChanged(e);
        }

        void ContactClosureChangedFusion(object sender, fusion.Thermo.Interfaces.FusionAccess_V1.ContactClosureEventArgs e)
        {
            ContactClosureEventArgs args = new ContactClosureEventArgs(e);
            OnContactClosureChanged(args);
        }

        public UIAPI.Interfaces.InstrumentAccess.MsScanContainer.IUMsScanContainer GetMsScanContainer(int msDetectorSet)
        {
            UIAPI.Interfaces.InstrumentAccess.MsScanContainer.IUMsScanContainer msCont = new UIAPI.Interfaces.InstrumentAccess.MsScanContainer.UMsScanContainerFusion(instAcc, msDetectorSet);
            return msCont;
        }

        protected virtual void OnAcquisitionErrorsArrived(AcquisitionErrorsArrivedEventArgs e)
        {
            EventHandler<AcquisitionErrorsArrivedEventArgs> handler = AcquisitionErrorsArrived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnConnectionChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = ConnectionChanged;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnContactClosureChanged(ContactClosureEventArgs e)
        {
            EventHandler<ContactClosureEventArgs> handler = ContactClosureChanged;
            if(handler != null)
            {
                handler(this, e);
            }
        }
    }

  internal class UInstrumentAccessVMS : IUInstrumentAccess
  {
    public UMsScanContainerVMS MsScanCont { get; }
    public bool Connected { get; }
    public IUControl Control { get; } = new UControlVMS();
    public int CountAnalogChannels { get; }
    public int CountMsDetectors { get; }
    public string[] DetectorClasses { get; }
    public int InstrumentId { get; }
    public string InstrumentName { get; }
    public event EventHandler<AcquisitionErrorsArrivedEventArgs> AcquisitionErrorsArrived;
    public event EventHandler<EventArgs> ConnectionChanged;
    public event EventHandler<ContactClosureEventArgs> ContactClosureChanged;

    public UInstrumentAccessVMS()
    {
      Connected = true;
      Control = new UControlVMS();
      MsScanCont = new UMsScanContainerVMS();
      CountAnalogChannels = 1;
      CountMsDetectors = 1;
      DetectorClasses = new string[1] { "dunno" };
      InstrumentId = 1;
      InstrumentName = "VirtualMS Instrument Name";
    }

    public IUMsScanContainer GetMsScanContainer(int msDetectorSet)
    {
      return MsScanCont;
    }

  }



}
