using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFLDraft.Models;
using System.Drawing;
using System.Configuration;
using System.IO;
using NFLDraft.Utilities;
using System.Windows.Media.Imaging;

namespace NFLDraft.ViewModels
{
    public class SchoolListViewModel : SchoolViewModelBase
    {

        #region Constructor

        public SchoolListViewModel(Team school) : base(school)
        {
            _school = school;

            
        }        

        #endregion

    }
}
