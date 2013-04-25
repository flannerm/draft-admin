using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using System.Windows.Input;
using DraftAdmin.Commands;
using DraftAdmin.DataAccess;
using DraftAdmin.PlayoutCommands;
using System.Collections.ObjectModel;
using System.Timers;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;

namespace DraftAdmin.ViewModels
{
    public class PlaylistTabViewModel : ViewModelBase
    {
        #region Private Members

        private Playlist _selectedPlaylist;
        private Playlist _playlistToLoad;
        private PlaylistItem _selectedPlaylistItem;
        private ObservableCollection<Playlist> _playlists;
        private ObservableCollection<Playlist> _loadedPlaylists;

        private DelegateCommand _loadPlaylistCommand;
        private DelegateCommand _removePlaylistCommand;
        private DelegateCommand _jumpToItemCommand;
        private DelegateCommand _disablePollCommand;
        private DelegateCommand _enablePollCommand;

        //private string _playoutFeedack;

        public delegate void JumpToItemEventHandler(int playlistItemOrder);
        public event JumpToItemEventHandler JumpToItemEvent;

        private string _timerText = "";

        private string _lastTemplateData = "";

        #endregion

        #region Properties

        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set { _selectedPlaylist = value;  OnPropertyChanged("SelectedPlaylist"); }
        }

        public Playlist PlaylistToLoad
        {
            get { return _playlistToLoad; }
            set { _playlistToLoad = value; OnPropertyChanged("PlaylistToLoad"); }
        }

        public ObservableCollection<Playlist> Playlists
        {
            get { return _playlists; }
            set { _playlists = value; OnPropertyChanged("Playlists"); }
        }

        public ObservableCollection<Playlist> LoadedPlaylists
        {
            get { return _loadedPlaylists; }
            set { _loadedPlaylists = value; OnPropertyChanged("LoadedPlaylists"); }
        }

        public PlaylistItem SelectedPlaylistItem
        {
            get { return _selectedPlaylistItem; }
            set { _selectedPlaylistItem = value; OnPropertyChanged("SelectedPlaylistItem"); }
        }

        public string TimerText
        {
            get { return _timerText; }
            set { _timerText = value; OnPropertyChanged("TimerText"); }
        }

        #endregion

        #region Constructor

        public PlaylistTabViewModel()
        {

        }

        #endregion

        #region Private Methods

        private void loadPlaylist()
        {
            if (_loadedPlaylists == null)
            {
                LoadedPlaylists = new ObservableCollection<Playlist>();
            }

            _playlistToLoad.PlaylistItems = DbConnection.GetPlaylistItems(_playlistToLoad.PlaylistID);

            int playlistID = _playlistToLoad.PlaylistID;

            _playlistToLoad.Timer = new System.Timers.Timer();
            _playlistToLoad.Timer.Elapsed += (sender, e) => playlistTimer_Elapsed(sender, e, playlistID);

            LoadedPlaylists.Add(_playlistToLoad);
        }

        private void removePlaylist()
        {
            if (_selectedPlaylist != null)
            {
                LoadedPlaylists.Remove(_selectedPlaylist);
            }
        }

        private void jumpToItem()
        {
            OnJumpToItemEvent(_selectedPlaylistItem.PlaylistOrder);
        }

        private void OnJumpToItemEvent(int playlistItemOrder)
        {
            JumpToItemEventHandler handler = JumpToItemEvent;

            if (handler != null)
            {
                handler(playlistItemOrder);
            }
        }

        private void disablePollItems()
        {
            DbConnection.DisablePollItems(_selectedPlaylist.PlaylistID);

            int selectedPlaylistItem = _selectedPlaylist.CurrentPlaylistItem;

            SelectedPlaylist.PlaylistItems = DbConnection.GetPlaylistItems(_selectedPlaylist.PlaylistID);

            if (selectedPlaylistItem <= SelectedPlaylist.PlaylistItems.Count)
            {
                SelectedPlaylist.CurrentPlaylistItem = selectedPlaylistItem;
            }
        }

        private void enablePollItems()
        {
            DbConnection.EnablePollItems(_selectedPlaylist.PlaylistID);

            int selectedPlaylistItem = _selectedPlaylist.CurrentPlaylistItem;

            SelectedPlaylist.PlaylistItems = DbConnection.GetPlaylistItems(_selectedPlaylist.PlaylistID);

            if (selectedPlaylistItem <= SelectedPlaylist.PlaylistItems.Count)
            {
                SelectedPlaylist.CurrentPlaylistItem = selectedPlaylistItem;
            }
        }

        private void playlistTimer_Elapsed(object sender, ElapsedEventArgs e, int playlistID)
        {
            Debug.Print("PlaylistID " + playlistID + " timer elapsed");

            System.Timers.Timer timer = (System.Timers.Timer)sender;

            timer.Stop();

            Playlist playlist = _loadedPlaylists.SingleOrDefault(p => p.PlaylistID == playlistID);

            nextPlaylistItem(playlist);
        }

        private void nextPlaylistItem(Playlist playlist, bool forceRestart = false)
        {
            PlaylistItem playlistItem = null;

            try
            {
                if (playlist.TimerRunning == true || forceRestart) //MJF
                {
                    if (playlist.CurrentPlaylistItem >= playlist.PlaylistItems.Count)
                    {
                        playlist.CurrentPlaylistItem = 0;
                    }

                    playlistItem = playlist.PlaylistItems[playlist.CurrentPlaylistItem];

                    playlistItem.OnAir = false;

                    if (playlistItem.Enabled == false)
                    {
                        playlist.CurrentPlaylistItem += 1;
                        nextPlaylistItem(playlist);
                    }
                    else
                    {
                        playlistItem.XmlDataRows = DbConnection.GetPlaylistItemData(playlistItem);

                        if (playlistItem.XmlDataRows != null)
                        {
                            if (playlistItem.XmlDataRows.Count > 0) //check to see if there are any rows in this playlist item to show
                            {
                                if (playlistItem.CurrentRow >= playlistItem.XmlDataRows.Count) //at the end of this playlist item's data, go to the next item
                                {
                                    playlistItem.CurrentRow = 0;
                                    playlist.CurrentPlaylistItem += 1;
                                    nextPlaylistItem(playlist);
                                }
                                else
                                {
                                    PlayerCommand commandToSend = new PlayerCommand();

                                    commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                                    commandToSend.CommandID = Guid.NewGuid().ToString();
                                    commandToSend.Parameters = new List<CommandParameter>();
                                    commandToSend.Parameters.Add(new CommandParameter("TemplateName", playlistItem.Template));

                                    commandToSend.TemplateData = playlistItem.XmlDataRows[playlistItem.CurrentRow].GetXMLString();

                                    playlist.Timer.Interval = Convert.ToDouble((playlistItem.Duration * 1000));

                                    if (playlistItem.MergeDataNoTransitions)
                                    {
                                        commandToSend.Parameters.Add(new CommandParameter("MergeDataWithoutTransitions", "true"));

                                        OnSendCommandNoTransitions(commandToSend);

                                        //Debug.Print("Clock sent to PageEngine: " + DateTime.Now);

                                        if (playlist.TimerRunning)
                                        {
                                            playlist.Timer.Start();
                                        }
                                    }
                                    else
                                    {
                                        //don't send data to the playout for the prompter if it hasn't changed.  too many graphics causes the clock to choke
                                        if (playlistItem.Template.ToUpper() == "PROMPTER")
                                        {
                                            if (_lastTemplateData != commandToSend.TemplateData)
                                            {
                                                _lastTemplateData = commandToSend.TemplateData;
                                                OnSendCommand(commandToSend, playlist);
                                            }
                                            else
                                            {
                                                if (playlist.TimerRunning)
                                                {
                                                    playlist.Timer.Start();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            OnSendCommand(commandToSend, playlist);
                                        }

                                        //Debug.Print(playlistItem.Template.ToString() + " sent to PageEngine");
                                    }

                                    //SelectedPlaylist.PlaylistItems[SelectedPlaylist.CurrentPlaylistItem].OnAir = true;

                                    //OnPropertyChanged("SelectedPlaylist.PlaylistItems");
                                    playlistItem.OnAir = true;


                                    playlistItem.CurrentRow += 1;
                                }

                            }
                            else
                            {
                                if (playlist.PlaylistItems.Count > 1)
                                {
                                    playlistItem.CurrentRow = 0;
                                    playlist.CurrentPlaylistItem += 1;
                                    nextPlaylistItem(playlist, forceRestart);
                                }
                            }

                        } //playlistItem.XmlDataRows != null

                    } //playlistItem.Enabled == false

                } //playlist.TimerRunning
            }
            catch (Exception ex)
            {
                if (playlistItem != null)
                {
                    playlistItem.CurrentRow = 0;
                }

                if (playlist != null)
                {
                    playlist.CurrentPlaylistItem += 1;

                    nextPlaylistItem(playlist);
                }
            }

            //OnPropertyChanged("SelectedPlaylist");
        }

        #endregion

        #region Public Methods

        public void LoadPlaylists()
        {
            Playlists = DbConnection.GetPlaylists();
        }

        public void ResetPlaylists()
        {
            if (_loadedPlaylists != null)
            {
                
                foreach (Playlist playlist in _loadedPlaylists)
                {
                    playlist.PlaylistItems[playlist.CurrentPlaylistItem].CurrentRow = 0;
                    playlist.CurrentPlaylistItem = 0;

                    foreach (PlaylistItem item in playlist.PlaylistItems)
                    {
                        item.OnAir = false;
                    }

                    nextPlaylistItem(playlist, true);
                }

                //if (_currentL3PlaylistItem < GlobalCollections.Instance.PlaylistItems.Count)
                //{
                //    GlobalCollections.Instance.PlaylistItems[_currentL3PlaylistItem].CurrentRow = 0; //reset the current playlist item
                //}

                //_currentL3PlaylistItem = 0;

                //nextL3Item(false);

                //setStatusBarMsg("Cycle reset at " + DateTime.Now.ToLongTimeString(), "Green");
            }        
        }

        #endregion

        #region Commands

        public ICommand LoadPlaylistCommand
        {
            get
            {
                if (_loadPlaylistCommand == null)
                {
                    _loadPlaylistCommand = new DelegateCommand(loadPlaylist);
                }
                return _loadPlaylistCommand;
            }
        }

        public ICommand RemovePlaylistCommand
        {
            get
            {
                if (_removePlaylistCommand == null)
                {
                    _removePlaylistCommand = new DelegateCommand(removePlaylist);
                }
                return _removePlaylistCommand;
            }
        }

        public ICommand JumpToItemCommand
        {
            get
            {
                if (_jumpToItemCommand == null)
                {
                    _jumpToItemCommand = new DelegateCommand(jumpToItem);
                }
                return _jumpToItemCommand;
            }
        }

        public ICommand DisablePollCommand
        {
            get
            {
                if (_disablePollCommand == null)
                {
                    _disablePollCommand = new DelegateCommand(disablePollItems);
                }
                return _disablePollCommand;
            }
        }

        public ICommand EnablePollCommand
        {
            get
            {
                if (_enablePollCommand == null)
                {
                    _enablePollCommand = new DelegateCommand(enablePollItems);
                }
                return _enablePollCommand;
            }
        }

        #endregion
    }
}
