using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using DraftAdmin.Commands;
using DraftAdmin.DataAccess;
using System.Windows.Input;
using System.ComponentModel;

namespace DraftAdmin.ViewModels
{
    public class TeamTabViewModel : ViewModelBase
    {
        #region Private Members

        private Team _selectedTeam;
        private Team _selectedTeamTemp;
        private TeamEditViewModel _selectedTeamEditVM;

        private bool _askSaveTeamOnDirty = false;

        private bool _refreshEnabled = true;

        private DelegateCommand _refreshTeamsCommand;

        #endregion

        #region Public Members

        public DelegateCommand<object> SaveTeamChanges { get; set; }
        public DelegateCommand<object> DiscardTeamChanges { get; set; }

        #endregion

        #region Properties

        public Team SelectedTeam
        {
            get { return _selectedTeam; }
            set
            {
                if (value != null && value != _selectedTeam && _selectedTeam != null && _selectedTeam.IsDirty == true)
                {
                    _selectedTeamTemp = value;
                    PromptMessage = "Save changes to " + _selectedTeam.FullName + "?";
                    AskSaveTeamOnDirty = true;
                }
                else
                {
                    selectTeam(value);
                }
            }
        }

        public TeamEditViewModel SelectedTeamEditVM
        {
            get { return _selectedTeamEditVM; }
            set { _selectedTeamEditVM = value; OnPropertyChanged("SelectedTeamEditVM"); }
        }

        public bool AskSaveTeamOnDirty
        {
            get { return _askSaveTeamOnDirty; }
            set { _askSaveTeamOnDirty = value; OnPropertyChanged("AskSaveTeamOnDirty"); }
        }

        public bool RefreshEnabled
        {
            get { return _refreshEnabled; }
            set { _refreshEnabled = value; OnPropertyChanged("RefreshEnabled"); }
        }

        #endregion

        #region Constructor

        public TeamTabViewModel()
        {
            SaveTeamChanges = new DelegateCommand<object>(saveTeamChangesAction);
            DiscardTeamChanges = new DelegateCommand<object>(discardTeamChangesAction);
        }

        #endregion

        #region Private Methods

        private void selectTeam(Team team)
        {
            _selectedTeam = team;

            if (_selectedTeam != null)
            {
                SelectedTeamEditVM = new TeamEditViewModel(_selectedTeam);
                SelectedTeamEditVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(OnSetStatusBarMsg);
                SelectedTeamEditVM.SendCommandEvent += new SendCommandEventHandler(OnSendCommand);
            }
        }

        private void saveTeamChangesAction(object parameter)
        {
            _selectedTeam.IsDirty = false;

            AskSaveTeamOnDirty = false;

            saveTeam();
        }

        private void discardTeamChangesAction(object parameter)
        {
            _selectedTeam.IsDirty = false;

            AskSaveTeamOnDirty = false;

            _selectedTeam = DbConnection.GetTeam(_selectedTeam.ID);

            selectTeam(_selectedTeamTemp);
        }

        private void saveTeam()
        {
            if (DbConnection.SaveTeam(_selectedTeam) == true)
            {
                OnSetStatusBarMsg(_selectedTeam.FullName + " saved at " + DateTime.Now.ToLongTimeString(), "Green");
                refreshTeams();
            }
            else
            {
                OnSetStatusBarMsg("Error saving " + _selectedTeam.FullName + ".", "Red");
            }
        }

        private void refreshTeams()
        {
            OnSetStatusBarMsg("Loading teams...", "#f88803");

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                RefreshEnabled = false;
                Global.GlobalCollections.Instance.LoadTeams(worker);
            };

            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                OnSetStatusBarMsg("Loading teams (" + args.ProgressPercentage.ToString() + "%)", "#f88803");
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                OnSetStatusBarMsg("Teams loaded at: " + DateTime.Now.ToString("h:mm:ss tt"), "Green");
                RefreshEnabled = true;
            };

            worker.RunWorkerAsync(); 
        }

        #endregion

        #region Commands

        public ICommand RefreshTeamsCommand
        {
            get
            {
                if (_refreshTeamsCommand == null)
                {
                    _refreshTeamsCommand = new DelegateCommand(refreshTeams);
                }
                return _refreshTeamsCommand;
            }
        }

        #endregion

    }
}
