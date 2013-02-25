using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using System.Windows.Input;
using DraftAdmin.Commands;
using DraftAdmin.DataAccess;

namespace DraftAdmin.ViewModels
{
    public class SchoolEditViewModel : TeamViewModelBase
    {

        #region Constructor

        public SchoolEditViewModel(Team school) : base(school)
        {
            
        }

        #endregion

    }
}
