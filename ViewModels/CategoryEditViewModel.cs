using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Commands;
using DraftAdmin.Models;
using DraftAdmin.DataAccess;
using System.Windows.Input;

namespace DraftAdmin.ViewModels
{
    public class CategoryEditViewModel : CategoryViewModelBase
    {
        #region Private Members

        private TidbitViewModel _selectedTidbit = null;
        private string _selectedTidbitText = "";

        private bool _askDeleteTidbit = false;
        private string _promptMessage = "";

        private DelegateCommand _saveCategoryCommand;
        private DelegateCommand _addTidbitCommand;
        private DelegateCommand _deleteTidbitCommand;

        public DelegateCommand<object> DeleteTidbit { get; set; }
        public DelegateCommand<object> CancelDeleteTidbit { get; set; }

        #endregion

        #region Properties

        public TidbitViewModel SelectedTidbit
        {
            get { return _selectedTidbit; }
            set 
            { 
                _selectedTidbit = value;

                if (_selectedTidbit != null)
                {
                    if (_selectedTidbit.TidbitText != null)
                    {
                        SelectedTidbitText = _selectedTidbit.TidbitText;
                    }
                }

                OnPropertyChanged("SelectedTidbit");             
            }
        }

        public string SelectedTidbitText
        {
            get { return _selectedTidbitText; }
            set { _selectedTidbitText = value; OnPropertyChanged("SelectedTidbitText"); }
        }

        public bool AskDeleteTidbit
        {
            get { return _askDeleteTidbit; }
            set { _askDeleteTidbit = value; OnPropertyChanged("AskDeleteTidbit"); }
        }

        public string PromptMessage
        {
            get { return _promptMessage; }
            set { _promptMessage = value; OnPropertyChanged("PromptMessage"); }
        }

        #endregion

        #region Constructor

        public CategoryEditViewModel(Category category) : base(category)
        {
            DeleteTidbit = new DelegateCommand<object>(deleteTidbitAction);
            CancelDeleteTidbit = new DelegateCommand<object>(cancelDeleteTidbitAction);
        }

        #endregion

        #region Private Methods

        private void updateTidbits()
        {
            _category.Tidbits = DbConnection.GetTidbitsMySql(3, _category.ID);
        }

        private void saveCategory()
        {
            if (DbConnection.SaveCategory(_category) == true)
            {
                OnSetStatusBarMsg(_category.FullName + " saved at " + DateTime.Now.ToLongTimeString(), "Green");
                Global.GlobalCollections.Instance.LoadCategories();
            }
            else
            {
                OnSetStatusBarMsg("Error saving " + _category.FullName + ".", "Red");
            }

            IsDirty = false;
        }

        private void addTidbit()
        {
            if (DbConnection.AddTidbitMySql(3, _category.ID) == true)
            {
                OnSetStatusBarMsg(_category.FullName + " - tidbit added.", "Green");
                updateTidbits();
                loadTidbits();
            }

        }

        private void deleteTidbit()
        {
            if (_selectedTidbit != null)
            {
                PromptMessage = "Delete " + _selectedTidbit.Timecode + " tidbit?";
                AskDeleteTidbit = true;
            }

            //if (_selectedTidbit != null)
            //{
            //    if (DbConnection.DeleteTidbit(_selectedTidbit.TidbitTypeID, _category.ID, _selectedTidbit.TidbitOrder) == true)
            //    {
            //        OnSetStatusBarMsg(_category.FullName + " tidbits updated.", "Green", false, false, false);
            //        updateTidbits();
            //        loadTidbits();
            //    }
            //}
        }

        private void deleteTidbitAction(object parameter)
        {
            AskDeleteTidbit = false;

            if (DbConnection.DeleteTidbitMySql(_selectedTidbit.ReferenceType, _category.ID, _selectedTidbit.TidbitOrder) == true)
            {
                OnSetStatusBarMsg(_category.FullName + " tidbits updated.", "Green");
                updateTidbits();
                loadTidbits();
            }
        }

        private void cancelDeleteTidbitAction(object parameter)
        {
            AskDeleteTidbit = false;
        }

        #endregion

        #region Commands

        public ICommand SaveCategoryCommand
        {
            get
            {
                if (_saveCategoryCommand == null)
                {
                    _saveCategoryCommand = new DelegateCommand(saveCategory);
                }
                return _saveCategoryCommand;
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
