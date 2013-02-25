using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Commands;
using DraftAdmin.Models;
using DraftAdmin.DataAccess;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace DraftAdmin.ViewModels
{
    public class PlayerTabViewModel : ViewModelBase
    {
        #region Private Members

        private Player _selectedPlayer;
        private Player _selectedPlayerTemp;
        private PlayerEditViewModel _selectedPlayerEditVM;

        private Player _playerToDelete;

        private DelegateCommand _addPlayerCommand;
        private DelegateCommand _deletePlayerCommand;
        private DelegateCommand _clearFilterCommand;
        private DelegateCommand _refreshPlayersCommand;

        private bool _askSaveOnDirty = false;
        private bool _askDeletePlayer = false;

        public delegate void DraftPlayerEventHandler(Int32 playerId);

        public event DraftPlayerEventHandler DraftPlayerEvent;

        private string _firstNameFilter = "";
        private string _lastNameFilter = "";
        private string _positionFilter = "";
        private string _schoolFilter = "";

        private ObservableCollection<Player> _filteredPlayers;

        #endregion

        #region Public Members

        public DelegateCommand<object> SaveChanges { get; set; }
        public DelegateCommand<object> DiscardChanges { get; set; }

        public DelegateCommand<object> DeletePlayer { get; set; }
        public DelegateCommand<object> CancelDeletePlayer { get; set; }

        #endregion

        #region Properties

        public bool AskSaveOnDirty
        {
            get { return _askSaveOnDirty; }
            set { _askSaveOnDirty = value; OnPropertyChanged("AskSaveOnDirty"); }
        }

        public bool AskDeletePlayer
        {
            get { return _askDeletePlayer; }
            set { _askDeletePlayer = value; OnPropertyChanged("AskDeletePlayer"); }
        }

        public Player SelectedPlayer
        {
            get { return _selectedPlayer; }
            set
            {
                if (value != null && value != _selectedPlayer && _selectedPlayer != null && _selectedPlayer.IsDirty == true)
                {
                    _selectedPlayerTemp = value;
                    PromptMessage = "Save changes to " + _selectedPlayer.FirstName + " " + _selectedPlayer.LastName + "?";
                    AskSaveOnDirty = true;
                }
                else
                {
                    selectPlayer(value);
                }
            }
        }

        public PlayerEditViewModel SelectedPlayerEditVM
        {
            get { return _selectedPlayerEditVM; }
            set { _selectedPlayerEditVM = value; OnPropertyChanged("SelectedPlayerEditVM"); }
        }

        public string FirstNameFilter
        {
            get { return _firstNameFilter; }
            set { _firstNameFilter = value; OnPropertyChanged("FirstNameFilter"); filterPlayers(); }
        }

        public string LastNameFilter
        {
            get { return _lastNameFilter; }
            set { _lastNameFilter = value; OnPropertyChanged("LastNameFilter"); filterPlayers(); }
        }

        public string PositionFilter
        {
            get { return _positionFilter; }
            set { _positionFilter = value; OnPropertyChanged("PositionFilter"); filterPlayers(); }
        }

        public string SchoolFilter
        {
            get { return _schoolFilter; }
            set { _schoolFilter = value; OnPropertyChanged("SchoolFilter");  filterPlayers(); }
        }

        public ObservableCollection<Player> FilteredPlayers
        {
            get { return _filteredPlayers; }
            set { _filteredPlayers = value; OnPropertyChanged("FilteredPlayers"); }
        }

        #endregion

        #region Constructor

        public PlayerTabViewModel()
        {
            FilteredPlayers = Global.GlobalCollections.Instance.Players;

            SaveChanges = new DelegateCommand<object>(saveChangesAction);
            DiscardChanges = new DelegateCommand<object>(discardChangesAction);

            DeletePlayer = new DelegateCommand<object>(deletePlayerAction);
            CancelDeletePlayer = new DelegateCommand<object>(cancelDeletePlayerAction);
        }

        #endregion

        #region Private Methods

        private void filterPlayers()
        {
            var query = Global.GlobalCollections.Instance.Players.Where(p => Regex.IsMatch(p.LastName, _lastNameFilter, RegexOptions.IgnoreCase) && Regex.IsMatch(p.FirstName, _firstNameFilter, RegexOptions.IgnoreCase) && Regex.IsMatch(p.Position, _positionFilter, RegexOptions.IgnoreCase) && (p.School != null && Regex.IsMatch(Convert.ToString(p.School.Name), _schoolFilter, RegexOptions.IgnoreCase)));

            var filteredPlayers = new ObservableCollection<Player>(query);

            FilteredPlayers = filteredPlayers;
        }

        private void clearFilter()
        {
            PositionFilter = "";
            FirstNameFilter = "";
            LastNameFilter = "";
            SchoolFilter = "";
        }

        private void saveChangesAction(object parameter)
        {
            AskSaveOnDirty = false;

            savePlayer();

            _selectedPlayer.IsDirty = false;
        }

        private void discardChangesAction(object parameter)
        {
            AskSaveOnDirty = false;

            _selectedPlayer = DbConnection.GetPlayer(_selectedPlayer.PlayerId);

            _selectedPlayer.IsDirty = false;

            selectPlayer(_selectedPlayerTemp);
        }

        private void deletePlayerAction(object parameter)
        {
            AskDeletePlayer = false;

            _playerToDelete = _selectedPlayer;

            if (DbConnection.DeletePlayer(_selectedPlayer) == true)
            {
                refreshPlayers();
                OnSetStatusBarMsg(_playerToDelete.FirstName + " " + _playerToDelete.LastName + " deleted", "Green");
            }
            else
            {
                OnSetStatusBarMsg("Error deleting " + _selectedPlayer.FirstName + " " + _selectedPlayer.LastName, "Red");
            }
        }

        private void cancelDeletePlayerAction(object parameter)
        {
            AskDeletePlayer = false;
        }          

        private void addPlayer()
        {
            SelectedPlayer = null;

            SelectedPlayerEditVM = new PlayerEditViewModel(null);
            SelectedPlayerEditVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(OnSetStatusBarMsg);
            SelectedPlayerEditVM.DraftPlayerEvent += new PlayerEditViewModel.DraftPlayerEventHandler(draftPlayer);
            SelectedPlayerEditVM.RefreshPlayersEvent += new PlayerEditViewModel.RefreshPlayersEventHandler(refreshPlayers);

            //if (DbConnection.AddPlayer() == true)
            //{
            //    OnSetStatusBarMsg("New player added.", "Green");
            //    Global.GlobalCollections.Instance.LoadPlayers();
            //}
            //else
            //{
            //    OnSetStatusBarMsg("Error adding new player.", "Red");
            //}
        }

        private void deletePlayer()
        {
            if (_selectedPlayer != null)
            {
                PromptMessage = "Delete " + _selectedPlayer.FirstName + " " + _selectedPlayer.LastName + "?";
                AskDeletePlayer = true;
            }
        }

        private void savePlayer()
        {
            if (DbConnection.SavePlayer(_selectedPlayer) == true)
            {
                OnSetStatusBarMsg(_selectedPlayer.FirstName + " " + _selectedPlayer.LastName + " saved at " + DateTime.Now.ToShortTimeString(), "Green");
            }
            else
            {
                OnSetStatusBarMsg("Error saving " + _selectedPlayer.FirstName + " " + _selectedPlayer.LastName + ".", "Red");
            }
        }

        private void selectPlayer(Player player)
        {
            _selectedPlayer = player;

            if (_selectedPlayer != null)
            {
                SelectedPlayerEditVM = new PlayerEditViewModel(_selectedPlayer);
                SelectedPlayerEditVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(OnSetStatusBarMsg);
                SelectedPlayerEditVM.DraftPlayerEvent += new PlayerEditViewModel.DraftPlayerEventHandler(draftPlayer);
                SelectedPlayerEditVM.RefreshPlayersEvent += new PlayerEditViewModel.RefreshPlayersEventHandler(refreshPlayers);
            }
        }

        private void draftPlayer(Int32 playerId)
        {
            OnDraftPlayer(playerId);
        }

        private void OnDraftPlayer(Int32 playerId)
        {
            DraftPlayerEventHandler handler = DraftPlayerEvent;

            if (handler != null)
            {
                handler(playerId);
            }
        }

        private void refreshPlayers()
        {
            Global.GlobalCollections.Instance.LoadPlayers();

            filterPlayers();
        }

        #endregion

        #region Public Methods

        public void RefreshPlayers()
        {
            clearFilter();
        }

        #endregion

        #region Commands

        public ICommand AddPlayerCommand
        {
            get
            {
                if (_addPlayerCommand == null)
                {
                    _addPlayerCommand = new DelegateCommand(addPlayer);
                }
                return _addPlayerCommand;
            }
        }

        public ICommand DeletePlayerCommand
        {
            get
            {
                if (_deletePlayerCommand == null)
                {
                    _deletePlayerCommand = new DelegateCommand(deletePlayer);
                }
                return _deletePlayerCommand;
            }
        }

        public ICommand ClearFilterCommand
        {
            get
            {
                if (_clearFilterCommand == null)
                {
                    _clearFilterCommand = new DelegateCommand(clearFilter);
                }
                return _clearFilterCommand;
            }
        }

        public ICommand RefreshPlayersCommand
        {
            get
            {
                if (_refreshPlayersCommand == null)
                {
                    _refreshPlayersCommand = new DelegateCommand(refreshPlayers);
                }
                return _refreshPlayersCommand;
            }
        }

        #endregion

    }
}
