using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DraftAdmin.Models;
using System.Collections.ObjectModel;
using DraftAdmin.PlayoutCommands;

namespace DraftAdmin.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {

        #region Private Members

        private string _statusMessageText;
        private string _statusMessageColor;
        private string _promptMessage;

        public delegate void SendCommandEventHandler(PlayerCommand command, Playlist playlist = null);
        public delegate void SendCommandNoTransitionsEventHandler(PlayerCommand command);
        public delegate void SetStatusBarMsgEventHandler(string msgText, string msgColor);
        public delegate void StartCycleEventHandler();
        public delegate void StopCycleEventHandler();
        public delegate void ResetCycleEventHandler();

        public event SetStatusBarMsgEventHandler SetStatusBarMsg;
        public event SendCommandEventHandler SendCommandEvent;
        public event SendCommandNoTransitionsEventHandler SendCommandNoTransitionsEvent;
        public event StartCycleEventHandler StartCycleEvent;
        public event StopCycleEventHandler StopCycleEvent;
        public event ResetCycleEventHandler ResetCycleEvent;

        #endregion

        #region Properties

        public string StatusMessageText
        {
            get { return _statusMessageText; }
            set { _statusMessageText = value; OnPropertyChanged("StatusMessageText"); }
        }

        public string StatusMessageColor
        {
            get { return _statusMessageColor; }
            set { _statusMessageColor = value; OnPropertyChanged("StatusMessageColor"); }
        }

        public string PromptMessage
        {
            get { return _promptMessage; }
            set { _promptMessage = value; OnPropertyChanged("PromptMessage"); }
        }

        #endregion

        #region Private Methods

        #endregion

        #region Protected Methods

        protected void OnSetStatusBarMsg(string msgText, string msgColor)
        {
            SetStatusBarMsgEventHandler handler = SetStatusBarMsg;

            if (handler != null)
            {
                handler(msgText, msgColor);
            }
        }

        //protected void OnSendCommand(PlayerCommand command)
        //{
        //    SendCommandEventHandler handler = SendCommandEvent;

        //    if (handler != null)
        //    {
        //        handler(command);
        //    }
        //}

        protected void OnSendCommand(PlayerCommand command, Playlist playlist)
        {
            SendCommandEventHandler handler = SendCommandEvent;

            if (handler != null)
            {
                handler(command, playlist);
            }
        }

        protected void OnSendCommandNoTransitions(PlayerCommand command)
        {
            SendCommandNoTransitionsEventHandler handler = SendCommandNoTransitionsEvent;

            if (handler != null)
            {
                handler(command);
            }
        }

        protected void OnStartCycle()
        {
            StartCycleEventHandler handler = StartCycleEvent;

            if (handler != null)
            {
                handler();
            }
        }

        protected void OnStopCycle()
        {
            StopCycleEventHandler handler = StopCycleEvent;

            if (handler != null)
            {
                handler();
            }
        }

        protected void OnResetCycle()
        {
            ResetCycleEventHandler handler = ResetCycleEvent;

            if (handler != null)
            {
                handler();
            }
        }

        #endregion

        #region INotifyPropertyChanges

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IDisposable Members

        //Implement IDisposable.
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
