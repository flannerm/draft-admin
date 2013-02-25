using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using DraftAdmin.Models;
using DraftAdmin.DataAccess;
using DraftAdmin.Commands;
using System.Windows.Input;

namespace DraftAdmin.ViewModels
{
    public class CategoryTabViewModel : ViewModelBase
    {
        #region Private Members
        
        private Category _selectedCategory;
        private Category _selectedCategoryTemp;
        private CategoryEditViewModel _selectedCategoryEditVM;

        private ObservableCollection<CategoryViewModelBase> _categoryVMs;

        private DelegateCommand _refreshCategoriesCommand;

        private bool _askSaveCategoryOnDirty = false;

        #endregion

        #region Public Members

        public DelegateCommand<object> SaveCategoryChanges { get; set; }
        public DelegateCommand<object> DiscardCategoryChanges { get; set; }

        #endregion

        #region Properties

        public ObservableCollection<CategoryViewModelBase> Categories
        {
            get { return _categoryVMs; }
            set { _categoryVMs = value; OnPropertyChanged("Categories"); }
        }

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (value != null && value != _selectedCategory && _selectedCategory != null && _selectedCategory.IsDirty == true)
                {
                    _selectedCategoryTemp = value;
                    PromptMessage = "Save changes to " + _selectedCategory.FullName + "?";
                    AskSaveCategoryOnDirty = true;
                }
                else
                {
                    selectCategory(value);
                }
            }
        }

        public bool AskSaveCategoryOnDirty
        {
            get { return _askSaveCategoryOnDirty; }
            set { _askSaveCategoryOnDirty = value; OnPropertyChanged("AskSaveCategoryOnDirty"); }
        }

        public CategoryEditViewModel SelectedCategoryEditVM
        {
            get { return _selectedCategoryEditVM; }
            set { _selectedCategoryEditVM = value; OnPropertyChanged("SelectedCategoryEditVM"); }
        }

        #endregion

        #region Private Methods

        private void loadCategories()
        {
            ObservableCollection<Category> categories = DbConnection.GetCategories(1);

            _categoryVMs = new ObservableCollection<CategoryViewModelBase>();

            foreach (Category category in categories)
            {
                CategoryViewModelBase categoryVM = new CategoryViewModelBase(category);

                _categoryVMs.Add(categoryVM);
            }

            OnPropertyChanged("Categories");
        }

        private void saveCategoryChangesAction(object parameter)
        {
            _selectedCategory.IsDirty = false;

            AskSaveCategoryOnDirty = false;

            saveCategory();
        }

        private void saveCategory()
        {
            if (DbConnection.SaveCategory(_selectedCategory) == true)
            {
                OnSetStatusBarMsg(_selectedCategory.FullName + " saved at " + DateTime.Now.ToLongTimeString(), "Green");
            }
            else
            {
                OnSetStatusBarMsg("Error saving " + _selectedCategory.FullName + ".", "Red");
            }
        }

        private void discardCategoryChangesAction(object parameter)
        {
            _selectedCategory.IsDirty = false;

            AskSaveCategoryOnDirty = false;

            _selectedCategory = DbConnection.GetCategory(_selectedCategory.ID);

            selectCategory(_selectedCategoryTemp);
        }

        private void selectCategory(Category category)
        {
            _selectedCategory = category;

            OnPropertyChanged("SelectedCatgory");

            if (_selectedCategory != null)
            {
                SelectedCategoryEditVM = new CategoryEditViewModel(_selectedCategory);
                SelectedCategoryEditVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(OnSetStatusBarMsg);
            }
        }

        #endregion

        #region Constructor

        public CategoryTabViewModel()
        {
            SaveCategoryChanges = new DelegateCommand<object>(saveCategoryChangesAction);
            DiscardCategoryChanges = new DelegateCommand<object>(discardCategoryChangesAction);
        }

        #endregion

        #region Commands

        public ICommand RefreshCategoriesCommand
        {
            get
            {
                if (_refreshCategoriesCommand == null)
                {
                    _refreshCategoriesCommand = new DelegateCommand(loadCategories);
                }
                return _refreshCategoriesCommand;
            }
        }

        #endregion

    }
}
