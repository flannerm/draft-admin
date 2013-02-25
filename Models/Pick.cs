using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DraftAdmin.Models
{
    public class Pick : ModelBase
    {
        #region Private Members

        private int _pick;
        private int _round;
        private int _roundPick;
        private Team _team;

        #endregion

        #region Properties

        public int OverallPick
        {
            get { return _pick; }
            set { _pick = value; OnPropertyChanged("OverallPick"); }
        }

        public int Round
        {
            get { return _round; }
            set { _round = value; OnPropertyChanged("Round"); }
        }

        public int RoundPick
        {
            get { return _roundPick; }
            set { _roundPick = value; OnPropertyChanged("RoundPick"); }
        }

        public Team Team
        {
            get { return _team; }
            set { _team = value; OnPropertyChanged("Team"); }
        }

        #endregion

    }
}
