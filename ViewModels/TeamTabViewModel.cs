using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using DraftAdmin.Commands;
using DraftAdmin.DataAccess;
using System.Windows.Input;

namespace DraftAdmin.ViewModels
{
    public class TeamTabViewModel : ViewModelBase
    {
        #region Private Members

        private Team _selectedTeam;
        private Team _selectedTeamTemp;
        private TeamEditViewModel _selectedTeamEditVM;

        private bool _askSaveTeamOnDirty = false;

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
            Global.GlobalCollections.Instance.LoadTeams();
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
