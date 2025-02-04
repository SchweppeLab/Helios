extern alias exploris;
extern alias fusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAPI.Interfaces.InstrumentAccess
{
    /// <summary>
    /// Wrapper around the InstrumentAccessContainer. IAPI Docs:<br/>
    /// This interface is the central access point for a direct access of an instrument. 
    /// It covers both online functionality as well as offline manipulation of methods.
    /// </summary>
    public interface IUInstrumentAccessContainer : IDisposable
    {
        /// <summary>
        /// Check if connection to the instrument was successful.
        /// </summary>
        /// <returns>true if connection was successful.</returns>
        bool Check();

        /// <summary>
        /// Get access to a particular instrument accessor using this method.
        /// </summary>
        /// <param name="index">Index of the instrument starting from 1. Values of 2 or more need additional instruments attached to the data system.</param>
        /// <returns>The return value allows access to a particular instrument. 
        /// If one service hosts several instruments in parallel, this value can be of a different type for each. 
        /// </returns>
        IUInstrumentAccess Get(int index);

        /// <summary>
        /// Currently used for UIAPI diagnostics. Likely to be removed at some point.
        /// </summary>
        /// <returns></returns>
        string InstrumentType();

        /// <summary>
        /// Return whether this API instance connected to the service. 
        /// This will always be false until a call to Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.StartOnlineAccess has been performed.<br/>
        /// A value change can we watched by registering to Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.ServiceConnectionChanged.<br/>
        /// Note: this is a function in UIAPI to expose the bool value of the underlying IAPI InstrumentAccessContainer.ServiceConnected state without
        /// having to update it indirectly.
        /// </summary>
        /// <returns>true/false status of underlying instrument service connection.</returns>
        //bool ServiceConnected();
        bool ServiceConnected { get; }

        /// <summary>
        /// From IAPI Docs:<br/>
        /// The API allows both online and offline functionality. Offline functionality is always available, but online functionality requires a connection to the service hosting the instrument.<br/>
        /// No online instrument connection is possible without an online connection to the service.<br/><br/>
        /// This call initiates the connection. Check Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.ServiceConnectionChanged and Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.ServiceConnected for success.<br/>
        /// Further calls to this method don't have any effect. Once, this method has been called, am automatic reconnect will be scheduled if the connection breaks.
        /// </summary>
        void StartOnlineAccess();

        /// <summary>
        /// This event will be thrown when at least one error arrived from the instrument during an acquisition. This event handler will not be used for status reports or messages of the transport layer.<br/>
        /// For messages during acquisitions see Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccess.AcquisitionErrorsArrived.
        /// </summary>
        event EventHandler<UMessagesArrivedEventArgs> MessagesArrived;

        /// <summary>
        /// This event handler will be fired when Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.ServiceConnected has changed its value. 
        /// This method will never be called if Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.StartOnlineAccess has not been called.
        /// </summary>
        event EventHandler<EventArgs> ServiceConnectionChanged;
        
    }

    class UInstrumentAccessContainerExploris : IUInstrumentAccessContainer
    {
        private exploris.Thermo.Interfaces.ExplorisAccess_V1.IExplorisInstrumentAccessContainer cont;
        private bool check = false;
        public event EventHandler<EventArgs> ServiceConnectionChanged;
        public event EventHandler<UMessagesArrivedEventArgs> MessagesArrived;
        public bool ServiceConnected { get; protected set; }

        public UInstrumentAccessContainerExploris()
        {
            try
            {
                cont = ExplorisConnection.Get();
                cont.ServiceConnectionChanged += ServiceConnectionChangedExploris;
                cont.MessagesArrived += MessagesArrivedExploris;
                check = true;
            }
            catch
            {
                check = false;
            }

        }
        public bool Check()
        {
            return check;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                cont.Dispose();
            }
            // free native resources if there are any.
        }

        void MessagesArrivedExploris(object sender, exploris.Thermo.Interfaces.InstrumentAccess_V1.MessagesArrivedEventArgs e)
        {
            UMessagesArrivedEventArgs args = new UMessagesArrivedEventArgs(e);
            OnMessagesArrived(args);
        }

        void ServiceConnectionChangedExploris(object sender, EventArgs e)
        {
            ServiceConnected = cont.ServiceConnected;
            OnServiceConnectionChanged(e);
        }

        public UIAPI.Interfaces.InstrumentAccess.IUInstrumentAccess Get(int index)
        {
            return new UIAPI.Interfaces.InstrumentAccess.UInstrumentAccessExploris(cont, index);
        }

        public string InstrumentType()
        {
            return "Exploris";
        }
        protected virtual void OnMessagesArrived(UMessagesArrivedEventArgs e)
        {
            EventHandler<UMessagesArrivedEventArgs> handler = MessagesArrived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnServiceConnectionChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = ServiceConnectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //public bool ServiceConnected()
        //{
        //    return cont.ServiceConnected;
        //}
        public void StartOnlineAccess()
        {
            try
            {
                cont.StartOnlineAccess();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("StartOnlineAccess Failed: " + e.Message);
            }
        }

    }

    class UInstrumentAccessContainerFusion : IUInstrumentAccessContainer
    {
        private fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccessContainer cont;
        private bool check = false;
        public event EventHandler<EventArgs> ServiceConnectionChanged;
        public event EventHandler<UMessagesArrivedEventArgs> MessagesArrived;
        public bool ServiceConnected { get; protected set; }

        public UInstrumentAccessContainerFusion()
        {
            try
            {
                cont = Thermo.TNG.Factory.Factory<fusion.Thermo.Interfaces.FusionAccess_V1.IFusionInstrumentAccessContainer>.Create();
                cont.ServiceConnectionChanged += ServiceConnectionChangedFusion;
                cont.MessagesArrived += MessagesArrivedFusion;
                check = true;
            }
            catch
            {
                check = false;
            }

        }
        public bool Check()
        {
            return check;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                cont.Dispose();
            }
            // free native resources if there are any.
        }

        void MessagesArrivedFusion(object sender, Thermo.Interfaces.InstrumentAccess_V1.MessagesArrivedEventArgs e)
        {
            UMessagesArrivedEventArgs args = new UMessagesArrivedEventArgs(e);
            OnMessagesArrived(args);
        }

        void ServiceConnectionChangedFusion(object sender, EventArgs e)
        {
            ServiceConnected = cont.ServiceConnected;
            OnServiceConnectionChanged(e);
        }

        public UIAPI.Interfaces.InstrumentAccess.IUInstrumentAccess Get(int index)
        {
            return new UIAPI.Interfaces.InstrumentAccess.UInstrumentAccessFusion(cont, index);
        }

        public string InstrumentType()
        {
            return "Fusion";
        }

        protected virtual void OnMessagesArrived(UMessagesArrivedEventArgs e)
        {
            EventHandler<UMessagesArrivedEventArgs> handler = MessagesArrived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnServiceConnectionChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = ServiceConnectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //public bool ServiceConnected()
        //{
        //    return cont.ServiceConnected;
        //}

        public void StartOnlineAccess()
        {
            try
            {
                cont.StartOnlineAccess();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("StartOnlineAccess Failed: " + e.Message);
            }
        }
    }

  /// <summary>
  /// Factory class that attempts to first connect to a Fusion instrument. Failing
  /// that, it attempts to connect to an Exploris instrument.
  /// </summary>
  static public class InstrumentAccessContainerFactory
  {
    /// <summary>
    /// Get an instance of the instrument.
    /// </summary>
    /// <returns>UIAPI.Interfaces.IInstrument object</returns>
    static public IUInstrumentAccessContainer Get()
    {
      UInstrumentAccessContainerFusion fusion = new UInstrumentAccessContainerFusion();
      if (fusion.Check()) return fusion;

      UInstrumentAccessContainerExploris exploris = new UInstrumentAccessContainerExploris();
      if (exploris.Check()) return exploris;

      UInstrumentAccessContainerVM vm = new UInstrumentAccessContainerVM();
      if (vm.Check()) return vm;

      return null;

    }
  }
}
