using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Commands;
using DraftAdmin.Models;
using DraftAdmin.DataAccess;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Configuration;
using DraftAdmin.PlayoutCommands;
using DraftAdmin.Output;

namespace DraftAdmin.ViewModels
{
    public class TeamEditViewModel : TeamViewModelBase
    {
        #region Private Members

        private Visibility _previewTidbitButtonVisibility = Visibility.Hidden;

        private DelegateCommand _previewTidbitCommand;

        #endregion

        #region Properties

        public Visibility PreviewTidbitButtonVisibility
        {
            get { return _previewTidbitButtonVisibility; }
            set { _previewTidbitButtonVisibility = value; OnPropertyChanged("PreviewTidbitButtonVisibility"); }
        }

        #endregion

        #region Constructor

        public TeamEditViewModel(Team team) : base(team)
        {
            if (ConfigurationManager.AppSettings["EnableTeamTidbitPreview"].ToString().ToUpper() == "TRUE")
            {
                PreviewTidbitButtonVisibility = Visibility.Visible;
            }
        }

        #endregion

        #region Private Methods

        private void previewTidbit()
        {
            if (_selectedTidbit != null)
            {
                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.CommandID = Guid.NewGuid().ToString();
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "TeamTidbitPreview"));

                XmlDataRow xmlRow = new XmlDataRow();

                xmlRow.Add("TIDBIT_1", _selectedTidbit.TidbitText);
                xmlRow.Add("TRICODE_1", _team.Tricode);
                xmlRow.Add("SWATCH_1", _team.SwatchTga.LocalPath);
                xmlRow.Add("TEAMLOGO_1", _team.LogoTgaNoKey.LocalPath);

                commandToSend.TemplateData = xmlRow.GetXMLString();

                OnSendCommand(commandToSend, null);
            }
        }

        #endregion
        
        #region Commands

        public ICommand PreviewTidbitCommand
        {
            get
            {
                if (_previewTidbitCommand == null)
                {
                    _previewTidbitCommand = new DelegateCommand(previewTidbit);
                }
                return _previewTidbitCommand;
            }
        }

        #endregion

    }
}
