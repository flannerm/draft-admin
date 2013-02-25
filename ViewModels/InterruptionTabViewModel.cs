using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using DraftAdmin.PlayoutCommands;
using DraftAdmin.DataAccess;
using System.Windows.Input;
using DraftAdmin.Commands;
using DraftAdmin.Global;
using DraftAdmin.Models;


namespace DraftAdmin.ViewModels
{
    public class InterruptionTabViewModel : ViewModelBase
    {
        #region Private Members

        private ObservableCollection<CategoryViewModelBase> _interruptionVMs;

        private Category _selectedInterruption;
        private Category _selectedInterruptionTemp;
        private InterruptionEditViewModel _selectedInterruptionEditVM;

        private DelegateCommand _refreshInterruptionsCommand;

        private bool _askSaveInterruptionOnDirty = false;

        #endregion

        #region Public Members

        public delegate void ShowInterruptionEventHandler(PlayerCommand commandToSend, Playlist playlist = null);

        public delegate void StopCycleEventHandler();

        #endregion

        #region Properties

        public ObservableCollection<CategoryViewModelBase> Interruptions
        {
            get { return _interruptionVMs; }
            set { _interruptionVMs = value; OnPropertyChanged("Interruptions"); }
        }

        public Category SelectedInterruption
        {
            get { return _selectedInterruption; }
            set
            {
                if (value != null && value != _selectedInterruption && _selectedInterruption != null && _selectedInterruption.IsDirty == true)
                {
                    _selectedInterruptionTemp = value;
                    PromptMessage = "Save changes to " + _selectedInterruption.FullName + "?";
                    AskSaveInterruptionOnDirty = true;
                }
                else
                {
                    selectInterruption(value);
                }
            }
        }

        public InterruptionEditViewModel SelectedInterruptionEditVM
        {
            get { return _selectedInterruptionEditVM; }
            set
            {
                _selectedInterruptionEditVM = value;

                //_selectedInterruptionEditVM.OnShowInterruption += new InterruptionEditViewModel.ShowInterruptionEventHandler(showInterruption);

                OnPropertyChanged("SelectedInterruptionEditVM");
            }
        }

        public bool AskSaveInterruptionOnDirty
        {
            get { return _askSaveInterruptionOnDirty; }
            set { _askSaveInterruptionOnDirty = value; OnPropertyChanged("AskSaveInterruptionOnDirty"); }
        }

        #endregion

        #region Events

        public event ShowInterruptionEventHandler OnShowInterruption;

        public event StopCycleEventHandler OnStopCycle;

        #endregion

        #region Constructor



        #endregion

        #region Private Methods

        private void loadInterruptions()
        {
            GlobalCollections.Instance.LoadInterruptions();
        }

        private void selectInterruption(Category interruption)
        {
            _selectedInterruption = interruption;
            
            OnPropertyChanged("SelectedInterruption");

            if (_selectedInterruption != null)
            {
                SelectedInterruptionEditVM = new InterruptionEditViewModel(_selectedInterruption);
                SelectedInterruptionEditVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(OnSetStatusBarMsg);
                SelectedInterruptionEditVM.SendCommandEvent += new InterruptionEditViewModel.SendCommandEventHandler(OnShowInterruption);
                SelectedInterruptionEditVM.OnStopCycle += new InterruptionEditViewModel.StopCycleEventHandler(OnStopCycle);
            }
        }

        //private void showInterruption(PlayerCommand commandToSend)
        //{
        //    //L3TimerRunning = false;
        //    //sendCommandToPlayout(commandToSend);
        //}

        private void saveInterruption()
        {
            if (DbConnection.SaveCategory(_selectedInterruption) == true)
            {
                OnSetStatusBarMsg(_selectedInterruption.FullName + " saved at " + DateTime.Now.ToLongTimeString(), "Green");
            }
            else
            {
                OnSetStatusBarMsg("Error saving " + _selectedInterruption.FullName + ".", "Red");
            }
        }

        private void discardCategoryChangesAction(object parameter)
        {
            _selectedInterruption.IsDirty = false;

            AskSaveInterruptionOnDirty = false;

            _selectedInterruption = DbConnection.GetCategory(_selectedInterruption.ID);

            selectInterruption(_selectedInterruptionTemp);
        }

        #endregion

        #region Commands

        public ICommand RefreshInterruptionsCommand
        {
            get
            {
                if (_refreshInterruptionsCommand == null)
                {
                    _refreshInterruptionsCommand = new DelegateCommand(loadInterruptions);
                }
                return _refreshInterruptionsCommand;
            }
        }

        #endregion

    }
}
