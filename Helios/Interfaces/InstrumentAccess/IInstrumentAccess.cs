extern alias fusion;
extern alias exploris;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Interfaces.InstrumentAccess.AnalogTraceContainer;
using Helios.Interfaces.InstrumentAccess.Control;
using Helios.Interfaces.InstrumentAccess.MsScanContainer;
using Pipes;

namespace Helios.Interfaces.InstrumentAccess
{

  /// <summary>
  /// Wrapper around IAPI IInstrumentAccess. IAPI Docs:<br/>
  /// This interface describes the access to one particular instrument both for reading data as for managing access or behaviour of the instrument.
  /// </summary>
  public interface IInstrumentAccess
  {
    /// <summary>
    /// From IAPI Docs:<br/>
    /// Get access to the interface covering all control functionality of an instrument.<br/>
    /// This property is accessible offline.
    /// </summary>
    IControl Control { get; }

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

    IMsScanContainer GetMsScanContainer(int msDetectorSet);

    /// <summary>
    /// From IAPI: Get access to the container of analog trace information to be accessible by a specific analog detector set (UV, PDA, etc).
    /// </summary>
    /// <param name="analogDetectorSet">index of the detector starting from 0</param>
    /// <returns>Returns a particular analog detector set or null if a detector with the specific index is not present in the instrument.</returns>
    IAnalogTraceContainer GetAnalogTraceContainer(int analogDetectorSet);

    int CountAnalogChannels { get; }
    int CountMsDetectors { get; }
    string[] DetectorClasses { get; }
    int InstrumentId { get; }
    string InstrumentName { get; }
  }

    class HeliosInstrumentAccessExploris : IInstrumentAccess
    {
        exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccess instAcc;
        public bool Connected { get; }
        public IControl Control { get; }
        public int CountAnalogChannels { get; }
        public int CountMsDetectors { get; }
        public string[] DetectorClasses { get; }
        public int InstrumentId { get; }
        public string InstrumentName { get; }
        public event EventHandler<AcquisitionErrorsArrivedEventArgs> AcquisitionErrorsArrived;
        public event EventHandler<EventArgs> ConnectionChanged;
        public event EventHandler<ContactClosureEventArgs> ContactClosureChanged;

    public HeliosInstrumentAccessExploris(exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccessContainer iac, int index)
    {
      instAcc = iac.Get(index);
      instAcc.AcquisitionErrorsArrived += AcquisitionErrorsArrivedExploris;
      instAcc.ConnectionChanged += ConnectionChangedExploris;
      //instAcc.ContactClosureChanged += ContactClosureChangedFusion; //not implemented in Exploris
      //Control = InstControlFactory.Get(instAcc);
      Connected = instAcc.Connected;

      Control = new HeliosControlExploris(instAcc);
      CountAnalogChannels = instAcc.CountAnalogChannels;
      CountMsDetectors = instAcc.CountMsDetectors;
      DetectorClasses = new string[instAcc.DetectorClasses.Length];
      for (int i = 0; i < instAcc.DetectorClasses.Length; i++) { 
        DetectorClasses[i] = instAcc.DetectorClasses[i];
      }
      InstrumentId = instAcc.InstrumentId;
      InstrumentName = instAcc.InstrumentName;
    }

        void AcquisitionErrorsArrivedExploris(object sender, exploris.Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs e)
        {
            AcquisitionErrorsArrivedEventArgs args = new ExplorisAcquisitionErrorsArrivedEventArgs(e);
            OnAcquisitionErrorsArrived(args);
        }

        void ConnectionChangedExploris(object sender, EventArgs e)
        {
            OnConnectionChanged(e);
        }

    public IAnalogTraceContainer GetAnalogTraceContainer(int msDetectorSet)
    {
      return new HeliosAnalogTraceContainerExploris(instAcc, msDetectorSet);
    }

    public IMsScanContainer GetMsScanContainer(int msDetectorSet)
    {
        IMsScanContainer msCont = new HeliosMsScanContainerExploris(instAcc,msDetectorSet);
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

  class HeliosInstrumentAccessFusion : IInstrumentAccess
  {
    fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccess instAcc;
    public bool Connected { get; }
    public IControl Control { get; }
        public int CountAnalogChannels { get; }
        public int CountMsDetectors { get; }
        public string[] DetectorClasses { get; }
        public int InstrumentId { get; }
        public string InstrumentName { get; }
        public event EventHandler<AcquisitionErrorsArrivedEventArgs> AcquisitionErrorsArrived;
        public event EventHandler<EventArgs> ConnectionChanged;
        public event EventHandler<ContactClosureEventArgs> ContactClosureChanged;

        public HeliosInstrumentAccessFusion(fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccessContainer iac, int index)
        {
            instAcc = iac.Get(index);
            instAcc.AcquisitionErrorsArrived += AcquisitionErrorsArrivedFusion;
            instAcc.ConnectionChanged += ConnectionChangedFusion;
            instAcc.ContactClosureChanged += ContactClosureChangedFusion;
            //Control = InstControlFactory.Get(instAcc);
            Connected = instAcc.Connected;
            Control = new HeliosControlFusion(instAcc);
            CountAnalogChannels = instAcc.CountAnalogChannels;
            CountMsDetectors = instAcc.CountMsDetectors;

      DetectorClasses = new string[instAcc.DetectorClasses.Length];
      for (int i = 0; i < instAcc.DetectorClasses.Length; i++)
      {
        DetectorClasses[i] = instAcc.DetectorClasses[i];
      }
      InstrumentId = instAcc.InstrumentId;
            InstrumentName = instAcc.InstrumentName;
        }

        void AcquisitionErrorsArrivedFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.AcquisitionErrorsArrivedEventArgs e)
        {
            AcquisitionErrorsArrivedEventArgs args = new FusionAcquisitionErrorsArrivedEventArgs(e);
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

    public IAnalogTraceContainer GetAnalogTraceContainer(int msDetectorSet)
    {
      return new HeliosAnalogTraceContainerFusion(instAcc, msDetectorSet);
    }

    public IMsScanContainer GetMsScanContainer(int msDetectorSet)
        {
            IMsScanContainer msCont = new HeliosMsScanContainerFusion(instAcc, msDetectorSet);
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

  internal class HeliosInstrumentAccessVMS : IInstrumentAccess
  {
    public HeliosAnalogTraceContainerVMS AnalogTraceContainer { get; }
    public HeliosMsScanContainerVMS MsScanCont { get; }
    public bool Connected { get; }
    public IControl Control { get; } = new HeliosControlVMS();
    public int CountAnalogChannels { get; }
    public int CountMsDetectors { get; }
    public string[] DetectorClasses { get; }
    public int InstrumentId { get; }
    public string InstrumentName { get; }
    public event EventHandler<AcquisitionErrorsArrivedEventArgs> AcquisitionErrorsArrived;
    public event EventHandler<EventArgs> ConnectionChanged;
    public event EventHandler<ContactClosureEventArgs> ContactClosureChanged;

    private readonly PipesClient pipeClient = null;

    public HeliosInstrumentAccessVMS(PipesClient pc = null)
    {
      pipeClient = pc;

      AnalogTraceContainer = new HeliosAnalogTraceContainerVMS();
      Connected = true;
      Control = new HeliosControlVMS(pipeClient);
      MsScanCont = new HeliosMsScanContainerVMS();
      CountAnalogChannels = 1;
      CountMsDetectors = 1;
      DetectorClasses = new string[1] { "dunno" };
      InstrumentId = 1;
      InstrumentName = "VirtualMS";
      
    }

    public IAnalogTraceContainer GetAnalogTraceContainer(int msDetectorSet)
    {
      return AnalogTraceContainer;
    }

    public IMsScanContainer GetMsScanContainer(int msDetectorSet)
    {
      return MsScanCont;
    }

  }



}
