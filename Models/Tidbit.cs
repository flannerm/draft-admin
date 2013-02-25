using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DraftAdmin.Models
{
    public class Tidbit : ModelBase
    {

        #region Private Members

        //private int _tidbitId;
        private int _referenceType;
        private int _referenceId;
        private int _tidbitOrder;
        private string _tidbitText;
        private string _timecode;
        private bool _enabled;

        #endregion

        #region Properties

        //public int TidbitID
        //{
        //    get { return _tidbitId; }
        //    set { _tidbitId = value; OnPropertyChanged("TidbitID"); }
        //}

        public int ReferenceType
        {
            get { return _referenceType; }
            set { _referenceType = value; OnPropertyChanged("ReferenceType"); }
        }

        public Int32 ReferenceID
        {
            get { return _referenceId; }
            set { _referenceId = value; OnPropertyChanged("ReferenceID"); }
        }

        public int TidbitOrder
        {
            get { return _tidbitOrder; }
            set { _tidbitOrder = value; OnPropertyChanged("TidbitOrder"); }
        }

        public string TidbitText
        {
            get { return _tidbitText; }
            set { _tidbitText = value; OnPropertyChanged("TidbitText"); }
        }

        public string Timecode
        {
            get { return _timecode; }
            set { _timecode = value; OnPropertyChanged("Timecode"); }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; OnPropertyChanged("Enabled"); }
        }

        #endregion

    }
}
