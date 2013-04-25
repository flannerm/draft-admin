using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Input;
using DraftAdmin.Commands;
using DraftAdmin.DataAccess;
using System.Net;

namespace DraftAdmin.ViewModels
{
    public class TeamsAndPlayersViewModel : ViewModelBase
    {
        #region Private Members

        scliveweb.Service _ws;

        DataTable _sports;
        DataTable _leagues;
        DataTable _teams;
        DataTable _players;

        string _selectedSport;
        string _selectedLeague;
        string _selectedTeam;
        
        string _selectedPlayerId;
        string _selectedPlayerFName;
        string _selectedPlayerLName;
        string _selectedPlayerPos;

        private DelegateCommand _addPlayerCommand;

        #endregion

        #region Properties

        public DataTable Sports
        {
            get { return _sports; }
            set { _sports = value; OnPropertyChanged("Sports"); }                
        }

        public DataTable Leagues
        {
            get { return _leagues; }
            set { _leagues = value; OnPropertyChanged("Leagues"); }
        }

        public DataTable Teams
        {
            get { return _teams; }
            set { _teams = value; OnPropertyChanged("Teams"); }
        }

        public DataTable Players
        {
            get { return _players; }
            set { _players = value; OnPropertyChanged("Players"); }
        }

        public string SelectedSport
        {
            get { return _selectedSport; }
            set 
            { 
                _selectedSport = value; 
                OnPropertyChanged("SelectedSport");

                loadLeagues();
            }
        }

        public string SelectedLeague
        {
            get { return _selectedLeague; }
            set 
            { 
                _selectedLeague = value; 
                OnPropertyChanged("SelectedLeague");

                loadTeams();
            }
        }

        public string SelectedTeam
        {
            get { return _selectedTeam; }
            set
            {
                _selectedTeam = value;
                OnPropertyChanged("SelectedTeam");

                loadPlayers();
            }
        }

        public string SelectedPlayerId
        {
            get { return _selectedPlayerId; }
            set 
            {
                if (value != _selectedPlayerId)
                {
                    _selectedPlayerId = value;                                
                    OnPropertyChanged("SelectedPlayerId");

                    if (_selectedPlayerId != null)
                    {
                        DataRow[] row = _players.Select("ID = " + _selectedPlayerId);

                        _selectedPlayerFName = row[0]["FIRST_NAME"].ToString();
                        _selectedPlayerLName = row[0]["LAST_NAME"].ToString();
                        _selectedPlayerPos = row[0]["PRIMARY_POSITION"].ToString();     
                    }   
                }     
            }
        }

        #endregion

        #region Constructor

        public TeamsAndPlayersViewModel(scliveweb.Service ws)
        {
            //_ws = new scliveweb.Service();

            _ws = ws;

            loadLeagues();
        }

        #endregion

        #region Private Methods

        private void loadLeagues()
        {
            DataSet ds = null;

            try
            {
                Object[] parms = { "GeneralSports" };
                ds = _ws.CallFunctionByName("GetNewsLeagues", parms);

                if (ds != null)
                {
                    Leagues = ds.Tables[0];
                }
                else
                {
                    Leagues = null;
                }
            }
            catch (WebException ex)
            { }
            finally
            { }
        }

        private void loadTeams()
        {
            DataSet ds = null;

            try
            {
                Object[] parms = { "GeneralSports", _selectedLeague };
                ds = _ws.CallFunctionByName("GetNewsTeamsbyLeague", parms);

                if (ds != null)
                {
                    Teams = ds.Tables[0];
                }
                else
                {
                    Teams = null;
                }
            }
            catch (WebException ex)
            { }
            finally
            { }
        }

        private void loadPlayers()
        {
            DataSet ds = null;

            try
            {
                Object[] parms = { "GeneralSports", Convert.ToInt32(_selectedTeam) };
                ds = _ws.CallFunctionByName("GetPlayersbyTeam", parms);

                if (ds != null)
                {
                    Players = ds.Tables[0];
                }
                else
                {
                    Players = null;
                }
            }
            catch (WebException ex)
            { }
            finally
            { }
        }

        private void addPlayer()
        {
            if (_selectedPlayerId != null)
            {
                DbConnection.AddPlayerToDraftPlayers(Convert.ToInt64(_selectedPlayerId), _selectedPlayerFName, _selectedPlayerLName, _selectedPlayerPos, Convert.ToInt32(_selectedTeam));
            }
        }

        #endregion

        #region Commands

        public ICommand AddPlayerCommand
        {
            get
            {
                if (_addPlayerCommand == null)
                {
                    _addPlayerCommand = new DelegateCommand(addPlayer);
                }
                return _addPlayerCommand;
            }
        }       

        #endregion
    }
}
