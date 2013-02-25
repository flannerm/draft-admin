using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
//using PBRS.Shared.Logging;
using Skt_05;
//using System.Xml.Serialization;
using System.IO;
//using System.Timers;
using System.ComponentModel;
using Timer = System.Threading.Timer;
using DraftAdmin.PlayoutCommands;

namespace DraftAdmin.Sockets
{

    public class Talker : IDisposable
    {
        private AsyncSkt _socketListener;
        private AsyncSkt _socketTalker;

        private string _identifier = "";

        // string TalkingIP;
        private string _talkingPort = "1000";
        private string _listeningPort = "1001";

        // used for heartbeat:
        private Timer _heartbeatTimer;
        //private int _heartbeatInterval = 600000;
        private TimeSpan _heartbeatInterval = new TimeSpan(0, 2, 0);
        private ISynchronizeInvoke _synchronizingObject { get; set; }
        // Track whether Dispose has been called.
        private bool _disposed = false;

        #region Properties
        public string TalkingPort
        {
            get { return _talkingPort; }
        }

        public string ListeningPort
        {
            get { return _listeningPort; }
        }
        #endregion

        #region "Events"

        public delegate void DataArrivalHandler(PlayerCommand CommandReceived, string ID);
        public event DataArrivalHandler DataArrival;
        protected void OnDataArrival(PlayerCommand CommandtoRaise, string ID)
        {
            if (DataArrival != null)
            {
                DataArrival(CommandtoRaise, ID);
            }
        }

        public delegate void ConnectionHandler();
        public event ConnectionHandler Connected;
        protected void OnConnected()
        {
            if (Connected != null)
            {
                Connected();
            }
        }

        public delegate void ConnectionClosedHandler();
        public event ConnectionClosedHandler ConnectionClosed;
        protected void OnConnectionClosed()
        {
            if (ConnectionClosed != null)
            {
                ConnectionClosed();
            }
        }


        #endregion


        #region "Public"
        // on instantiation...
        public Talker(string ID = "")
        {
            //LogAccess.LogOutput = //LogAccess.LogOutputType.Local;
            InitializeTalker();
            _identifier = ID;
        }


        ~Talker()
        {

            ProcessOnDisconnect();
        }

        public void Connect(string IPtoConnectTo)
        {
            Connect(IPtoConnectTo, _talkingPort);
        }


        public bool Talk(string xmlCommandToSend)
        {
            DisableHeartbeat();
            try
            {
                SendData(xmlCommandToSend, _socketTalker);
                return true;
            }
            catch (Exception ex)
            {
                //LogAccess.WriteLog(ex.ToString(), "Talker");
                return false;
            }
            finally
            {
                //re enable the timer so we can send future heartbeats
                EnableHeartbeat(); // _heartbeatTimer.Change(_heartbeatInterval,_heartbeatInterval);
            }

        }

        public bool Talk(PlayerCommand CommandToSend)
        {
            //disable the heartbeat so we don't get a heartbeat while trying to send a command
            DisableHeartbeat(); // _heartbeatTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            // update ID, timestamp
            CommandToSend.ID = TcpIpCommon.GetMyID();
            CommandToSend.Timestamp = DateTime.Now;

            try
            {
                SendData(CommandToSend, _socketTalker);
                return true;
            }
            catch (Exception ex)
            {
                //LogAccess.WriteLog(ex.ToString(), "Talker");
                return false;
            }
            finally
            {
                //re enable the timer so we can send future heartbeats
                EnableHeartbeat(); // _heartbeatTimer.Change(_heartbeatInterval,_heartbeatInterval);
            }
        }

        #endregion


        #region "Private"

        private void DisableHeartbeat()
        {
            try
            {
                if (_heartbeatTimer != null)
                {
                    _heartbeatTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                }
            }
            catch (System.ObjectDisposedException objDisosed)
            {
                //Do nothing since timer is already disposed we can't disable it, and it's already effectively disabled.
            }
        }

        private void EnableHeartbeat()
        {
            try
            {
                if (_heartbeatTimer != null)
                {
                    _heartbeatTimer.Change(_heartbeatInterval, _heartbeatInterval);
                }
            }
            catch (System.ObjectDisposedException objDisosed)
            {
                //Do nothing since if the timer is disposed we can't enable the heartbeat ...log it.
                //LogAccess.WriteLog("Enable of heartbeat failed due to timer being disposed", "Talker");
            }
        }

        private void InitializeTalker()
        {
            //LogAccess.WriteLog("Initializing Talker", "Talk");
            _socketTalker = new AsyncSkt();

            // create callback delegates:
            _socketTalker.Connected += new AsyncSkt.ConnectedEventHandler(ProcessOnConnect);

        }

        private void InitializeListener()
        {
            //LogAccess.WriteLog("Initializing Listener", "Talker");
            _socketListener = new AsyncSkt();

            // create callback delegates:
            _socketListener.MsgReceived += new AsyncSkt.MsgReceivedEventHandler(ProcessOnDataArrival);
            _socketListener.ConnectionAccepted += new AsyncSkt.ConnectionAcceptedEventHandler(ProcessConnectionAccepted);
            _socketListener.SktError += new AsyncSkt.SktErrorEventHandler(ProcessError);

        }


        public void Connect(string DestinationIP, string DestinationPort)
        {

            if (DestinationPort != null)
            {
                _talkingPort = DestinationPort;
            }

            System.Net.IPAddress remoteConnection;
            remoteConnection = System.Net.IPAddress.Parse(DestinationIP);

            _socketTalker.RemoteIP = remoteConnection;
            _socketTalker.Port = Convert.ToInt32(_talkingPort);
            _socketTalker.PacketSize = 4096;


            _socketTalker.Connect();


        }


        private void Listen(string PortToListenOn)
        {

            if (PortToListenOn != null)
            {
                _listeningPort = PortToListenOn;
            }


            if (_socketListener == null)
            {
                InitializeListener();
            }

            // begin listening
            _socketListener.Port = Convert.ToInt32(_listeningPort);
            _socketListener.PacketSize = 4096;

            _socketListener.StartListening();
            //LogAccess.WriteLog("Listing (port=" + _listeningPort + ")", "Talker");
        }


        // private routines
        private void SendHeartBeat(AsyncSkt SocketToSendOn)
        {
            //disable the heartbeat so we don't get queued events
            DisableHeartbeat(); // _heartbeatTimer.Enabled = false;
            PlayerCommand myHeartBeat = new PlayerCommand()
            {
                ID = TcpIpCommon.GetMyID(),
                Timestamp = DateTime.Now,
                Command = CommandType.Heartbeat
            };

            //LogAccess.WriteLog("...heartbeat...", "Talker");
            SendData(myHeartBeat, SocketToSendOn);
            //re enable the timer so we can send future heartbeats
            EnableHeartbeat(); // _heartbeatTimer.Enabled = true;
        }



        private bool SendData(PlayerCommand CommandObjectToSend, AsyncSkt ConnectionToUse)
        {
            // do we have a valid connection?
            if (ConnectionToUse == null)
            { return false; }

            // do we have an object to send?
            if (CommandObjectToSend == null)
            { return false; }

            string SerializedData = "";
            try
            {
                SerializedData = TcpIpCommon.SerializeCommandObject(CommandObjectToSend);
            }
            catch (Exception ex)
            {
                //LogAccess.WriteLog("Error serializing data in SendData: " + ex.ToString(), "Talker");
            }

            // (bsk) add divider for splitting...
            SerializedData += "<msg>";

            try
            {
                ConnectionToUse.SendData(SerializedData);
            }
            catch (Exception ex)
            {
                //LogAccess.WriteLog("Error sending data: " + ex.ToString(), "Talker");
            }
            //LogAccess.WriteLog("SendData complete", "Talker");
            return true;
        }

        private bool SendData(string xmlToSend, AsyncSkt ConnectionToUse)
        {
            // do we have a valid connection?
            if (ConnectionToUse == null)
            { return false; }

            // do we have an object to send?
            if (string.IsNullOrEmpty(xmlToSend))
            { return false; }

            try
            {
                ConnectionToUse.SendData(xmlToSend);
            }
            catch (Exception ex)
            {
                //LogAccess.WriteLog("Error sending data: " + ex.ToString(), "Talker");
            }
            //LogAccess.WriteLog("SendData complete", "Talker");
            return true;
        }
        #endregion


        #region "CallBack Events"

        private void ProcessError(SocketException ex)
        {
            if (ex.NativeErrorCode == 10054)
            {
                // socket has been closed
                ProcessOnDisconnect();
            }
            else if (ex.NativeErrorCode == 10061)
            {
                // this is an active refusal of connection just log it for now ...later maybe we will try to autoreconnect
                //LogAccess.WriteLog("TCPIP Error -" + ex.Message, "TcpipCommunicator", Shared.Logging.LogMessageType.Warning);
            }
            else
            {
                throw ex;
            }
            Debug.Print(ex.Message.ToString());
        }

        private void ProcessOnDisconnect()
        {
            //LogAccess.WriteLog("Disconnecting...", "Talker");
            if (_socketTalker != null)
            {
                _socketTalker.Close();
            }

            if (_socketListener != null)
            {
                _socketListener.Close();
            }

            OnConnectionClosed();

        }

        // routines to process events from socket delegates:
        private void ProcessOnConnect()
        {
            //LogAccess.WriteLog("Connecting...", "Talker");
            int portToListenOn = _socketTalker.Port + 1;
            _listeningPort = portToListenOn.ToString();

            Listen(_listeningPort);
        }

        private void ProcessOnDataArrival(string incomingCommand)
        {
            // reset heartbeat
            DisableHeartbeat();
            EnableHeartbeat();

            PlayerCommand commandToProcess = new PlayerCommand();

            // split..?
            string[] stringSeperators = new string[] { "<msg>" };
            string[] myCommands = incomingCommand.Split(stringSeperators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string command in myCommands)
            {
                try
                {
                    commandToProcess = TcpIpCommon.DeserializeCommandObject(command);
                }
                catch (Exception ex)
                {
                    // log here?
                    Debug.Print(ex.ToString());
                    //LogAccess.WriteLog("Error processing command: " + ex.ToString(), "Talker");
                    //throw;
                }

                if (commandToProcess != null)
                {
                    //SendAcknowledgement(commandToProcess.CommandID);
                    OnDataArrival(commandToProcess, _identifier);
                }
            }

        }

        private void ProcessConnectionAccepted()
        {
            //LogAccess.WriteLog("Connection Accepted!", "Talker");
            // begin heartbeat
            _heartbeatTimer = new Timer(heartbeatTimer_Elapsed, null, _heartbeatInterval, _heartbeatInterval);
            OnConnected();
        }

        private System.Object lockThis = new System.Object();
        void heartbeatTimer_Elapsed(object state)
        {
            lock (lockThis)
            {
                SendHeartBeat(_socketTalker);
            }

        }

        #endregion

        #region IDisposable Implementation
        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_heartbeatTimer != null)
                    {
                        _heartbeatTimer.Dispose();
                    }
                    if (_socketTalker != null)
                    {
                        _socketTalker.Close();
                        _socketTalker = null;
                    }
                }
                // Note disposing has been done.
                _disposed = true;
            }
        }
        #endregion
    }
}
