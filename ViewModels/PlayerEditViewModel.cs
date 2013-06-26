using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using System.Collections.ObjectModel;
using DraftAdmin.DataAccess;
using System.Windows.Input;
using DraftAdmin.Commands;
using System.Configuration;

namespace DraftAdmin.ViewModels
{
    public class PlayerEditViewModel : PlayerViewModelBase
    {
        #region Private Members

        private ObservableCollection<Team> _schools;
        private ObservableCollection<Team> _teams;

        private TidbitViewModel _selectedTidbit;

        private DelegateCommand _savePlayerCommand;
        private DelegateCommand _addTidbitCommand;
        private DelegateCommand _deleteTidbitCommand;
        private DelegateCommand _draftPlayerCommand;

        public DelegateCommand<object> DraftPlayer { get; set; }
        public DelegateCommand<object> CancelDraftPlayer { get; set; }
        public DelegateCommand<object> CloseMessagePrompt { get; set; }

        public delegate void DraftPlayerEventHandler(Int32 playerId);
        public delegate void RefreshPlayersEventHandler();

        public event DraftPlayerEventHandler DraftPlayerEvent;
        public event RefreshPlayersEventHandler RefreshPlayersEvent;

        private bool _askDraftPlayer = false;
        private bool _showMessagePrompt = false;

        private string _rank1Title;
        private string _rank2Title;

        #endregion

        #region Properties

        public ObservableCollection<Team> Schools
        {
            get { return _schools; }
            set { _schools = value; OnPropertyChanged("Schools"); }
        }

        public ObservableCollection<Team> Teams
        {
            get { return _teams; }
            set { _teams = value; OnPropertyChanged("Teams"); }
        }

        public TidbitViewModel SelectedTidbit
        {
            get { return _selectedTidbit; }
            set { _selectedTidbit = value; OnPropertyChanged("SelectedTidbit"); }
        }

        public bool AskDraftPlayer
        {
            get { return _askDraftPlayer; }
            set { _askDraftPlayer = value; OnPropertyChanged("AskDraftPlayer"); }
        }

        public bool ShowMessagePrompt
        {
            get { return _showMessagePrompt; }
            set { _showMessagePrompt = value; OnPropertyChanged("ShowMessagePrompt"); }
        }

        public string Rank1Title
        {
            get { return _rank1Title; }
            set { _rank1Title = value; OnPropertyChanged("Rank1Title"); }
        }

        public string Rank2Title
        {
            get { return _rank2Title; }
            set { _rank2Title = value; OnPropertyChanged("Rank2Title"); }
        }

        #endregion

        #region Constructor

        public PlayerEditViewModel(Player player) : base(player)
        {
            switch (ConfigurationManager.AppSettings["DraftType"].ToString().ToUpper())
            {
                case "NBA":
                    _rank1Title = "Jay Rank:";
                    _rank2Title = "Fran Rank:";
                    break;
                case "NFL":
                    _rank1Title = "Kiper Rank:";
                    _rank2Title = "McShay Rank:";
                    break;
            }


            DraftPlayer = new DelegateCommand<object>(draftPlayerAction);
            CancelDraftPlayer = new DelegateCommand<object>(cancelDraftPlayerAction);

            CloseMessagePrompt = new DelegateCommand<object>(closeMessagePrompt);

            //_player = player;                    
        }

        #endregion

        #region Private Methods

        private void updateTidbits()
        {
            Tidbits = DbConnection.GetTidbitsSDR(1, _playerId);
        }

        private void savePlayer()
        {
            Player player = new Player();

            player.FirstName = _firstName;
            player.LastName = _lastName;
            player.TvName = _tvName;
            player.Position = _position;
            player.PositionFull = _positionFull;
            player.Height = _height;
            player.Weight = _weight;
            player.Headshot = _headshot;
            player.School = _school;
            player.Tidbits = _tidbits;
            player.KiperRank = _kiperRank;
            player.McShayRank = _mcShayRank;
            player.Class = _class;
            player.TradeTidbit = _tradeTidbit;

            if (_playerId == 0) //add a new player, get the new ID from SDR
            {
                player.PlayerId = Convert.ToInt32(DbConnection.AddPlayer(player));
            }
            else
            {
                player.PlayerId = _playerId;
            }

            if (player.PlayerId > 0)
            {
                if (DbConnection.SavePlayer(player) == true)
                {
                    OnSetStatusBarMsg(_firstName + " " + _lastName + " saved at " + DateTime.Now.ToLongTimeString(), "Green");
                    refreshPlayers();
                }
                else
                {
                    OnSetStatusBarMsg("Error saving " + _firstName + " " + _lastName + ".", "Red");
                }
            }
            else
            {
                OnSetStatusBarMsg("Error saving " + _firstName + " " + _lastName + ".  PlayerID = 0.", "Red");
            }
            

            IsDirty = false;
        }

        private void refreshPlayers()
        {
            OnRefreshPlayers();
        }

        private void addTidbit()
        {
            if (DbConnection.AddTidbitSDR(1, _playerId) == true)
            {
                OnSetStatusBarMsg(_firstName + " " + _lastName + " - tidbit added.", "Green");
                updateTidbits();
                loadTidbits();
            }

        }

        private void deleteTidbit()
        {
            if (_selectedTidbit != null)
            {
                if (DbConnection.DeleteTidbitSDR(_selectedTidbit.ReferenceType, _playerId, _selectedTidbit.TidbitOrder) == true)
                {
                    OnSetStatusBarMsg(_firstName + " " + _lastName + " tidbits updated.", "Green");
                    updateTidbits();
                    loadTidbits();
                }
            }
        }

        private void draftPlayer()
        {
            if (_pick == null)
            {
                Pick currentPick = Global.GlobalCollections.Instance.OnTheClock;

                PromptMessage = "Select " + _firstName + " " + _lastName + " from " + _school.Name + " with the #" + currentPick.OverallPick.ToString() + "(" + currentPick.Team.Name + ") pick?";

                AskDraftPlayer = true;
            }
            else
            {
                PromptMessage = _firstName + " " + _lastName + " has already been picked by the " + _pick.Team.Name + "!";

                ShowMessagePrompt = true;                
            }
        }

        private void draftPlayerAction(object parameter)
        {
            AskDraftPlayer = false;

            //add the player to the current selection tab
            OnDraftPlayer(_playerId);
        }

        private void OnDraftPlayer(Int32 playerId)
        {
            DraftPlayerEventHandler handler = DraftPlayerEvent;

            if (handler != null)
            {
                handler(playerId);
            }
        }

        private void OnRefreshPlayers()
        {
            RefreshPlayersEventHandler handler = RefreshPlayersEvent;

            if (handler != null)
            {
                handler();
            }
        }

        private void cancelDraftPlayerAction(object parameter)
        {
            AskDraftPlayer = false;
        }

        private void closeMessagePrompt(object parameter)
        {
            ShowMessagePrompt = false;
        }

        #endregion

        #region Commands

        public ICommand SavePlayerCommand
        {
            get
            {
                if (_savePlayerCommand == null)
                {
                    _savePlayerCommand = new DelegateCommand(savePlayer);
                }
                return _savePlayerCommand;
            }
        }

        public ICommand AddTidbitCommand
        {
            get
            {
                if (_addTidbitCommand == null)
                {
                    _addTidbitCommand = new DelegateCommand(addTidbit);
                }
                return _addTidbitCommand;
            }
        }

        public ICommand DeleteTidbitCommand
        {
            get
            {
                if (_deleteTidbitCommand == null)
                {
                    _deleteTidbitCommand = new DelegateCommand(deleteTidbit);
                }
                return _deleteTidbitCommand;
            }
        }

        public ICommand DraftPlayerCommand
        {
            get
            {
                if (_draftPlayerCommand == null)
                {
                    _draftPlayerCommand = new DelegateCommand(draftPlayer);
                }
                return _draftPlayerCommand;
            }
        }

        #endregion
    }
}
