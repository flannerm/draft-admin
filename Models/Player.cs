using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DraftAdmin.Models
{
    public class Player : ModelBase
    {

        #region Private Members

        private Int32 _playerId;
        private string _firstName;
        private string _lastName;
        private string _tvName;
        private string _hometown;
        private Team _school;
        //private Team _nflTeam;
        private Pick _pick;
        private int _kiperRank;
        private int _mcshayRank;
        private string _position;
        private string _positionFull;
        private string _state;
        private string _headshot;
        private string _height;
        private string _weight;
        private string _class;
        private string _tradeTidbit;
        private List<Tidbit> _tidbits;
        private bool _isDirty = false;

        #endregion

        #region Properties

        public Int32 PlayerId
        {
            get { return _playerId; }
            set { _playerId = value; }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        public string TvName
        {
            get { return _tvName; }
            set { _tvName = value; }
        }

        public string Position
        {
            get { return _position; }
            set { _position = value;  }
        }

        public string PositionFull
        {
            get { return _positionFull; }
            set { _positionFull = value; }
        }

        public string Hometown
        {
            get { return _hometown; }
            set { _hometown = value; }
        }

        public string State
        {
            get { return _state; }
            set { _state = value; }
        }

        public string Headshot
        {
            get { return _headshot; }
            set { _headshot = value; }
        }

        public string Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public string Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        public string Class
        {
            get { return _class; }
            set { _class = value; }
        }

        public string TradeTidbit
        {
            get { return _tradeTidbit; }
            set { _tradeTidbit = value; }
        }

        public int KiperRank
        {
            get { return _kiperRank; }
            set { _kiperRank = value; }
        }

        public int McShayRank
        {
            get { return _mcshayRank; }
            set { _mcshayRank = value; }
        }

        public Team School
        {
            get { return _school; }
            set { _school = value; }
        }

        public Pick Pick
        {
            get { return _pick; }
            set { _pick = value; }
        }

        //public Team Team
        //{
        //    get { return _nflTeam; }
        //    set { _nflTeam = value; OnPropertyChanged("NFLTeam"); }
        //}

        public List<Tidbit> Tidbits
        {
            get { return _tidbits; }
            set { _tidbits = value; }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        #endregion
    }
}
