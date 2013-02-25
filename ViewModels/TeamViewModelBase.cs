using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using System.Drawing;
using System.Configuration;
using System.IO;
using DraftAdmin.Utilities;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DraftAdmin.Commands;
using DraftAdmin.DataAccess;

namespace DraftAdmin.ViewModels
{
    public class TeamViewModelBase : ViewModelBase
    {

        #region Private Members

        protected Team _team;

        protected TidbitViewModel _selectedTidbit = null;

        protected ObservableCollection<TidbitViewModel> _tidbitVMs;

        private DelegateCommand _saveTeamCommand;
        private DelegateCommand _addTidbitCommand;
        private DelegateCommand _deleteTidbitCommand;

        #endregion

        #region Properties

        public Team Team
        {
            get { return _team; }
            set { _team = value; OnPropertyChanged("Team"); }
        }

        public Int32 ID
        {
            get { return _team.ID; }
            set { _team.ID = value; OnPropertyChanged("ID"); }
        }

        public int Rank
        {
            get { return _team.Rank; }
            set { _team.Rank = value; OnPropertyChanged("Rank"); IsDirty = true; }
        }

        public string DisplayRank
        {
            get
            {
                if (_team.Rank == 0)
                {
                    return "";
                }
                else
                {
                    return _team.Rank.ToString();
                }
            }
        }

        public string FullName
        {
            get { return _team.FullName; }
            set { _team.FullName = value; OnPropertyChanged("FullName"); IsDirty = true; }
        }

        public string Tricode
        {
            get { return _team.Tricode; }
            set { _team.Tricode = value; OnPropertyChanged("Tricode"); IsDirty = true; }
        }

        public string City
        {
            get { return _team.City; }
            set { _team.City = value; OnPropertyChanged("City"); IsDirty = true; }
        }

        public string Name
        {
            get { return _team.Name; }
            set { _team.Name = value; OnPropertyChanged("Name"); IsDirty = true; }
        }

        public string OverallRecord
        {
            get { return _team.OverallRecord; }
            set { _team.OverallRecord = value; OnPropertyChanged("OverallRecord"); IsDirty = true; }
        }

        public string ConferenceRecord
        {
            get { return _team.ConferenceRecord; }
            set { _team.ConferenceRecord = value; OnPropertyChanged("ConferenceRecord"); IsDirty = true; }
        }

        public int LotteryPctRank
        {
            get { return _team.LotteryPctRank; }
            set { _team.LotteryPctRank = value; OnPropertyChanged("LotteryPctRank"); IsDirty = true; }
        }

        public int LotteryOrder
        {
            get { return _team.LotteryOrder; }
            set { _team.LotteryOrder = value; OnPropertyChanged("LotteryOrder"); IsDirty = true; }
        }

        public Uri LogoTga
        {
            get { return _team.LogoTga; }
            set { _team.LogoTga = value; OnPropertyChanged("LogoTga"); }
        }

        public Uri LogoPng
        {
            get { return _team.LogoPng; }
            set { _team.LogoPng = value; OnPropertyChanged("LogoPng"); }
        }

        public Conference Conference
        {
            get { return _team.Conference; }
            set { _team.Conference = value; OnPropertyChanged("Conference"); }
        }

        public ObservableCollection<TidbitViewModel> TidbitVMs
        {
            get { return _tidbitVMs; }
            set { _tidbitVMs = value; OnPropertyChanged("TidbitVMs"); }
        }
             
        public TidbitViewModel SelectedTidbit
        {
            get { return _selectedTidbit; }
            set { _selectedTidbit = value; OnPropertyChanged("SelectedTidbit"); }
        }

        public bool IsDirty
        {
            get { return _team.IsDirty; }
            set { _team.IsDirty = value; OnPropertyChanged("IsDirty"); }
        }

        #endregion

        #region Constructor

        public TeamViewModelBase(Team team)
        {
            _team = team;
            
            loadTidbits();
        }

        #endregion

        #region Private Methods

        private void updateTidbits()
        {
            if (ConfigurationManager.AppSettings["TeamTidbitsDatabase"].ToString().ToUpper() == "MYSQL")
            {
                _team.Tidbits = DbConnection.GetTidbitsMySql(2, _team.ID);
            }
            else
            {
                _team.Tidbits = DbConnection.GetTidbitsSDR(2, _team.ID);
            }

            loadTidbits();
        }

        private void saveTeam()
        {
            if (DbConnection.SaveTeam(_team) == true)
            {
                OnSetStatusBarMsg(_team.FullName + " saved at " + DateTime.Now.ToLongTimeString(), "Green");
                Global.GlobalCollections.Instance.LoadTeams();
            }
            else
            {
                OnSetStatusBarMsg("Error saving " + _team.FullName + ".", "Red");
            }

            IsDirty = false;
        }

        private void addTidbit()
        {
            bool tidbitAdded = false;

            if (ConfigurationManager.AppSettings["TeamTidbitsDatabase"].ToString().ToUpper() == "MYSQL")
            {
                tidbitAdded = DbConnection.AddTidbitMySql(2, _team.ID);
            }
            else
            {
                tidbitAdded = DbConnection.AddTidbitSDR(2, _team.ID);
            }

            if (tidbitAdded)
            {
                OnSetStatusBarMsg(_team.FullName + " - tidbit added.", "Green");
                updateTidbits();
                loadTidbits();
            }

        }

        private void deleteTidbit()
        {
            bool tidbitDeleted = false;

            if (_selectedTidbit != null)
            {
                if (ConfigurationManager.AppSettings["TeamTidbitsDatabase"].ToString().ToUpper() == "MYSQL")
                {
                    tidbitDeleted = DbConnection.DeleteTidbitMySql(_selectedTidbit.ReferenceType, _team.ID, _selectedTidbit.TidbitOrder);
                }
                else
                {
                    tidbitDeleted = DbConnection.DeleteTidbitSDR(_selectedTidbit.ReferenceType, _team.ID, _selectedTidbit.TidbitOrder);
                }

                if (tidbitDeleted)
                {
                    OnSetStatusBarMsg(_team.FullName + " tidbits updated.", "Green");
                    updateTidbits();
                    loadTidbits();
                }
            }
        }

        #endregion

        #region Protected Methods

        protected void loadTidbits()
        {
            if (TidbitVMs == null)
            {
                TidbitVMs = new ObservableCollection<TidbitViewModel>();
            }
            else
            {
                TidbitVMs.Clear();
            }            
            
            if (_team.Tidbits != null)
            {
                foreach (Tidbit tidbit in _team.Tidbits)
                {
                    TidbitVMs.Add(new TidbitViewModel(tidbit));
                }
            }            
        }

        #endregion

        #region Commands

        public ICommand SaveTeamCommand
        {
            get
            {
                if (_saveTeamCommand == null)
                {
                    _saveTeamCommand = new DelegateCommand(saveTeam);
                }
                return _saveTeamCommand;
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

        #endregion

    }
}

