using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using DraftAdmin.DataAccess;

namespace DraftAdmin.ViewModels
{
    public class TidbitViewModel : ViewModelBase
    {

        #region Private Members

        private Tidbit _tidbit;

        #endregion

        #region Properties

        //public int TidbitID
        //{
        //    get { return _tidbit.TidbitID; }
        //    set { _tidbit.TidbitID = value; OnPropertyChanged("TidbitID"); }
        //}

        public int ReferenceType
        {
            get { return _tidbit.ReferenceType; }
            set { _tidbit.ReferenceType = value; OnPropertyChanged("ReferenceType"); }
        }

        public Int32 ReferenceID
        {
            get { return _tidbit.ReferenceID; }
            set { _tidbit.ReferenceID = value; OnPropertyChanged("ReferenceID"); }
        }

        public int TidbitOrder
        {
            get { return _tidbit.TidbitOrder; }
            set 
            {
                //have to update the database here so the old order gets change.  if the db doesn't update here, we get left with an extraneous tidbit with the old order...
                if (_tidbit.TidbitOrder != value)
                {
                    if (DbConnection.UpdateTidbitSDR(_tidbit.ReferenceType, _tidbit.ReferenceID, _tidbit.TidbitOrder, _tidbit.TidbitText, _tidbit.Timecode, _tidbit.Enabled, value))
                    {
                        _tidbit.TidbitOrder = value;
                        OnPropertyChanged("TidbitOrder"); 
                    }
                }
                else
                {
                    _tidbit.TidbitOrder = value;
                    OnPropertyChanged("TidbitOrder"); 
                }                 
            }
        }

        public string TidbitText
        {
            get { return _tidbit.TidbitText; }
            set { _tidbit.TidbitText = value; OnPropertyChanged("TidbitText"); }
        }

        public string Timecode
        {
            get { return _tidbit.Timecode; }
            set { _tidbit.Timecode = value; OnPropertyChanged("Timecode"); }
        }

        public bool Enabled
        {
            get { return _tidbit.Enabled; }
            set { _tidbit.Enabled = value; OnPropertyChanged("Enabled"); }
        }

        #endregion

        #region Constructor

        public TidbitViewModel(Tidbit tidbit)
        {
            _tidbit = tidbit;
        }

        #endregion

    }
}
