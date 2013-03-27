using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Commands;
using System.Collections.ObjectModel;
using DraftAdmin.Models;
using DraftAdmin.DataAccess;
using System.Windows.Input;
using System.ComponentModel;

namespace DraftAdmin.ViewModels
{
    public class SchoolTabViewModel : ViewModelBase
    {
        #region Private Members

        private Team _selectedSchool;
        private Team _selectedSchoolTemp;
        private SchoolEditViewModel _selectedSchoolEditVM;

        private bool _askSaveSchoolOnDirty = false;

        private bool _refreshEnabled = true;

        private DelegateCommand _refreshSchoolsCommand;

        #endregion

        #region Public Members

        public DelegateCommand<object> SaveSchoolChanges { get; set; }
        public DelegateCommand<object> DiscardSchoolChanges { get; set; }

        #endregion

        #region Properties

        public Team SelectedSchool
        {
            get { return _selectedSchool; }
            set
            {
                if (value != null && value != _selectedSchool && _selectedSchool != null && _selectedSchool.IsDirty == true)
                {
                    _selectedSchoolTemp = value;
                    PromptMessage = "Save changes to " + _selectedSchool.FullName + "?";
                    AskSaveSchoolOnDirty = true;
                }
                else
                {
                    selectSchool(value);
                }
            }
        }

        public SchoolEditViewModel SelectedSchoolEditVM
        {
            get { return _selectedSchoolEditVM; }
            set { _selectedSchoolEditVM = value; OnPropertyChanged("SelectedSchoolEditVM"); }
        }

        public bool AskSaveSchoolOnDirty
        {
            get { return _askSaveSchoolOnDirty; }
            set { _askSaveSchoolOnDirty = value; OnPropertyChanged("AskSaveSchoolOnDirty"); }
        }

        public bool RefreshEnabled
        {
            get { return _refreshEnabled; }
            set { _refreshEnabled = value; OnPropertyChanged("RefreshEnabled"); }
        }

        #endregion

        #region Constructor

        public SchoolTabViewModel()
        {
            SaveSchoolChanges = new DelegateCommand<object>(saveSchoolChangesAction);
            DiscardSchoolChanges = new DelegateCommand<object>(discardSchoolChangesAction);
        }

        #endregion

        #region Private Methods

        private void selectSchool(Team school)
        {
            _selectedSchool = school;

            if (_selectedSchool != null)
            {
                SelectedSchoolEditVM = new SchoolEditViewModel(_selectedSchool);
                SelectedSchoolEditVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(OnSetStatusBarMsg);
            }
        }

        private void saveSchoolChangesAction(object parameter)
        {
            _selectedSchool.IsDirty = false;

            AskSaveSchoolOnDirty = false;

            saveSchool();
        }

        private void discardSchoolChangesAction(object parameter)
        {
            _selectedSchool.IsDirty = false;

            AskSaveSchoolOnDirty = false;

            _selectedSchool = DbConnection.GetTeam(_selectedSchool.ID);

            selectSchool(_selectedSchoolTemp);
        }

        private void saveSchool()
        {
            if (DbConnection.SaveTeam(_selectedSchool) == true)
            {
                OnSetStatusBarMsg(_selectedSchool.FullName + " saved at " + DateTime.Now.ToLongTimeString(), "Green");
                refreshSchools();
            }
            else
            {
                OnSetStatusBarMsg("Error saving " + _selectedSchool.FullName + ".", "Red");
            }
        }

        private void refreshSchools()
        {
            OnSetStatusBarMsg("Loading schools...", "#f88803");

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                RefreshEnabled = false;
                Global.GlobalCollections.Instance.LoadSchools(worker);
            };

            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                OnSetStatusBarMsg("Loading schools (" + args.ProgressPercentage.ToString() + "%)", "#f88803");
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                OnSetStatusBarMsg("Schools loaded at: " + DateTime.Now.ToString("h:mm:ss tt"), "Green");
                RefreshEnabled = true;
            };

            worker.RunWorkerAsync();
        }

        #endregion

        #region Commands

        public ICommand RefreshSchoolsCommand
        {
            get
            {
                if (_refreshSchoolsCommand == null)
                {
                    _refreshSchoolsCommand = new DelegateCommand(refreshSchools);
                }
                return _refreshSchoolsCommand;
            }
        }

        #endregion
    }
}
