using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using System.Collections.ObjectModel;

namespace DraftAdmin.ViewModels
{
    public class CategoryViewModelBase : ViewModelBase
    {

        #region Private Members

        protected Category _category;

        private ObservableCollection<TidbitViewModel> _tidbits;

        #endregion

        #region Properties

        public Category Category
        {
            get { return _category; }
            set { _category = value; OnPropertyChanged("Category"); }
        }

        public Int32 ID
        {
            get { return _category.ID; }
            set { _category.ID = value; OnPropertyChanged("ID"); }
        }
        
        public string FullName
        {
            get { return _category.FullName; }
            set { _category.FullName = value; OnPropertyChanged("FullName"); }
        }

        public string Tricode
        {
            get { return _category.Tricode; }
            set { _category.Tricode = value; OnPropertyChanged("Tricode"); }
        }

        public Uri LogoTga
        {
            get { return _category.LogoTga; }
            set { _category.LogoTga = value; OnPropertyChanged("LogoTga"); }
        }

        public Uri LogoPng
        {
            get { return _category.LogoPng; }
            set { _category.LogoPng = value; OnPropertyChanged("LogoPng"); }
        }

        public ObservableCollection<TidbitViewModel> CategoryTidbits
        {
            get { return _tidbits; }
            set { _tidbits = value; OnPropertyChanged("CategoryTidbits"); }
        }

        public bool IsDirty
        {
            get { return _category.IsDirty; }
            set { _category.IsDirty = value; OnPropertyChanged("IsDirty"); }
        }

        #endregion

        #region Constructor

        public CategoryViewModelBase(Category category)
        {
            _category = category;

            loadTidbits();
        }

        #endregion

        #region Protected Methods

        protected void loadTidbits()
        {
            if (CategoryTidbits == null)
            {
                CategoryTidbits = new ObservableCollection<TidbitViewModel>();
            }

            CategoryTidbits.Clear();

            foreach (Tidbit tidbit in _category.Tidbits)
            {
                CategoryTidbits.Add(new TidbitViewModel(tidbit));
            }
        }

        #endregion

    }
}
