using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace DraftAdmin.Models
{
    public class Playlist : ModelBase
    {
        #region Private Members

        private int _playlistId;
        private string _playlistName;
        private ObservableCollection<PlaylistItem> _playlistItems;
        private int _currentPlaylistItem = 0;
        private System.Timers.Timer _timer;
        private bool _timerRunning = false;

        #endregion

        #region Properties

        public int PlaylistID
        {
            get { return _playlistId; }
            set { _playlistId = value; OnPropertyChanged("PlaylistID"); }
        }

        public string PlaylistName
        {
            get { return _playlistName; }
            set { _playlistName = value; OnPropertyChanged("PlaylistName"); }
        }
        
        public ObservableCollection<PlaylistItem> PlaylistItems
        {
            get { return _playlistItems; }
            set { _playlistItems = value; OnPropertyChanged("PlaylistItems"); }
        }

        public int CurrentPlaylistItem
        {
            get { return _currentPlaylistItem; }
            set { _currentPlaylistItem = value; OnPropertyChanged("CurrentPlaylistItem"); }
        }

        public System.Timers.Timer Timer
        {
            get { return _timer; }
            set { _timer = value; OnPropertyChanged("Timer"); }
        }

        public bool TimerRunning
        {
            get { return _timerRunning; }
            set { _timerRunning = value; OnPropertyChanged("TimerRunning"); }
        }

        #endregion

    }
}
