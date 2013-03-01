using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using System.Windows.Input;
using DraftAdmin.Commands;
using DraftAdmin.PlayoutCommands;
using DraftAdmin.Output;
using System.Configuration;
using System.IO;

namespace DraftAdmin.ViewModels
{
    public class InterruptionEditViewModel : CategoryEditViewModel
    {

        #region Private Members

        private DelegateCommand _showInterruptionCommand;
        private DelegateCommand _refreshInterruptionTextFileTextCommand;

        public delegate void StopCycleEventHandler();
        
        public event StopCycleEventHandler StopCycleEvent;

        private string _textToAir;

        private string _interruptionTextFileText;

        #endregion

        #region Public Members

        #endregion

        #region Events

        public event StopCycleEventHandler OnStopCycle;

        #endregion
        
        #region Properties

        public string TextToAir
        {
            get { return _textToAir; }
            set { _textToAir = value; OnPropertyChanged("TextToAir"); }
        }

        public bool PlayoutEnabled
        {
            get
            {
                if (ConfigurationManager.AppSettings["ConnectToPlayout"].ToString().ToUpper() == "TRUE")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string InterruptionTextFileText
        {
            get { return _interruptionTextFileText; }
            set { _interruptionTextFileText = value; OnPropertyChanged("InterruptionTextFileText"); }
        }

        #endregion

        #region Constructor

        public InterruptionEditViewModel(Category category) : base(category)
        {
            
        }

        #endregion

        #region Private Methods

        private void showInterruption()
        {
            if (this.SelectedTidbitText != null && this.SelectedTidbitText != "")
            {
                if (OnStopCycle != null) OnStopCycle();

                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.CommandID = Guid.NewGuid().ToString();
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", ConfigurationManager.AppSettings["InterruptionTemplate"].ToString()));

                XmlDataRow xmlRow = new XmlDataRow();

                xmlRow.Add("CHIP_1", this.Category.LogoTga.LocalPath);
                xmlRow.Add("TIDBIT_1", this.SelectedTidbitText);
                xmlRow.Add("SWATCH_1", this.Category.SwatchFile.LocalPath);

                commandToSend.TemplateData = xmlRow.GetXMLString();

                OnSendCommand(commandToSend, null);
            }
        }

        private void refreshInterruptionTextFileText()
        {
            //InterruptionTextFileText = "";

            //if (File.Exists(ConfigurationManager.AppSettings["InterruptionTextFile"].ToString())) 
            //{
            //    using (StreamReader sr = File.OpenText(ConfigurationManager.AppSettings["InterruptionTextFile"].ToString()))
            //    {
            //        String input;
            //        while ((input = sr.ReadLine()) != null) 
            //        {
            //            InterruptionTextFileText += input;
            //        }
                 
            //    }
            //}
        }

        #endregion

        #region Commands

        public ICommand ShowInterruptionCommand
        {
            get
            {
                if (_showInterruptionCommand == null)
                {
                    _showInterruptionCommand = new DelegateCommand(showInterruption);
                }
                return _showInterruptionCommand;
            }
        }

        public ICommand RefreshInterruptionTextFileTextCommand
        {
            get
            {
                if (_refreshInterruptionTextFileTextCommand == null)
                {
                    _refreshInterruptionTextFileTextCommand = new DelegateCommand(refreshInterruptionTextFileText);
                }
                return _refreshInterruptionTextFileTextCommand;
            }
        }

        #endregion

        
    }
}
