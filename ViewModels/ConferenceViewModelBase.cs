using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using System.Collections.ObjectModel;

namespace DraftAdmin.ViewModels
{
    public class ConferenceViewModelBase : ViewModelBase
    {
        #region Private Members

        protected Conference _conf;

        private ObservableCollection<TidbitViewModel> _tidbits;

        #endregion

        #region Properties

        public Conference Conference
        {
            get { return _conf; }
            set { _conf = value; OnPropertyChanged("Conference"); }
        }

        public Int32 ID
        {
            get { return _conf.ID; }
            set { _conf.ID = value; OnPropertyChanged("ID"); }
        }

        public string FullName
        {
            get { return _conf.Name; }
            set { _conf.Name = value; OnPropertyChanged("FullName"); IsDirty = true; }
        }

        public string Tricode
        {
            get { return _conf.Tricode; }
            set { _conf.Tricode = value; OnPropertyChanged("Tricode"); IsDirty = true; }
        }

        public Uri LogoTga
        {
            get { return _conf.LogoTga; }
            set { _conf.LogoTga = value; OnPropertyChanged("LogoTga"); }
        }

        public Uri LogoPng
        {
            get { return _conf.LogoPng; }
            set { _conf.LogoPng = value; OnPropertyChanged("LogoPng"); }
        }

        public ObservableCollection<TidbitViewModel> ConferenceTidbits
        {
            get { return _tidbits; }
            set { _tidbits = value; OnPropertyChanged("ConferenceTidbits"); }
        }

        public bool IsDirty
        {
            get { return _conf.IsDirty; }
            set { _conf.IsDirty = value; OnPropertyChanged("IsDirty"); }
        }

        public int NumOfRecruits
        {
            get { return _conf.NumOfRecruits; }
            set { _conf.NumOfRecruits = value; OnPropertyChanged("NumOfRecruits"); }
        }

        public List<string> Recruits
        {
            get { return _conf.Recruits; }
            set { _conf.Recruits = value; OnPropertyChanged("Recruits"); }
        }

        #endregion

        #region Constructor

        public ConferenceViewModelBase(Conference conference)
        {
            _conf = conference;

            loadTidbits();
        }

        #endregion

        #region Protected Methods

        protected void loadTidbits()
        {
            if (ConferenceTidbits == null)
            {
                ConferenceTidbits = new ObservableCollection<TidbitViewModel>();
            }

            ConferenceTidbits.Clear();

            foreach (Tidbit tidbit in _conf.Tidbits)
            {
                ConferenceTidbits.Add(new TidbitViewModel(tidbit));
            }
        }

        #endregion
    }
}
