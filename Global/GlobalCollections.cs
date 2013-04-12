using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using DraftAdmin.Models;
using DraftAdmin.DataAccess;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Configuration;
using System.IO;
using System.Drawing;
using DraftAdmin.Utilities;
using System.Windows.Threading;
using DraftAdmin.PlayoutCommands;
using System.Timers;
using System.Diagnostics;

namespace DraftAdmin.Global
{
    public sealed class GlobalCollections : INotifyPropertyChanged
    {
        private static GlobalCollections _instance = new GlobalCollections();

        private GlobalCollections() { }

        public static GlobalCollections Instance
        {
            get { return _instance; }
        }

        #region Private Members

        private ObservableCollection<Player> _players;
        private ObservableCollection<Team> _schools;
        private ObservableCollection<Team> _teams;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Category> _interruptions;
        private ObservableCollection<Pick> _draftOrder;
        private Dictionary<Int32, BitmapImage> _logos;

        //private Dictionary<Playlist, Timer> _playlistTimers;

        private Pick _onTheClock;

        private int _lastPick;

        #endregion

        #region Properties

        public ObservableCollection<Player> Players
        {
            get { return _players; }
            set { _players = value; OnPropertyChanged("Players"); }
        }

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

        public ObservableCollection<Category> Categories
        {
            get { return _categories; }
            set { _categories = value; OnPropertyChanged("Categories"); }
        }

        public ObservableCollection<Category> Interruptions
        {
            get { return _interruptions; }
            set { _interruptions = value; OnPropertyChanged("Interruptions"); }
        }

        public ObservableCollection<Pick> DraftOrder
        {
            get { return _draftOrder; }
            set { _draftOrder = value; OnPropertyChanged("DraftOrder"); }           
        }

        public Pick OnTheClock
        {
            get { return _onTheClock; }
            set { _onTheClock = value; OnPropertyChanged("OnTheClock"); }
        }

        public Dictionary<Int32, BitmapImage> Logos
        {
            get { return _logos; }
            set { _logos = value; OnPropertyChanged("Logos"); }
        }

        public int LastPick
        {
            get { return _lastPick; }
        }

        #endregion

        #region Public Methods

        public void LoadSchools(BackgroundWorker worker = null)
        {
            ObservableCollection<Team> schools = DbConnection.GetSchools(worker);
            Schools = schools;
        }

        public void LoadTeams(BackgroundWorker worker = null)
        {
            ObservableCollection<Team> teams = DbConnection.GetProTeams(worker);
            Teams = teams;
        }

        public void LoadPlayers(BackgroundWorker worker = null)
        {
            ObservableCollection<Player> players = DbConnection.GetPlayers(worker);
            Players = players;
        }

        public void LoadDraftOrder()
        {
            DraftOrder = DbConnection.GetDraftOrder(_teams);
        }

        public void LoadCategories()
        {
            Categories = DbConnection.GetCategories(1);
        }

        public void LoadInterruptions()
        {
            Interruptions = DbConnection.GetCategories(2);
        }

        public void LoadOnTheClock()
        {
            try
            {
                int currentPickNum = 0;

                if (_draftOrder != null)
                {
                    if (_draftOrder.Count > 0)
                    {
                        currentPickNum = _players.Where(p => p.Pick != null).DefaultIfEmpty().Max(p => p == null ? 0 : p.Pick.OverallPick);

                        OnTheClock = (Pick)_draftOrder.SingleOrDefault(p => p.OverallPick == currentPickNum + 1);

                        _lastPick = _draftOrder.Max(p => p.OverallPick);
                    }
                }
            }
            catch (Exception ex)
            { }

        }

        public void LoadLogos()
        {
            ObservableCollection<Team> teams = DbConnection.GetProTeams();
            ObservableCollection<Team> schools = DbConnection.GetSchools();

            var allTeams = teams.Concat(schools);

            _logos = new Dictionary<int, BitmapImage>();

            foreach (Team team in allTeams)
            {
                string logoFilePath = team.LogoTga.LocalPath;

                logoFilePath = logoFilePath.Replace("\\\\HEADSHOT01\\IMAGES", ConfigurationManager.AppSettings["ImsDirectory"].ToString());

                if (File.Exists(logoFilePath))
                {
                    Bitmap bmp = TargaImage.LoadTargaImage(logoFilePath);
                    var strm = new System.IO.MemoryStream();
                    bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Bmp);

                    BitmapImage logoBitmap = new BitmapImage();
                    logoBitmap.BeginInit();
                    logoBitmap.StreamSource = strm;
                    logoBitmap.EndInit();

                    _logos.Add(team.ID, logoBitmap);

                }               
            }
        }

        #endregion

        #region Private Methods

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
