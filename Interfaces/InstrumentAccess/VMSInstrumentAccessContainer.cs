using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thermo.Interfaces.InstrumentAccess_V1;
using UIAPI.Interfaces.InstrumentAccess.MsScanContainer;
using Pipes;
using System.CodeDom;

namespace UIAPI.Interfaces.InstrumentAccess
{
  internal class UInstrumentAccessContainerVM : IUInstrumentAccessContainer
  {

    VMSInstrumentAccess instAcc = new VMSInstrumentAccess();
    private bool check = false;
    //DataStreamer ss = null;
    PipesClient pipeClient = null;

    public UInstrumentAccessContainerVM()
    {
      var exists = System.Diagnostics.Process.GetProcessesByName("VirtualMS").Count() > 0;
      if (exists) check = true;
      else check = false;
      return;
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
        pipeClient?.Stop();
        //cont.Dispose();
      }
      // free native resources if there are any.
    }

    public string InstrumentType()
    {
      return "VirtualMS";
    }

    protected virtual void OnMessagesArrived(MessageEventArgs e)
    {
      MessagesArrived?.Invoke(this, e);
    }

    protected virtual void OnServiceConnectionChanged(EventArgs e)
    {
      ServiceConnectionChanged?.Invoke(this, e);
    }

    public void StartOnlineAccess()
    {
      pipeClient = new PipesClient("VirtualMS");
      pipeClient.ServerMessage += OnServerMessage;
      pipeClient.Error += OnError;
      pipeClient.Start();
      ServiceConnected = true;
      OnServiceConnectionChanged(new EventArgs());
    }

    public IUInstrumentAccess Get(int index)
    {
      return instAcc;
    }

    private void OnServerMessage(PipesConnection connection, PipeMessage message)
    {
      switch (message.MsgCode)
      {
        case '0':
          Console.WriteLine("Server says: {0}", message.DecodeString());
          break;
        case '1':
          ((VMSMsScanContainer)instAcc.GetMsScanContainer(0)).ReceiveScan(message.MsgData);
          break;
        default:
          Console.WriteLine("Server sent unrecognized message code: {0}", message.MsgCode);
          break;
      }

    }

    private void OnError(Exception exception)
    {
      Console.Error.WriteLine("ERROR: {0}", exception);
    }

    /// <summary>
    /// This event will be thrown when at least one error arrived from the instrument during an acquisition. This event handler will not be used for status reports or messages of the transport layer.<br/>
    /// For messages during acquisitions see Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccess.AcquisitionErrorsArrived.
    /// </summary>
    public event EventHandler<MessageEventArgs> MessagesArrived;

    /// <summary>
    /// This event handler will be fired when Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.ServiceConnected has changed its value. 
    /// This method will never be called if Thermo.Interfaces.InstrumentAccess_V1.IInstrumentAccessContainer.StartOnlineAccess has not been called.
    /// </summary>
    public event EventHandler<EventArgs> ServiceConnectionChanged;

    public bool ServiceConnected { get; protected set; }
  }

}
