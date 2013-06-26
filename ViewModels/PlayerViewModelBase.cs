using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using System.Windows.Media.Imaging;
using DraftAdmin.DataAccess;

namespace DraftAdmin.ViewModels
{
    public class PlayerViewModelBase : ViewModelBase
    {

        #region Private Members

        //protected Player _player;

        protected Int32 _playerId;
        protected string _firstName;
        protected string _lastName;
        protected string _tvName;
        protected string _position;
        protected string _positionFull;
        protected string _hometown;
        protected string _state;
        protected string _headshot;
        protected string _height;
        protected string _weight;
        protected int _kiperRank;
        protected int _mcShayRank;
        protected string _class;
        protected string _tradeTidbit;
        protected bool _isDirty;
        protected Team _school;
        protected Pick _pick;

        protected List<Tidbit> _tidbits;
        private ObservableCollection<TidbitViewModel> _tidbitVMs;

        #endregion

        #region Properties

        public Int32 PlayerId
        {
            get { return _playerId; }
            set { _playerId = value; OnPropertyChanged("PlayerId"); }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; OnPropertyChanged("FirstName"); IsDirty = true; }
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; OnPropertyChanged("LastName"); IsDirty = true; }
        }

        public string TvName
        {
            get { return _tvName; }
            set { _tvName = value; OnPropertyChanged("TvName"); IsDirty = true; }
        }

        public string Position
        {
            get { return _position; }
            set { _position = value; OnPropertyChanged("Position"); IsDirty = true; }
        }

        public string PositionFull
        {
            get { return _positionFull; }
            set { _positionFull = value; OnPropertyChanged("PositionFull"); IsDirty = true; }
        }

        public string Hometown
        {
            get { return _hometown; }
            set { _hometown = value; OnPropertyChanged("Hometown"); IsDirty = true; }
        }

        public string State
        {
            get { return _state; }
            set { _state = value; OnPropertyChanged("State"); IsDirty = true; }
        }

        public string Headshot
        {
            get { return _headshot; }
            set { _headshot = value; OnPropertyChanged("Headshot"); IsDirty = true; }
        }

        public string Height
        {
            get { return _height; }
            set { _height = value; OnPropertyChanged("Height"); IsDirty = true; }
        }

        public string Weight
        {
            get { return _weight; }
            set { _weight = value; OnPropertyChanged("Weight"); IsDirty = true; }
        }

        public string Class
        {
            get { return _class; }
            set { _class = value; OnPropertyChanged("Class"); IsDirty = true; }
        }

        public string TradeTidbit
        {
            get { return _tradeTidbit; }
            set { _tradeTidbit = value; OnPropertyChanged("TradeTidbit"); IsDirty = true; }
        }

        public int KiperRank
        {
            get { return _kiperRank; }
            set { _kiperRank = value; OnPropertyChanged("Rank"); IsDirty = true; }
        }

        public int McShayRank
        {
            get { return _mcShayRank; }
            set { _mcShayRank = value; OnPropertyChanged("McShayRank"); IsDirty = true; }
        }

        public Team School
        {
            get { return _school; }
            set { _school = value; OnPropertyChanged("School"); }
        }

        public Pick Pick
        {
            get { return _pick; }
            set { _pick = value; OnPropertyChanged("Pick"); }
        }

        public List<Tidbit> Tidbits
        {
            get { return _tidbits; }
            set 
            { 
                _tidbits = value;
                loadTidbits();
            }
        }

        public ObservableCollection<TidbitViewModel> TidbitVMs
        {
            get { return _tidbitVMs; }
            set { _tidbitVMs = value; OnPropertyChanged("TidbitVMs"); }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        #endregion

        #region Constructor

        public PlayerViewModelBase(Player player)
        {
            //_player = player;

            if (player != null)
            {
                _playerId = player.PlayerId;
                _firstName = player.FirstName;
                _lastName = player.LastName;
                _tvName = player.TvName;
                _hometown = player.Hometown;
                _state = player.State;
                _position = player.Position;
                _positionFull = player.PositionFull;
                _height = player.Height;
                _weight = player.Weight;
                _class = player.Class;
                _headshot = player.Headshot;
                _kiperRank = player.KiperRank;
                _mcShayRank = player.McShayRank;
                _school = player.School;
                _pick = player.Pick;
                _tradeTidbit = player.TradeTidbit;

                _tidbits = player.Tidbits;

                loadTidbits();
            }
        }

        #endregion

        #region Protected Methods

        

        #endregion

        #region Private Methods

        protected void loadTidbits()
        {
            //if (_tidbits == null)
            //{
            //    PlayerTidbits = new ObservableCollection<TidbitViewModel>();
            //}

            //PlayerTidbits.Clear();

            if (_tidbitVMs == null)
            {
                _tidbitVMs = new ObservableCollection<TidbitViewModel>();
            }
            else
            {
                _tidbitVMs.Clear();
            }

            if (_tidbits != null)
            {
                foreach (Tidbit tidbit in _tidbits)
                {
                    _tidbitVMs.Add(new TidbitViewModel(tidbit));
                }
            }            
        }

        #endregion

    }
}
