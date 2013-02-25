using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Models;
using DraftAdmin.Commands;
using System.Windows.Input;
using DraftAdmin.DataAccess;
using DraftAdmin.PlayoutCommands;
using DraftAdmin.Output;


namespace DraftAdmin.ViewModels
{
    public class DraftOrderTabViewModel : ViewModelBase
    {

        #region Private Members

        private Pick _selectedPick;
        private Team _selectedTradePickTeam;

        private DelegateCommand _tradePickCommand;
        private DelegateCommand _hideTradeIntCommand;
        private DelegateCommand _refreshDraftOrderCommand;

        private bool _askTradePick = false;
        private bool _askAnimateTrade = false;

        private string _tradeString = "";

        #endregion

        #region Public Members

        public DelegateCommand<object> TradePick { get; set; }
        public DelegateCommand<object> CancelTradePick { get; set; }

        public DelegateCommand<object> AnimateTrade { get; set; }
        public DelegateCommand<object> CancelAnimateTrade { get; set; }

        #endregion

        #region Properties

        public Pick SelectedPick
        {
            get { return _selectedPick; }
            set { _selectedPick = value; OnPropertyChanged("SelectedPick"); }
        }

        public Team SelectedTradePickTeam
        {
            get { return _selectedTradePickTeam; }
            set { _selectedTradePickTeam = value; OnPropertyChanged("SelectedTradePickTeam"); }
        }

        public bool AskTradePick
        {
            get { return _askTradePick; }
            set { _askTradePick = value; OnPropertyChanged("AskTradePick"); }
        }

        public bool AskAnimateTrade
        {
            get { return _askAnimateTrade; }
            set { _askAnimateTrade = value; OnPropertyChanged("AskAnimateTrade"); }
        }

        #endregion

        #region Constructor

        public DraftOrderTabViewModel()
        {
            TradePick = new DelegateCommand<object>(tradePickAction);
            CancelTradePick = new DelegateCommand<object>(cancelTradePickAction);

            AnimateTrade = new DelegateCommand<object>(animateTradeAction);
            CancelAnimateTrade = new DelegateCommand<object>(cancelAnimateTradeAction);
        }

        #endregion

        #region Private Methods

        private void tradePick()
        {
            if (_selectedPick != null)
            {
                PromptMessage = "Trade #" + _selectedPick.OverallPick + " overall pick to " + _selectedTradePickTeam.Name + "?";
                AskTradePick = true;
            }
        }

        private void tradePickAction(object parameter)
        {
            AskTradePick = false;

            if (DbConnection.TradePick(_selectedPick, _selectedTradePickTeam))
            {
                if (Global.GlobalCollections.Instance.OnTheClock != null)
                {
                    int currentPick = Global.GlobalCollections.Instance.OnTheClock.OverallPick;

                    OnSetStatusBarMsg("#" + _selectedPick.OverallPick + " overall pick traded to " + _selectedTradePickTeam.Name, "Green");

                    int pick = _selectedPick.OverallPick;

                    _tradeString = "<font EventFranklinGothic-Demi>" + _selectedPick.Team.Name.ToUpper() + "<\\font> trade pick to <font EventFranklinGothic-Demi>" + _selectedTradePickTeam.Name.ToUpper() + "<\\font>";

                    Global.GlobalCollections.Instance.LoadDraftOrder();
                    Global.GlobalCollections.Instance.LoadOnTheClock();

                    if (pick == currentPick)
                    {
                        OnStopCycle();

                        PromptMessage = "Animate this trade?";
                        AskAnimateTrade = true;
                    }
                    else if (pick == currentPick + 1 || pick == currentPick + 2)
                    {
                        PlayerCommand commandToSend = new PlayerCommand();

                        commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                        commandToSend.Parameters = new List<CommandParameter>();
                        commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Next"));

                        XmlDataRow xmlRow = new XmlDataRow();

                        Pick nextPick1;
                        Pick nextPick2;

                        nextPick1 = (Pick)Global.GlobalCollections.Instance.DraftOrder.SingleOrDefault(p => p.OverallPick == currentPick + 1);
                        nextPick2 = (Pick)Global.GlobalCollections.Instance.DraftOrder.SingleOrDefault(p => p.OverallPick == currentPick + 2);

                        xmlRow.Add("ABBREV_4_1", nextPick1.Team.Tricode);
                        xmlRow.Add("ABBREV_4_2", nextPick2.Team.Tricode);
                        xmlRow.Add("CHANGE_TOTEM_FLAG", "0");

                        commandToSend.TemplateData = xmlRow.GetXMLString();

                        OnSendCommand(commandToSend, null);
                    }
                }
                else
                {
                    //just reload the draft order
                    Global.GlobalCollections.Instance.LoadDraftOrder();
                }
            }
        }

        private void cancelTradePickAction(object parameter)
        {
            AskTradePick = false;
        }

        private void animateTradeAction(object parameter)
        {
            OnStopCycle();

            AskAnimateTrade = false;

            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));
            commandToSend.Parameters.Add(new CommandParameter("QueueCommand", "true"));

            XmlDataRow xmlRow = new XmlDataRow();

            xmlRow.Add("CLOCK_OVERLAY", "TRADE");
            xmlRow.Add("LOGO_1", Global.GlobalCollections.Instance.OnTheClock.Team.LogoTgaNoKey.LocalPath);
            xmlRow.Add("SWATCH_1", Global.GlobalCollections.Instance.OnTheClock.Team.SwatchTga.LocalPath);
            xmlRow.Add("ABBREV_4_1", Global.GlobalCollections.Instance.OnTheClock.Team.Tricode);

            commandToSend.TemplateData = xmlRow.GetXMLString();

            OnSendCommand(commandToSend, null);

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "TradeInterruption"));

            xmlRow = new XmlDataRow();

            xmlRow.Add("TIDBIT_1", _tradeString);

            commandToSend.TemplateData = xmlRow.GetXMLString();

            OnSendCommand(commandToSend, null);

            _tradeString = "";
        }

        private void cancelAnimateTradeAction(object parameter)
        {
            AskAnimateTrade = false;

            _tradeString = "";

            //maybe just hot change the team on the clock here?

            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));

            XmlDataRow xmlRow = new XmlDataRow();

            xmlRow.Add("CLOCK_OVERLAY", "ON THE CLOCK");
            xmlRow.Add("LOGO_1", Global.GlobalCollections.Instance.OnTheClock.Team.LogoTgaNoKey.LocalPath);
            xmlRow.Add("SWATCH_1", Global.GlobalCollections.Instance.OnTheClock.Team.SwatchTga.LocalPath);
            xmlRow.Add("ABBREV_4_1", Global.GlobalCollections.Instance.OnTheClock.Team.Tricode);

            commandToSend.TemplateData = xmlRow.GetXMLString();

            //raise an event to the main...
            OnSendCommand(commandToSend, null);
        }

        private void hideTradeInterruption()
        {
            OnResetCycle();

            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));
            commandToSend.Parameters.Add(new CommandParameter("QueueCommand", "true"));

            XmlDataRow xmlRow = new XmlDataRow();

            xmlRow.Add("CLOCK_OVERLAY", "ON THE CLOCK");
            xmlRow.Add("LOGO_1", Global.GlobalCollections.Instance.OnTheClock.Team.LogoTgaNoKey.LocalPath);
            xmlRow.Add("SWATCH_1", Global.GlobalCollections.Instance.OnTheClock.Team.SwatchTga.LocalPath);
            xmlRow.Add("ABBREV_4_1", Global.GlobalCollections.Instance.OnTheClock.Team.Tricode);

            commandToSend.TemplateData = xmlRow.GetXMLString();

            OnSendCommand(commandToSend, null);
            
            commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "HidePage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "TradeInterruption"));

            OnSendCommand(commandToSend, null);
        }

        private void refreshDraftOrder()
        {
            Global.GlobalCollections.Instance.LoadDraftOrder();
        }

        #endregion

        #region Commands

        public ICommand TradePickCommand
        {
            get
            {
                if (_tradePickCommand == null)
                {
                    _tradePickCommand = new DelegateCommand(tradePick);
                }
                return _tradePickCommand;
            }
        }

        public ICommand HideTradeInterruptionCommand
        {
            get
            {
                if (_hideTradeIntCommand == null)
                {
                    _hideTradeIntCommand = new DelegateCommand(hideTradeInterruption);
                }
                return _hideTradeIntCommand;
            }
        }

        public ICommand RefreshDraftOrderCommand
        {
            get
            {
                if (_refreshDraftOrderCommand == null)
                {
                    _refreshDraftOrderCommand = new DelegateCommand(refreshDraftOrder);
                }
                return _refreshDraftOrderCommand;
            }
        }

        

        #endregion

    }
}
