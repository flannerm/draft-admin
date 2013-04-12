using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DraftAdmin.Models;

namespace DraftAdmin.ViewModels
{
    public class TweetViewModel : ViewModelBase
    {
        #region Private Members

        private Tweet _tweet;

        #endregion

        #region Properties

        public Tweet Tweet
        {
            get { return _tweet; }
            set { _tweet = value; OnPropertyChanged("Tweet"); }
        }

        #endregion

        #region Constructor

        public TweetViewModel(Tweet tweet)
        {
            _tweet = tweet;
        }

        #endregion

    }
}
