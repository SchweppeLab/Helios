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
    /// A wrapper around IAPI IMessage interface. Documentation from IAPI:<br/>
    /// Describes a message coming from an instrument. This interface will also be used for status reports or messages of the transport layer.
    /// The final message should be constructed by the client by use of string.Format(message, messageArgs). Only strings can be transmitted.<br/>
    /// The instruments sends message = "Just a message.", messageArgs = null(or with arbitrary content) should be formatted as "Just a message.". <br/>
    /// The instruments sends message = "Just a message {0}.", messageArgs = null should be formatted as "Just a message ." because missing elements will be ignored.<br/>
    /// The instruments sends message= "Just a message {0}.", messageArgs={ "for you" } should be formatted as "Just a message for you.".
    /// </summary>
    public interface IUIAPIMessage
    {
        /// <summary>
        /// Not documented in IAPI.
        /// </summary>
        string AccountName { get; }
        /// <summary>
        /// Not documented in IAPI.
        /// </summary>
        string ApplicationName { get; }
        /// <summary>
        /// Not documented in IAPI.
        /// </summary>
        string ComputerName { get; }
        /// <summary>
        /// The time the message occured in UTC.
        /// </summary>
        DateTime CreationTime { get; }
        /// <summary>
        /// 0 for the transport layer of the ID of the instrument starting from 1.
        /// </summary>
        int InstrumentId { get; }
        /// <summary>
        /// null for messages of the transport layer of the name of the instrument.
        /// </summary>
        string InstrumentName { get; }
        /// <summary>
        /// Content of the message including its format. The language is always English. It will be used together with Thermo.Interfaces.InstrumentAccess_V1.IMessage.MessageArgs. 
        /// The final usage will be similar to string.Format(Message, MessageArgs);<br/>
        /// References to elements in MessageArgs can be done by a number starting with 0 in braces.
        /// </summary>
        string Message { get; }
        /// <summary>
        /// These are the arguments to Thermo.Interfaces.InstrumentAccess_V1.IMessage.Message to create the final message. Content is transferred using the independent locale in English.<br/>
        /// All parameters in messageFormat must be addressed, at least.
        /// </summary>
        string[] MessageArgs { get; }
        /// <summary>
        /// The unique ID of the message type. Background is that messages with the same message ID describe the same problem.
        /// </summary>
        uint MessageId { get; }
        /// <summary>
        /// Not documented in IAPI.
        /// </summary>
        int ProcessId { get; }
        /// <summary>
        /// Status is a set of flags and values which encode the severity of the message. Values can be combined of not stated otherwise.
        /// <br/>1 = The message is very informational as for debug messages and should be used rarely.This value is mutually exclusive with any other value between 1 and 7.
        /// <br/>2 = The message is significant, but doesn't show any defect. The message can be used for messages showing 'success'. This value is mutually exclusive with any other value between 1 and 7.
        /// <br/>3 = The message is indicating a warning.Warnings have to be made prominent, but there is no reason to stop any ongoing process.This value is mutually exclusive with any other value between 1 and 7.
        /// <br/>4 = This value shall be used for messages indicate a severe lack of functionality.The system may come back in a stable state by maintenance operations or user interaction.This value is mutually exclusive with any other value between 1 and 7.
        /// <br/>16 = The message is a debug message.It is not intended to store this message.
        /// <br/>256 = The message should be placed in a local log file.It is not intended to "publish" this message to a higher level.
        /// <br/>512 = An error occurred that requires that requires the reboot of the instrument / driver to come back into a stable state.
        /// <br/>1024 = An error occurred that requires physical activity or maintenance.
        /// <br/>2048 = An error occurred that voids the outcome of the current acquisition.
        /// </summary>
        int Status { get; }
        /// <summary>
        /// Not documented in IAPI.
        /// </summary>
        string UserName { get; }
    }

    class UIAPIMessage : IUIAPIMessage
    {
        public string AccountName { get; }
        public string ApplicationName { get; }
        public string ComputerName { get; }
        public DateTime CreationTime { get; }
        public int InstrumentId { get; }
        public string InstrumentName { get; }
        public string Message { get; }
        public string[] MessageArgs { get; }
        public uint MessageId { get; }
        public int ProcessId { get; }
        public int Status { get; }
        public string UserName { get; }

        public UIAPIMessage(exploris.Thermo.Interfaces.InstrumentAccess_V1.IMessage m)
        {
            AccountName = m.AccountName;
            ApplicationName = m.ApplicationName;
            ComputerName = m.ComputerName;
            CreationTime = m.CreationTime;
            InstrumentId = m.InstrumentId;
            InstrumentName = m.InstrumentName;
            Message = m.Message;
            MessageArgs = m.MessageArgs;
            MessageId = m.MessageId;
            ProcessId = m.ProcessId;
            Status = m.Status;
            UserName = m.UserName;
        }
        public UIAPIMessage(Thermo.Interfaces.InstrumentAccess_V1.IMessage m)
        {
            AccountName = m.AccountName;
            ApplicationName = m.ApplicationName;
            ComputerName = m.ComputerName;
            CreationTime = m.CreationTime;
            InstrumentId = m.InstrumentId;
            InstrumentName = m.InstrumentName;
            Message = m.Message;
            MessageArgs = m.MessageArgs;
            MessageId = m.MessageId;
            ProcessId = m.ProcessId;
            Status = m.Status;
            UserName = m.UserName;
        }
    }

    /// <summary>
    /// Wrapper around Thermo.Interfaces.InstrumentAccess_V1.IMessages. IAPI Docs:<br/>
    /// This class will be used for status reports or messages of the transport layer and all instruments.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Get access to the messages that have arrived from all instrument and the transport layer.
        /// </summary>
        public IList<IUIAPIMessage> Messages;

        /// <summary>
        /// Create a new Exploris Thermo.Interfaces.InstrumentAccess_V1.MessagesArrivedEventArgs.
        /// </summary>
        /// <param name="e"></param>
        public MessageEventArgs(exploris.Thermo.Interfaces.InstrumentAccess_V1.MessagesArrivedEventArgs e)
        {
            foreach (var v in e.Messages)
            {
                Messages.Add(new UIAPIMessage(v));
            }
        }

        /// <summary>
        /// Create a new Fusion Thermo.Interfaces.InstrumentAccess_V1.MessagesArrivedEventArgs.
        /// </summary>
        /// <param name="e"></param>
        public MessageEventArgs(Thermo.Interfaces.InstrumentAccess_V1.MessagesArrivedEventArgs e)
        {
            foreach (var v in e.Messages)
            {
                Messages.Add(new UIAPIMessage(v));
            }
        }
    }
}
