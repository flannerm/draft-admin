using System.ComponentModel;

namespace DraftAdmin.Models
{
    public abstract class ModelBase : INotifyPropertyChanged
    {

        private string _round;
        public string Round
        {
            get { return _round; }
            set { _round = value; OnPropertyChanged("Round"); }
        }

        #region INotifyPropertyChanges

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
