using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Commands;
using DraftAdmin.Models;
using DraftAdmin.DataAccess;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace DraftAdmin.ViewModels
{
    public class TeamEditViewModel : TeamViewModelBase
    {             
        #region Constructor

        public TeamEditViewModel(Team team) : base(team)
        {

        }

        #endregion

    }
}
