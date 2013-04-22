using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using DraftAdmin.Commands;
using DraftAdmin.PlayoutCommands;
using DraftAdmin.Output;
using DraftAdmin.Models;
using DraftAdmin.DataAccess;
using System.Configuration;

namespace DraftAdmin.ViewModels
{
    public class CurrentSelectionTabViewModel : ViewModelBase
    {

        #region Private Members
                
        private DelegateCommand _showPickIsInCommand;
        private DelegateCommand _showCurrentSelectionCommand;
        private DelegateCommand _revealCurrentSelectionCommand;
        private DelegateCommand _showPlayerTidbitsCommand;
        private DelegateCommand _nextOnTheClockCommand;
        private DelegateCommand _showClockCommand;
        private DelegateCommand _hideEndOfDraftCommand;
        
        private bool _askShowCurrentSelection = false;

        private Player _currentPlayer;

        private int _currentPlayerTidbit;

        private bool _refreshPlayersAfterSelection = true;

        private bool _otcHashtag = false;
        private string _selectedRightLogoFilename = "";

        #endregion

        #region Public Members

        public delegate void RefreshPlayersEventHandler();
        public delegate void TakeClockEventHandler();

        public event RefreshPlayersEventHandler RefreshPlayersEvent;
        public event TakeClockEventHandler TakeClockEvent;

        public DelegateCommand<object> ShowCurrentSelection { get; set; }
        public DelegateCommand<object> CancelShowCurrentSelection { get; set; }

        #endregion

        #region Properties

        public Player CurrentPlayer
        {
            get { return _currentPlayer; }
            set { _currentPlayer = value; OnPropertyChanged("CurrentPlayer"); }
        }

        public bool AskShowCurrentSelection
        {
            get { return _askShowCurrentSelection; }
            set { _askShowCurrentSelection = value; OnPropertyChanged("AskShowCurrentSelection"); }
        }

        public bool RefreshPlayersAfterSelection
        {
            get { return _refreshPlayersAfterSelection; }
            set { _refreshPlayersAfterSelection = value; OnPropertyChanged("RefreshPlayersAfterSelection"); }
        }

        public bool OTCHashtag
        {
            get { return _otcHashtag; }
            set { _otcHashtag = value; }
        }

        public string SelectedRightLogoFilename
        {
            get { return _selectedRightLogoFilename; }
            set { _selectedRightLogoFilename = value; }
        }

        #endregion

        #region Constructor

        public CurrentSelectionTabViewModel()
        {
            ShowCurrentSelection = new DelegateCommand<object>(showCurrentSelectionAction);
            CancelShowCurrentSelection = new DelegateCommand<object>(cancelShowCurrentSelectionAction);
        }

        #endregion

        #region Private Methods

        private void OnRefreshPlayers()
        {
            RefreshPlayersEventHandler handler = RefreshPlayersEvent;

            if (handler != null)
            {
                handler();
            }
        }

        private void showCurrentSelectionAction(object parameter)
        {           
            AskShowCurrentSelection = false;

            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.CommandID = Guid.NewGuid().ToString();
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "CurrentSelection"));

            XmlDataRow xmlRow = new XmlDataRow();

            xmlRow.Add("CURRENT_SELECTION_STATE", "CURRENTSELECTION");
            xmlRow.Add("CHIP_1", Global.GlobalCollections.Instance.OnTheClock.Team.PickPlateTga.LocalPath);
            xmlRow.Add("SWATCH_1", Global.GlobalCollections.Instance.OnTheClock.Team.SwatchTga.LocalPath);

            commandToSend.TemplateData = xmlRow.GetXMLString();

            //raise an event to the main...
            OnSendCommand(commandToSend, null);
        }

        private void cancelShowCurrentSelectionAction(object parameter)
        {
            AskShowCurrentSelection = false;
        }

        private void showCurrentSelection()
        {
            //stop the cycle on the MainViewModel
            OnStopCycle();

            PromptMessage = "Show Current Selection?";
            AskShowCurrentSelection = true;
        }

        private void showPickIsIn()
        {
            //int lastPick = Global.GlobalCollections.Instance.OnTheClock.OverallPick - 1;

            //CurrentPlayer = DbConnection.GetPlayerByPick(lastPick);

            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.CommandID = Guid.NewGuid().ToString();
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));

            XmlDataRow xmlRow = new XmlDataRow();

            xmlRow.Add("CLOCK_OVERLAY", "PICK IS IN");
            //xmlRow.Add("CLOCK_COLOR", "NORMAL");

            commandToSend.TemplateData = xmlRow.GetXMLString();

            OnSendCommand(commandToSend, null); 
        }

        private void revealCurrentSelection()
        {
            if (_currentPlayer != null)
            {
                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.CommandID = Guid.NewGuid().ToString();
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "CurrentSelection"));

                XmlDataRow xmlRow = new XmlDataRow();

                xmlRow.Add("CURRENT_SELECTION_STATE", "REVEAL");

                string curSelStr = _currentPlayer.FirstName + " " + _currentPlayer.LastName + " <font EventFranklinGothic Book>" + _currentPlayer.Position + " - " + _currentPlayer.School.Name + "<\\font>";

                if (_currentPlayer.TradeTidbit.Trim() != "")
                {
                    curSelStr += " " + _currentPlayer.TradeTidbit;
                }

                xmlRow.Add("PLAYER_1", curSelStr);

                commandToSend.TemplateData = xmlRow.GetXMLString();

                OnSendCommand(commandToSend, null);

                if (DbConnection.SelectPlayer(_currentPlayer))
                {
                    OnSetStatusBarMsg(_currentPlayer.FirstName + " " + _currentPlayer.LastName + " selected", "Green");

                    Global.GlobalCollections.Instance.LoadPlayers();

                    if (RefreshPlayersAfterSelection)
                    {
                        OnRefreshPlayers();  //refresh the players on the PlayerTabVM
                    } 
                }

                _currentPlayerTidbit = 0;

                Global.GlobalCollections.Instance.LoadOnTheClock();

                OnResetCycle();
            }
        }

        private void showPlayerTidbits()
        {
            if (_currentPlayer.Tidbits != null)
            {
                if (_currentPlayerTidbit < _currentPlayer.Tidbits.Count)
                {
                    PlayerCommand commandToSend = new PlayerCommand();

                    commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                    commandToSend.CommandID = Guid.NewGuid().ToString();
                    commandToSend.Parameters = new List<CommandParameter>();
                    commandToSend.Parameters.Add(new CommandParameter("TemplateName", "CurrentSelection"));

                    XmlDataRow xmlRow = new XmlDataRow();

                    xmlRow.Add("CURRENT_SELECTION_STATE", "SHOWTIDBITS");

                    string curSelStr = _currentPlayer.FirstName + " " + _currentPlayer.LastName + " <font EventFranklinGothic Book>" + _currentPlayer.Position + " - " + _currentPlayer.School.Name + "<\\font>";

                    if (_currentPlayer.TradeTidbit.Trim() != "")
                    {
                        curSelStr += " " + _currentPlayer.TradeTidbit;
                    }

                    xmlRow.Add("PLAYER_1", curSelStr);
                    xmlRow.Add("TIDBIT_1", _currentPlayer.Tidbits[_currentPlayerTidbit].TidbitText);

                    _currentPlayerTidbit++;

                    commandToSend.TemplateData = xmlRow.GetXMLString();

                    //raise an event to the main...
                    OnSendCommand(commandToSend, null);
                }
            }
        }

        private void nextOnTheClock()
        {
            bool isLastPick = false;

            if (Global.GlobalCollections.Instance.OnTheClock == null)
            {
                isLastPick = true;
            }
            else if (Global.GlobalCollections.Instance.OnTheClock.OverallPick > Global.GlobalCollections.Instance.LastPick)
            {
                isLastPick = true;               
            }

            CurrentPlayer = null;

            updateClock(isLastPick);

            updateHashtag();

            updateContent(isLastPick);
        }

        private void updateClock(bool isLastPick)
        {
            PlayerCommand commandToSend;

            //send a command to change the clock on-air
            commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));

            XmlDataRow xmlRow = new XmlDataRow();

            if (isLastPick)
            {
                xmlRow.Add("CLOCK_OVERLAY", "OVERLAY");
                xmlRow.Add("CHIP_1", ConfigurationManager.AppSettings["ClockOverlayDirectory"].ToString() + "\\" + ConfigurationManager.AppSettings["EndOfDraftChip"].ToString());
            }
            else
            {
                xmlRow.Add("ROUND_1", Global.GlobalCollections.Instance.OnTheClock.Round.ToString());
                xmlRow.Add("PICK_1", Global.GlobalCollections.Instance.OnTheClock.OverallPick.ToString());
                xmlRow.Add("LOGO_1", Global.GlobalCollections.Instance.OnTheClock.Team.LogoTgaNoKey.LocalPath);
                xmlRow.Add("SWATCH_1", Global.GlobalCollections.Instance.OnTheClock.Team.SwatchTga.LocalPath);
                xmlRow.Add("ABBREV_4_1", Global.GlobalCollections.Instance.OnTheClock.Team.Tricode);

                xmlRow.Add("CLOCK_OVERLAY", "ON THE CLOCK");
                xmlRow.Add("CLOCK", "");

                updateTotem(true);
            }

            //xmlRow.Add("CLOCK_COLOR", "NORMAL");

            commandToSend.TemplateData = xmlRow.GetXMLString();
            commandToSend.CommandID = Guid.NewGuid().ToString();

            OnSendCommand(commandToSend, null);
        }

        private void updateHashtag()
        {
            //show the hashtag on the right side
            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "RightLogo"));

            XmlDataRow xmlRow = new XmlDataRow();

            string hashtag = "";

            if (OTCHashtag && SelectedRightLogoFilename.ToUpper().IndexOf("HASHTAG") > -1)
            {
                hashtag = Global.GlobalCollections.Instance.OnTheClock.Team.Hashtag;
            }

            xmlRow.Add("TIDBIT_1", hashtag);

            commandToSend.TemplateData = xmlRow.GetXMLString();
            commandToSend.CommandID = Guid.NewGuid().ToString();

            OnSendCommand(commandToSend, null);
        }

        private void updateContent(bool isLastPick)
        {
            PlayerCommand commandToSend = new PlayerCommand();

            if (isLastPick) //show the end of draft template
            {
                //hide lower3rd templates
                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "HidePage");
                commandToSend.CommandID = Guid.NewGuid().ToString();
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Lower3rd_Connected"));
                commandToSend.Parameters.Add(new CommandParameter("QueueCommand", "true"));

                OnSendCommand(commandToSend, null);

                commandToSend = new PlayerCommand();

                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "HidePage");
                commandToSend.CommandID = Guid.NewGuid().ToString();
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Lower3rd_Separated"));

                OnSendCommand(commandToSend, null);

                commandToSend = new PlayerCommand();

                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.CommandID = Guid.NewGuid().ToString();
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "EndOfDraft"));

                OnSendCommand(commandToSend, null);
            }
            else
            {
                //hide the current selection template
                commandToSend = new PlayerCommand();

                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "HidePage");
                commandToSend.CommandID = Guid.NewGuid().ToString();
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "CurrentSelection"));

                OnSendCommand(commandToSend, null);

                //commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "HidePage");
                //commandToSend.CommandID = Guid.NewGuid().ToString();
                //commandToSend.Parameters = new List<CommandParameter>();
                //commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Lower3rd_Connected"));

                //OnSendCommand(commandToSend, null);
            }
        }

        private void updateTotem(bool changeTotem = false)
        {
            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Next"));

            XmlDataRow xmlRow = new XmlDataRow();

            Pick nextPick1;
            Pick nextPick2;

            nextPick1 = (Pick)Global.GlobalCollections.Instance.DraftOrder.SingleOrDefault(p => p.OverallPick == Global.GlobalCollections.Instance.OnTheClock.OverallPick + 1);
            nextPick2 = (Pick)Global.GlobalCollections.Instance.DraftOrder.SingleOrDefault(p => p.OverallPick == Global.GlobalCollections.Instance.OnTheClock.OverallPick + 2);

            if (nextPick1 != null)
            {
                xmlRow.Add("ABBREV_4_1", nextPick1.Team.Tricode);

                if (nextPick2 != null)
                {
                    xmlRow.Add("ABBREV_4_2", nextPick2.Team.Tricode);
                }
                else
                {
                    xmlRow.Add("ABBREV_4_2", "");
                }
            }
            else
            {
                xmlRow.Add("ABBREV_4_1", "");
                xmlRow.Add("ABBREV_4_2", "");
            }

            if (changeTotem)
            {
                xmlRow.Add("CHANGE_TOTEM_FLAG", "1");
            }
            else
            {
                xmlRow.Add("CHANGE_TOTEM_FLAG", "");
            }

            commandToSend.TemplateData = xmlRow.GetXMLString();
            commandToSend.CommandID = Guid.NewGuid().ToString();

            OnSendCommand(commandToSend, null);
        }

        private void showClock()
        {
            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.CommandID = Guid.NewGuid().ToString();
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));

            XmlDataRow xmlRow = new XmlDataRow();

            xmlRow.Add("CLOCK_OVERLAY", "CLOCK");

            commandToSend.TemplateData = xmlRow.GetXMLString();

            OnSendCommand(commandToSend, null);
        }

        private void hideEndOfDraft()
        {
            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "HidePage");
            commandToSend.CommandID = Guid.NewGuid().ToString();
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "EndOfDraft"));

            OnSendCommand(commandToSend, null);
        }

        #endregion

        #region Commands

        public ICommand ShowPickIsInCommand
        {
            get
            {
                if (_showPickIsInCommand == null)
                {
                    _showPickIsInCommand = new DelegateCommand(showPickIsIn);
                }
                return _showPickIsInCommand;
            }
        }

        public ICommand ShowCurrentSelectionCommand
        {
            get
            {
                if (_showCurrentSelectionCommand == null)
                {
                    _showCurrentSelectionCommand = new DelegateCommand(showCurrentSelection);
                }
                return _showCurrentSelectionCommand;
            }
        }

        public ICommand RevealCurrentSelectionCommand
        {
            get
            {
                if (_revealCurrentSelectionCommand == null)
                {
                    _revealCurrentSelectionCommand = new DelegateCommand(revealCurrentSelection);
                }
                return _revealCurrentSelectionCommand;
            }
        }

        public ICommand ShowPlayerTidbitsCommand
        {
            get
            {
                if (_showPlayerTidbitsCommand == null)
                {
                    _showPlayerTidbitsCommand = new DelegateCommand(showPlayerTidbits);
                }
                return _showPlayerTidbitsCommand;
            }
        }

        public ICommand NextOnTheClockCommand
        {
            get
            {
                if (_nextOnTheClockCommand == null)
                {
                    _nextOnTheClockCommand = new DelegateCommand(nextOnTheClock);
                }
                return _nextOnTheClockCommand;
            }
        }

        public ICommand ShowClockCommand
        {
            get
            {
                if (_showClockCommand == null)
                {
                    _showClockCommand = new DelegateCommand(showClock);
                }
                return _showClockCommand;
            }
        }

        public ICommand HideEndOfDraftCommand
        {
            get
            {
                if (_hideEndOfDraftCommand == null)
                {
                    _hideEndOfDraftCommand = new DelegateCommand(hideEndOfDraft);
                }
                return _hideEndOfDraftCommand;
            }
        }

        #endregion

    }
}
