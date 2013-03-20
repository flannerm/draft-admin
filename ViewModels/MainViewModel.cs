using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Input;
using System.Configuration;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Data;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;
using System.Threading;

using DraftAdmin.Global;
using DraftAdmin.Output;
using DraftAdmin.Sockets;
using DraftAdmin.PlayoutCommands;
using DraftAdmin.Commands;
using DraftAdmin.DataAccess;
using DraftAdmin.Models;
using System.Drawing;
using DraftAdmin.Utilities;
using System.Windows.Media.Imaging;

namespace DraftAdmin.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Members

        private PlayerTabViewModel _playerTabVM;
        private SchoolTabViewModel _schoolTabVM;
        private TeamTabViewModel _teamTabVM;
        private CategoryTabViewModel _categoryTabVM;
        private InterruptionTabViewModel _interruptionTabVM;
        private DraftOrderTabViewModel _draftOrderTabVM;
        private PlaylistTabViewModel _playlistTabVM;
        private CurrentSelectionTabViewModel _currentSelectionTabVM;
        private TeamsAndPlayersViewModel _teamsAndPlayersVM;

        private string _playoutMessageText;
        private string _playoutMessageColor;

        private DispatcherTimer _refreshPollTimer;

        private bool _playlistTimerRunning = false;

        private bool _refreshPoll = true;

        private DispatcherTimer _reconnectPlayoutTimer;
        private DispatcherTimer _checkPlayoutConnectionTimer;

        private bool _playoutConnectionAlive = false;
        private bool _playoutInitialized = false;
                        
        private DelegateCommand _importPlayersCommand;
        private DelegateCommand _importTeamsCommand;
        private DelegateCommand _importSchoolsCommand;
        private DelegateCommand _resetCycleCommand;
        private DelegateCommand _initializePlayoutCommand;
        private DelegateCommand _connectToPlayoutCommand;
        private DelegateCommand _togglePlaylistTimerCommand;
        private DelegateCommand _deleteLastPickCommand;
        private DelegateCommand _deleteAllPicksCommand;
        private DelegateCommand _refreshOverlaysCommand;
        private DelegateCommand _refreshPollChipsCommand;
        private DelegateCommand _refreshRightLogosCommand;

        private DelegateCommand _showPollQuestionCommand;
        private DelegateCommand _showPollResultsCommand;

        private DelegateCommand _getSchoolsFromSDRCommand;
        private DelegateCommand _getTeamsFromSDRCommand;

        private DelegateCommand _showClockOverlayCommand;
        private DelegateCommand _showClockCommand;

        private DelegateCommand _showRightLogoCommand;

        private DelegateCommand _nextOnTheClockCommand;

        public DelegateCommand<object> ImportPlayers { get; set; }
        public DelegateCommand<object> CancelImportPlayers { get; set; }

        public DelegateCommand<object> ImportTeams { get; set; }
        public DelegateCommand<object> CancelImportTeams { get; set; }

        public DelegateCommand<object> ImportSchools { get; set; }
        public DelegateCommand<object> CancelImportSchools { get; set; }

        public DelegateCommand<object> ResetCycle { get; set; }
        public DelegateCommand<object> CancelResetCycle { get; set; }

        public DelegateCommand<object> InitializePlayout { get; set; }
        public DelegateCommand<object> CancelInitializePlayout { get; set; }
        
        public DelegateCommand<object> GetSchoolsFromSDR { get; set; }
        public DelegateCommand<object> CancelGetSchoolsFromSDR { get; set; }

        public DelegateCommand<object> GetTeamsFromSDR { get; set; }
        public DelegateCommand<object> CancelGetTeamsFromSDR { get; set; }

        public DelegateCommand<object> DeleteLastPick { get; set; }
        public DelegateCommand<object> CancelDeleteLastPick { get; set; }

        public DelegateCommand<object> DeleteAllPicks { get; set; }
        public DelegateCommand<object> CancelDeleteAllPicks { get; set; }

        private bool _askImportPlayers = false;
        private bool _askImportTeams = false;
        private bool _askImportSchools = false;
        private bool _askResetCycle = false;
        private bool _askInitializePlayout = false;        
        private bool _askGetSchoolsFromSDR = false;
        private bool _askGetTeamsFromSDR = false;
        private bool _askDeleteLastPick = false;
        private bool _askDeleteAllPicks = false;

        private string _clock;
        private int _clockSeconds;

        private LogoChip _selectedClockOverlay;
        private LogoChip _selectedPollChip;
        private LogoChip _selectedRightLogo;

        private ObservableCollection<LogoChip> _clockOverlays;
        private ObservableCollection<LogoChip> _pollChips;
        private ObservableCollection<LogoChip> _rightLogos;

        private Talker _compTalker;
        private Talker _clockTalker;

        private delegate void setPlayoutFeedbackDelegate(string feedback);

        private string _lastCommandID;

        private List<string> _pollTextLines;
        private string _pollText;
        private System.Windows.Controls.ComboBoxItem _pollAnswers;

        private bool _clockRedUnderMin = true;

        private bool _useCountdownClock;
        private string _countdownTarget = ConfigurationManager.AppSettings["DraftStartDateTime"].ToString();
        private string _countdownClock;
        private DispatcherTimer _countdownTimer;

        private string _currentPlaylist;

        private int _prevClockSeconds = 999999;

        private string _pollChip;

        private bool _takeClock = true;
        private bool _pickIsIn = false;

        private Dictionary<string, int> _playlistCommands;

        private string _playoutFeedack;
        private string _playoutOutgoingCommand;

        #endregion

        #region Properties

        public PlayerTabViewModel PlayerTabVM
        {
            get { return _playerTabVM; }
            set { _playerTabVM = value; OnPropertyChanged("PlayerTabVM"); }
        }

        public SchoolTabViewModel SchoolTabVM
        {
            get { return _schoolTabVM; }
            set { _schoolTabVM = value; OnPropertyChanged("SchoolTabVM"); }
        }

        public TeamTabViewModel TeamTabVM
        {
            get { return _teamTabVM; }
            set { _teamTabVM = value; OnPropertyChanged("TeamTabVM"); }
        }

        public CategoryTabViewModel CategoryTabVM
        {
            get { return _categoryTabVM; }
            set { _categoryTabVM = value; OnPropertyChanged("CategoryTabVM"); }
        }

        public InterruptionTabViewModel InterruptionTabVM
        {
            get { return _interruptionTabVM; }
            set { _interruptionTabVM = value; OnPropertyChanged("InterruptionTabVM"); }
        }

        public DraftOrderTabViewModel DraftOrderTabVM
        {
            get { return _draftOrderTabVM; }
            set { _draftOrderTabVM = value; OnPropertyChanged("DraftOrderTabVM"); }
        }

        public PlaylistTabViewModel PlaylistTabVM
        {
            get { return _playlistTabVM; }
            set { _playlistTabVM = value; OnPropertyChanged("PlaylistTabVM"); }
        }

        public CurrentSelectionTabViewModel CurrentSelectionTabVM
        {
            get { return _currentSelectionTabVM; }
            set { _currentSelectionTabVM = value; OnPropertyChanged("CurrentSelectionTabVM"); }
        }

        public TeamsAndPlayersViewModel TeamsAndPlayersVM
        {
            get { return _teamsAndPlayersVM; }
            set { _teamsAndPlayersVM = value; OnPropertyChanged("TeamsAndPlayersVM"); }
        }

        public string PlayoutMessageText
        {
            get { return _playoutMessageText; }
            set { _playoutMessageText = value; OnPropertyChanged("PlayoutMessageText"); }
        }

        public string PlayoutMessageColor
        {
            get { return _playoutMessageColor; }
            set { _playoutMessageColor = value; OnPropertyChanged("PlayoutMessageColor"); }
        }

        public bool AskImportPlayers
        {
            get { return _askImportPlayers; }
            set { _askImportPlayers = value; OnPropertyChanged("AskImportPlayers"); }
        }

        public bool AskImportTeams
        {
            get { return _askImportTeams; }
            set { _askImportTeams = value; OnPropertyChanged("AskImportTeams"); }
        }

        public bool AskImportSchools
        {
            get { return _askImportSchools; }
            set { _askImportSchools = value; OnPropertyChanged("AskImportSchools"); }
        }

        public bool AskResetCycle
        {
            get { return _askResetCycle; }
            set { _askResetCycle = value; OnPropertyChanged("AskResetCycle"); }
        }

        public bool AskInitializePlayout
        {
            get { return _askInitializePlayout; }
            set { _askInitializePlayout = value; OnPropertyChanged("AskInitializePlayout"); }
        }

        public bool AskGetSchoolsFromSDR
        {
            get { return _askGetSchoolsFromSDR; }
            set { _askGetSchoolsFromSDR = value; OnPropertyChanged("AskGetSchoolsFromSDR"); }
        }

        public bool AskGetTeamsFromSDR
        {
            get { return _askGetTeamsFromSDR; }
            set { _askGetTeamsFromSDR = value; OnPropertyChanged("AskGetTeamsFromSDR"); }
        }

        public bool AskDeleteLastPick
        {
            get { return _askDeleteLastPick; }
            set { _askDeleteLastPick = value; OnPropertyChanged("AskDeleteLastPick"); }
        }

        public bool AskDeleteAllPicks
        {
            get { return _askDeleteAllPicks; }
            set { _askDeleteAllPicks = value; OnPropertyChanged("AskDeleteAllPicks"); }
        }

        public bool PlaylistTimerRunning
        {
            get { return _playlistTimerRunning; }
            set 
            {
                _playlistTimerRunning = value;

                if (_playlistTimerRunning)
                {
                    _lastCommandID = null;

                    foreach (Playlist playlist in PlaylistTabVM.LoadedPlaylists)
                    {                        
                        playlist.Timer.Interval = 100;
                        playlist.TimerRunning = true;

                        playlist.Timer.Start();
                    }
                }
                else
                {
                    _playlistCommands.Clear();

                    if (PlaylistTabVM.LoadedPlaylists != null)
                    {
                        foreach (Playlist playlist in PlaylistTabVM.LoadedPlaylists)
                        {
                            playlist.TimerRunning = false;
                            playlist.Timer.Stop();
                        }
                    }
                }

                OnPropertyChanged("PlaylistTimerRunning");
            }
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

        public string Clock
        {
            get { return _clock; }
            set 
            { 
                _clock = value; 
                OnPropertyChanged("Clock");
                
                if (_selectedClockOverlay != null)
                {
                    if (_selectedClockOverlay.FileName.ToUpper() != "<NONE>")
                    {
                        _clock = "";
                    }
                }

                updateClock();
            }
        }
        
        public int ClockSeconds
        {
            get { return _clockSeconds; }
            set { _clockSeconds = value; OnPropertyChanged("ClockSeconds"); }
        }

        public bool ClockRedUnderMin
        {
            get { return _clockRedUnderMin; }
            set { _clockRedUnderMin = value; OnPropertyChanged("ClockRedUnderMin"); }
        }

        public LogoChip SelectedClockOverlay
        {
            get { return _selectedClockOverlay; }
            set { _selectedClockOverlay = value; OnPropertyChanged("SelectedClockOverlay"); }
        }

        public ObservableCollection<LogoChip> ClockOverlays
        {
            get { return _clockOverlays; }
            set { _clockOverlays = value; OnPropertyChanged("ClockOverlays"); }
        }

        public LogoChip SelectedRightLogo
        {
            get { return _selectedRightLogo; }
            set { _selectedRightLogo = value; OnPropertyChanged("SelectedRightLogo"); }
        }

        public ObservableCollection<LogoChip> RightLogos
        {
            get { return _rightLogos; }
            set { _rightLogos = value; OnPropertyChanged("RightLogos"); }
        }

        public ObservableCollection<LogoChip> PollChips
        {
            get { return _pollChips; }
            set { _pollChips = value; OnPropertyChanged("PollChips"); }
        }

        public LogoChip SelectedPollChip
        {
            get { return _selectedPollChip; }
            set { _selectedPollChip = value; OnPropertyChanged("SelectedPollChip"); }
        }

        public string PollText
        {
            get { return _pollText; }
            set { _pollText = value; OnPropertyChanged("PollText"); }
        }

        public System.Windows.Controls.ComboBoxItem PollAnswers
        {
            get { return _pollAnswers; }
            set 
            { 
                _pollAnswers = value;
                DbConnection.UpdateNumberOfPollAnswers(Convert.ToInt16(_pollAnswers.Content));
            }
        }

        public bool UseCountdownClock
        {
            get { return _useCountdownClock; }
            set 
            { 
                _useCountdownClock = value; 
                OnPropertyChanged("UseCountdownClock");

                if (_useCountdownClock)
                {
                    if (_countdownTimer == null)
                    {
                        _countdownTimer = new DispatcherTimer();
                        _countdownTimer.Tick += new EventHandler(countdownTimerElapsed);
                        _countdownTimer.Interval = new TimeSpan(0, 0, 1);
                    }

                    _countdownTimer.Start();
                }
                else
                {
                    if (_countdownTimer != null)
                    {
                        _countdownTimer.Stop();
                    }                    
                }

                
            }
        }

        private void countdownTimerElapsed(object sender, EventArgs e)
        {
            if (_selectedClockOverlay != null)
            {
                if (_selectedClockOverlay.FileName.IndexOf("countdown") > -1)
                {
                    getCountdownClock(_countdownTarget);
                    updateCountdownClock();
                }
                else
                {
                    _countdownClock = "";
                    updateCountdownClock();
                }
            }
            else
            {
                _countdownClock = "";
                updateCountdownClock();
            }
        }
        
        public string CountdownTarget
        {
            get { return _countdownTarget; }
            set { _countdownTarget = value; OnPropertyChanged("CountdownTarget"); }
        }

        public bool RefreshPoll
        {
            get { return _refreshPoll; }
            set { _refreshPoll = value; OnPropertyChanged("RefreshPoll"); }
        }

        public string PollChip
        {
            get { return _pollChip; }
            set { _pollChip = value; OnPropertyChanged("PollChip"); }
        }

        public bool TakeClock
        {
            get { return _takeClock; }
            set { _takeClock = value; OnPropertyChanged("TakeClock"); }
        }

        public string PlayoutFeedback
        {
            get { return _playoutFeedack; }
            set { _playoutFeedack = value; OnPropertyChanged("PlayoutFeedback"); }
        }

        public string PlayoutOutgoingCommand
        {
            get { return _playoutOutgoingCommand; }
            set { _playoutOutgoingCommand = value; OnPropertyChanged("PlayoutOutgoingCommand"); }
        }
              
        #endregion

        #region Constructor

        public MainViewModel()
        {
            DbConnection.WebService = new scliveweb.Service();
            DbConnection.WebService.Url = ConfigurationManager.AppSettings["WebServiceUrl"].ToString();

            if (ConfigurationManager.AppSettings["WebServiceUrl"].ToString() == "http://misdevtest1/dataserver/service.asmx")
            {
                //WebRequest.DefaultWebProxy = new System.Net.WebProxy("http://misdevtest1:80");
            }
            else
            {
                //WebRequest.DefaultWebProxy = new System.Net.WebProxy("http://scliveweb:80");
            }   

            DbConnection.SetStatusBarMsg += new DbConnection.SetStatusBarMsgEventHandler(setStatusBarMsg);
            DbConnection.SendCommandNoTransitionsEvent += new DbConnection.SendCommandNoTransitionsEventHandler(sendCommandNoTransitions);

            ImportPlayers = new DelegateCommand<object>(importPlayersAction);
            CancelImportPlayers = new DelegateCommand<object>(cancelImportPlayersAction);

            ImportTeams = new DelegateCommand<object>(importTeamsAction);
            CancelImportTeams = new DelegateCommand<object>(cancelImportTeamsAction);

            ResetCycle = new DelegateCommand<object>(resetCycleAction);
            CancelResetCycle = new DelegateCommand<object>(cancelResetCycleAction);

            InitializePlayout = new DelegateCommand<object>(initializePlayoutAction);
            CancelInitializePlayout = new DelegateCommand<object>(cancelInitializePlayoutAction);
            
            GetSchoolsFromSDR = new DelegateCommand<object>(getSchoolsFromSDRAction);
            CancelGetSchoolsFromSDR = new DelegateCommand<object>(cancelGetSchoolsFromSDRAction);

            GetTeamsFromSDR = new DelegateCommand<object>(getTeamsFromSDRAction);
            CancelGetTeamsFromSDR = new DelegateCommand<object>(cancelGetTeamsFromSDRAction);

            DeleteLastPick = new DelegateCommand<object>(deleteLastPickAction);
            CancelDeleteLastPick = new DelegateCommand<object>(cancelDeleteLastPick);

            DeleteAllPicks = new DelegateCommand<object>(deleteAllPicksAction);
            CancelDeleteAllPicks = new DelegateCommand<object>(cancelDeleteAllPicks);

            _playlistCommands = new Dictionary<string, int>();

            if (ConfigurationManager.AppSettings["ConnectToPlayout"].ToString().ToUpper() == "TRUE")
            {
                connectToPlayout();
            }

            if (ConfigurationManager.AppSettings["RefreshPollInterval"].ToString() != "0")
            {
                _refreshPollTimer = new DispatcherTimer();
                _refreshPollTimer.Tick +=new EventHandler(refreshPollTimerElapsed);
                _refreshPollTimer.Interval = new TimeSpan(0, 0, Convert.ToInt16(ConfigurationManager.AppSettings["RefreshPollInterval"]));
                _refreshPollTimer.Start();
            }

            loadSchools();
            loadTeams();
            loadOnTheClock();
            loadDraftOrder();
            loadPlayers();

            loadCategories();
            loadInterruptions();

            TeamsAndPlayersVM = new TeamsAndPlayersViewModel(DbConnection.WebService);

            PlayerTabVM = new PlayerTabViewModel();
            PlayerTabVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(setStatusBarMsg);
            PlayerTabVM.DraftPlayerEvent += new PlayerTabViewModel.DraftPlayerEventHandler(draftPlayer);

            SchoolTabVM = new SchoolTabViewModel();
            SchoolTabVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(setStatusBarMsg);

            TeamTabVM = new TeamTabViewModel();
            TeamTabVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(setStatusBarMsg);

            CategoryTabVM = new CategoryTabViewModel();
            CategoryTabVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(setStatusBarMsg);

            InterruptionTabVM = new InterruptionTabViewModel();
            InterruptionTabVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(setStatusBarMsg);
            InterruptionTabVM.OnShowInterruption += new InterruptionTabViewModel.ShowInterruptionEventHandler(sendCommandToPlayout);
            InterruptionTabVM.OnStopCycle += new InterruptionTabViewModel.StopCycleEventHandler(stopCycle);
            
            DraftOrderTabVM = new DraftOrderTabViewModel();
            DraftOrderTabVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(setStatusBarMsg);
            DraftOrderTabVM.SendCommandEvent += new SendCommandEventHandler(sendCommandToPlayout);
            DraftOrderTabVM.StopCycleEvent += new StopCycleEventHandler(stopCycle);
            DraftOrderTabVM.StartCycleEvent += new StartCycleEventHandler(startCycle);
            DraftOrderTabVM.ResetCycleEvent += new ResetCycleEventHandler(resetCycleNoPrompt);

            PlaylistTabVM = new PlaylistTabViewModel();
            PlaylistTabVM.SetStatusBarMsg += new SetStatusBarMsgEventHandler(setStatusBarMsg);
            PlaylistTabVM.JumpToItemEvent += new PlaylistTabViewModel.JumpToItemEventHandler(jumpToPlaylistItem);
            PlaylistTabVM.SendCommandEvent += new PlaylistTabViewModel.SendCommandEventHandler(sendCommandToPlayout);
            PlaylistTabVM.SendCommandNoTransitionsEvent += new PlaylistTabViewModel.SendCommandNoTransitionsEventHandler(sendCommandNoTransitions);
            PlaylistTabVM.LoadPlaylists();

            CurrentSelectionTabVM = new CurrentSelectionTabViewModel();
            CurrentSelectionTabVM.SendCommandEvent += new CurrentSelectionTabViewModel.SendCommandEventHandler(sendCommandToPlayout);
            CurrentSelectionTabVM.StartCycleEvent += new CurrentSelectionTabViewModel.StartCycleEventHandler(startCycle);
            CurrentSelectionTabVM.StopCycleEvent += new CurrentSelectionTabViewModel.StopCycleEventHandler(stopCycle);
            CurrentSelectionTabVM.ResetCycleEvent += new CurrentSelectionTabViewModel.ResetCycleEventHandler(resetCycleNoPrompt);
            CurrentSelectionTabVM.RefreshPlayersEvent += new CurrentSelectionTabViewModel.RefreshPlayersEventHandler(refreshPlayers);

            loadClockOverlays();
            loadPollChips();
            loadRightLogos();
            getPollData();

            PollChip = "4GMC_logo_chip_ele.tga";
        }

        #endregion

        #region Private Methods

        private void loadSchools()
        {
            try
            {
                if (ConfigurationManager.AppSettings["LoadSchools"].ToString().ToUpper() != "FALSE")
                {
                    setStatusBarMsg("Loading schools...", "Yellow");

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.WorkerReportsProgress = false;

                    worker.DoWork += delegate(object s, DoWorkEventArgs args)
                    {
                        GlobalCollections.Instance.LoadSchools();
                    };

                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    {
                        setStatusBarMsg("Schools loaded at: " + DateTime.Now.ToString("h:mm:ss tt"), "Green");
                    };

                    worker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in LoadSchools: " + ex.ToString());
            }
        }

        private void loadTeams()
        {
            try
            {
                if (ConfigurationManager.AppSettings["LoadTeams"].ToString().ToUpper() != "FALSE")
                {
                    //setStatusBarMsg("Loading teams...", "Yellow");

                    //BackgroundWorker worker = new BackgroundWorker();
                    //worker.WorkerReportsProgress = false;

                    //worker.DoWork += delegate(object s, DoWorkEventArgs args)
                    //{
                        GlobalCollections.Instance.LoadTeams();
                    //};

                    //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    //{
                    //    setStatusBarMsg("Teams loaded at: " + DateTime.Now.ToString("h:mm:ss tt"), "Green");

                    //    loadDraftOrder();
                    //};

                    //worker.RunWorkerAsync();                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in LoadTeams: " + ex.ToString());
            }
        }

        private void loadDraftOrder()
        {
            try
            {
                if (ConfigurationManager.AppSettings["LoadDraftOrder"].ToString().ToUpper() != "FALSE")
                {
                    //setStatusBarMsg("Loading draft order...", "#f88803");

                    //BackgroundWorker worker = new BackgroundWorker();
                    //worker.WorkerReportsProgress = false;

                    //worker.DoWork += delegate(object s, DoWorkEventArgs args)
                    //{
                        GlobalCollections.Instance.LoadDraftOrder();
                    //};

                    //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    //{
                    //    setStatusBarMsg("Draft order loaded at: " + DateTime.Now.ToString("h:mm:ss tt"), "Green");
                    //};

                    //worker.RunWorkerAsync();                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in LoadDraftOrder: " + ex.ToString());
            }
        }

        private void loadPlayers()
        {
            try
            {
                if (ConfigurationManager.AppSettings["LoadPlayers"].ToString().ToUpper() != "FALSE")
                {
                    setStatusBarMsg("Loading players...", "#f88803");

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.WorkerReportsProgress = true;

                    worker.DoWork += delegate(object s, DoWorkEventArgs args)
                    {
                        GlobalCollections.Instance.LoadPlayers(worker);
                    };

                    worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
                    {
                        setStatusBarMsg("Loading players (" + args.ProgressPercentage.ToString() + "%)", "#f88803");
                    };

                    worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    {
                        setStatusBarMsg("Players loaded at: " + DateTime.Now.ToString("h:mm:ss tt"), "Green");
                        nextOnTheClock();
                        PlayerTabVM.FilteredPlayers = GlobalCollections.Instance.Players;
                    };

                    worker.RunWorkerAsync();                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in LoadPlayers: " + ex.ToString());
            }
        }

        private void loadOnTheClock()
        {
            try
            {
                if (ConfigurationManager.AppSettings["LoadOnTheClock"].ToString().ToUpper() != "FALSE")
                {
                    //setStatusBarMsg("Loading On The Clock...", "#f88803");

                    //BackgroundWorker worker = new BackgroundWorker();
                    //worker.WorkerReportsProgress = false;

                    //worker.DoWork += delegate(object s, DoWorkEventArgs args)
                    //{
                        GlobalCollections.Instance.LoadOnTheClock();
                    //};

                    //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                    //{
                    //    setStatusBarMsg("On The Clock loaded at: " + DateTime.Now.ToString("h:mm:ss tt"), "Green");
                    //};

                    //worker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in LoadOnTheClock: " + ex.ToString());
            }
        }

        private void loadCategories()
        {
            try
            {
                //setStatusBarMsg("Loading Categories...", "#f88803");

                //BackgroundWorker worker = new BackgroundWorker();
                //worker.WorkerReportsProgress = false;

                //worker.DoWork += delegate(object s, DoWorkEventArgs args)
                //{
                    GlobalCollections.Instance.LoadCategories();
                //};

                //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                //{
                //    setStatusBarMsg("Categories loaded at: " + DateTime.Now.ToString("h:mm:ss tt"), "Green");
                //};

                //worker.RunWorkerAsync();                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in LoadCategories: " + ex.ToString());
            }
        }

        private void loadInterruptions()
        {
            try
            {
                //setStatusBarMsg("Loading Interruptions...", "#f88803");

                //BackgroundWorker worker = new BackgroundWorker();
                //worker.WorkerReportsProgress = false;

                //worker.DoWork += delegate(object s, DoWorkEventArgs args)
                //{
                    GlobalCollections.Instance.LoadInterruptions();
                //};

                //worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                //{
                //    setStatusBarMsg("Interruptions loaded at: " + DateTime.Now.ToString("h:mm:ss tt"), "Green");
                //};

                //worker.RunWorkerAsync();                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in LoadInterruptions: " + ex.ToString());
            }
        }

        private void connectToPlayout()
        {
            if (_checkPlayoutConnectionTimer == null)
            {
                _checkPlayoutConnectionTimer = new DispatcherTimer();
                _checkPlayoutConnectionTimer.Tick += new EventHandler(checkPlayoutConnectionTimerElapsed);
                _checkPlayoutConnectionTimer.Interval = new TimeSpan(0, 0, 5);
                _checkPlayoutConnectionTimer.IsEnabled = true;
            }

            if (_reconnectPlayoutTimer == null)
            {
                _reconnectPlayoutTimer = new DispatcherTimer();
                _reconnectPlayoutTimer.Tick += new EventHandler(reconnectPlayoutTimerElapsed);
                _reconnectPlayoutTimer.Interval = new TimeSpan(0, 0, 5);
            }

            initializeTalker();
        }

        private void initializeTalker()
        {
            if (_compTalker != null)
            {
                _compTalker.Dispose();
            }

            _compTalker = new Talker("1");

            _compTalker.DataArrival += new Talker.DataArrivalHandler(compTalkerDataArrival);
            _compTalker.Connected += new Talker.ConnectionHandler(compConnectedTo);
            _compTalker.ConnectionClosed += new Talker.ConnectionClosedHandler(compConnectionClosed);
            //_compTalker.ConnectionRefused += new Talker.ConnectionRefusedHandler(compConnectionRefused);

            _compTalker.Connect(ConfigurationManager.AppSettings["PlayoutIP"].ToString(), ConfigurationManager.AppSettings["CompPlayoutPort"]);

            if (_clockTalker != null)
            {
                _clockTalker.Dispose();
            }

            _clockTalker = new Talker("2");

            _clockTalker.DataArrival += new Talker.DataArrivalHandler(clockTalkerDataArrival);
            _clockTalker.Connected += new Talker.ConnectionHandler(clockConnectedTo);
            _clockTalker.ConnectionClosed += new Talker.ConnectionClosedHandler(clockConnectionClosed);
            //_clockTalker.ConnectionRefused += new Talker.ConnectionRefusedHandler(clockConnectionRefused);

            _clockTalker.Connect(ConfigurationManager.AppSettings["PlayoutIP"].ToString(), ConfigurationManager.AppSettings["ClockPlayoutPort"]);
        
        }

        private void compConnectedTo()
        {            
            _playoutConnectionAlive = true;
            _reconnectPlayoutTimer.IsEnabled = false;

            setPlayoutBarMsg("Connected to playout", "Green");
        }

        private void compTalkerDataArrival(PlayerCommand CommandToProcess, string ID)
        {
            KeyValuePair<string, int>? playlistCommand = null;

            setPlayoutFeedbackDelegate handler = setPlayoutFeedback;

            handler(CommandToProcess.Command.ToString());

            switch (CommandToProcess.Command.ToString())
            {
                case "CommandSuccessful":
                    if (CommandToProcess.CommandID != null)
                    {
                        if (CommandToProcess.CommandID.ToString() == "INIT")
                        {
                            _playoutInitialized = true;
                        }
                        else
                        {
                            playlistCommand = _playlistCommands.SingleOrDefault(p => p.Key == CommandToProcess.CommandID.ToString());

                            if (playlistCommand != null)
                            {
                                int playlistID = playlistCommand.Value.Value;

                                if (_playlistTimerRunning)
                                {
                                    Playlist loadedPlaylist = PlaylistTabVM.LoadedPlaylists.SingleOrDefault(p => p.PlaylistID == playlistID);
                                    System.Timers.Timer timer = null;

                                    if (loadedPlaylist != null)
                                    {
                                        timer = loadedPlaylist.Timer;
                                    }
                                    
                                    if (timer != null) { timer.Start(); }
                                }

                                if (_playlistCommands.Count > 0)
                                {
                                    _playlistCommands.Remove(playlistCommand.Value.Key);
                                }                                 
                            }
                            else
                            {
                                if (_playlistTimerRunning)
                                {
                                    playlistCommand = _playlistCommands.SingleOrDefault(p => p.Key == CommandToProcess.CommandID.ToString());

                                    if (playlistCommand != null)
                                    {
                                        int playlistID = playlistCommand.Value.Value;

                                        if (_playlistTimerRunning)
                                        {
                                            //System.Timers.Timer timer = GlobalCollections.Instance.PlaylistTimers.FirstOrDefault(p => p.Key.PlaylistID == playlistID).Value;
                                            System.Timers.Timer timer = PlaylistTabVM.LoadedPlaylists.FirstOrDefault(p => p.PlaylistID == playlistID).Timer;

                                            if (timer != null) { timer.Start(); }
                                        }

                                        _playlistCommands.Remove(playlistCommand.Value.Key);
                                    }
                                }
                            }
                        }
                    }
                    break;   
                case "CommandFailed":
                    if (_playlistTimerRunning)
                    {
                        playlistCommand = _playlistCommands.SingleOrDefault(p => p.Key == CommandToProcess.CommandID.ToString());

                        if (playlistCommand != null)
                        {
                            int playlistID = playlistCommand.Value.Value;

                            if (_playlistTimerRunning)
                            {
                                //System.Timers.Timer timer = GlobalCollections.Instance.PlaylistTimers.FirstOrDefault(p => p.Key.PlaylistID == playlistID).Value;
                                System.Timers.Timer timer = PlaylistTabVM.LoadedPlaylists.FirstOrDefault(p => p.PlaylistID == playlistID).Timer;

                                if (timer != null) { timer.Start(); }
                            }

                            _playlistCommands.Remove(playlistCommand.Value.Key);
                        }
                    }
                    break;
            }
        }
          
        private void compConnectionClosed()
        {
            if (_playlistTimerRunning)
            {
                PlaylistTimerRunning = false;
            }

            setPlayoutBarMsg("Playout disconnected", "Red");

            _playoutConnectionAlive = false;
            _playoutInitialized = false;
            _reconnectPlayoutTimer.IsEnabled = true;            
        }

        private void compConnectionRefused()
        {
            if (_playlistTimerRunning)
            {
                PlaylistTimerRunning = false;
            }

            setPlayoutBarMsg("Playout connection refused", "Red");

            _playoutConnectionAlive = false;
            _playoutInitialized = false;
            _reconnectPlayoutTimer.IsEnabled = true; 
        }

        private void clockConnectedTo()
        {
            
        }

        private void clockTalkerDataArrival(PlayerCommand CommandToProcess, string ID)
        {
            
        }

        private void clockConnectionClosed()
        {
            
        }

        private void clockConnectionRefused()
        {

        }

        private void setPlayoutFeedback(string feedback)
        {
            PlayoutFeedback = feedback;           
        }

        private void initializePlayout()
        {
            PromptMessage = "Initialize playout?  The compression output will be reset!";
            AskInitializePlayout = true;
        }

        private void getSchoolsFromSDR()
        {
            PromptMessage = "Get schools from SDR?";
            AskGetSchoolsFromSDR = true;
        }

        private void getTeamsFromSDR()
        {
            PromptMessage = "Get NFL teams from SDR?";
            AskGetTeamsFromSDR = true;
        }

        private void l3TimerElapsed(object sender, EventArgs e)
        {
            
        }

        private void refreshPollTimerElapsed(object sender, EventArgs e)
        {
            if (_refreshPoll)
            {
                _refreshPollTimer.Stop();
                getPollData();
                _refreshPollTimer.Start();
            }
        }
        
        private void jumpToPlaylistItem(int playlistItemOrder)
        {
            
        }

        private void playlistAdded(Playlist playlist, System.Timers.Timer timer)
        {
            if (_playlistTimerRunning)
            {
                timer.Interval = 100;
                timer.Start();
            }
        }

        private void startCycle()
        {
            PlaylistTimerRunning = true;
        }

        private void stopCycle()
        {
            PlaylistTimerRunning = false;            
        }

        private void updateClock()
        {
            PlayerCommand commandToSend = new PlayerCommand();

            XmlDataRow xmlRow = new XmlDataRow();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();

            if (_takeClock)
            {
                if (_playlistTabVM.SelectedPlaylist != null)
                {
                    switch (_playlistTabVM.SelectedPlaylist.PlaylistName.ToUpper())
                    {
                        case "PROMPTER":
                            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Prompter"));
                            break;
                        case "BREAK CLOCK":
                            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "BreakClock"));
                            break;
                        default:
                            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));
                            xmlRow.Add("TURN_CLOCK_RED", _clockRedUnderMin.ToString().ToUpper());
                            break;
                    }
                }
                else
                {
                    commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));
                    xmlRow.Add("TURN_CLOCK_RED", _clockRedUnderMin.ToString().ToUpper());
                }

                switch (_clockSeconds)
                {
                    case 0:
                        if (_prevClockSeconds != 0)
                        {
                            xmlRow.Add("CLOCK_OVERLAY", "PICK IS IN");
                        }
                        break;
                    case 60:
                        if (_clockRedUnderMin)
                        {
                            xmlRow.Add("CLOCK_COLOR", "RED");
                        }
                        break;
                    default:
                        if (_clockSeconds > _prevClockSeconds && _prevClockSeconds > 0)
                        {
                            xmlRow.Add("CLOCK_OVERLAY", "PICK IS IN");
                            xmlRow.Add("CLOCK", "");
                            _pickIsIn = true;
                        }
                        else
                        {
                            //if (_useCountdownClock)
                            //{
                            //    xmlRow.Add("CLOCK", _countdownClock);
                            //}
                            //else
                            //{
                            //    if (_pickIsIn == false)
                            //    {
                            //        xmlRow.Add("CLOCK", _clock);
                            //    }
                            //}      

                            commandToSend.Parameters.Add(new CommandParameter("MergeDataWithoutTransitions", "true"));
                        }

                        break;
                }

                if (_clockSeconds == 0)
                {
                    _takeClock = false;
                        
                }
            }
            else
            {
                //commandToSend.Parameters.Add(new CommandParameter("MergeDataWithoutTransitions", "true"));
            }

            _prevClockSeconds = _clockSeconds;

            if (_useCountdownClock)
            {
                xmlRow.Add("CLOCK", _countdownClock);
            }
            else
            {
                if (_pickIsIn == false)
                {
                    xmlRow.Add("CLOCK", _clock);
                }
            }      

            commandToSend.TemplateData = xmlRow.GetXMLString();

            _clockTalker.Talk(commandToSend);
        }

        private void updateCountdownClock()
        {
            if (_useCountdownClock)
            {
                if (_playlistTabVM.SelectedPlaylist != null)
                {
                    PlayerCommand commandToSend = new PlayerCommand();

                    XmlDataRow xmlRow = new XmlDataRow();
                
                    commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                                
                    commandToSend.Parameters = new List<CommandParameter>();

                    if (_playlistTabVM.SelectedPlaylist.PlaylistName.ToUpper() == "PROMPTER")
                    {
                        commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Prompter"));
                    }
                    else
                    {
                        commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));
                    }
                
                    commandToSend.Parameters.Add(new CommandParameter("MergeDataWithoutTransitions", "true")); 
                
                    xmlRow.Add("CLOCK", _countdownClock);

                    commandToSend.TemplateData = xmlRow.GetXMLString();

                    _clockTalker.Talk(commandToSend);
                }
            }
        }              

        private void getCountdownClock(string targetTime)
        {
            string result = "";

            DateTime current = DateTime.Now;

            try
            {
                DateTime target = Convert.ToDateTime(targetTime);

                if (target > current)
                {
                    TimeSpan diff = target.Subtract(current);

                    if (diff.Days > 0)
                    {
                        if (diff.Days == 1)
                        {
                            result = diff.Days.ToString() + " day ";
                        }
                        else
                        {
                            result = diff.Days.ToString() + " days ";
                        }
                    }

                    if (diff.Hours > 0)
                    {
                        result += diff.Hours.ToString() + ":";
                    }                    
                    
                    if (diff.Minutes.ToString().Length == 1 && diff.Hours > 0)
                    {
                        result += "0" + diff.Minutes.ToString() + ":";
                    }
                    else
                    {
                        result += diff.Minutes.ToString() + ":";
                    }

                    if (diff.Seconds.ToString().Length == 1)
                    {
                        result += "0" + diff.Seconds.ToString();
                    }
                    else
                    {
                        result += diff.Seconds.ToString();
                    }
                    
                }


            }
            finally
            {

            }

            _countdownClock = result;
        }
        
        private void sendCommandToPlayout(PlayerCommand commandToSend, Playlist playlist = null)
        {
            try
            {
                PlayoutOutgoingCommand = commandToSend.TemplateData;

                if (playlist == null)
                {
                    _playlistCommands.Add(commandToSend.CommandID, 0);
                }
                else
                {
                    _playlistCommands.Add(commandToSend.CommandID, playlist.PlaylistID);
                }

                _lastCommandID = commandToSend.CommandID;

                _compTalker.Talk(commandToSend);
            }
            catch (Exception ex)
            { }
        }
        
        private void setStatusBarMsg(string msgText, string msgColor)
        {
            StatusMessageText = msgText;
            StatusMessageColor = msgColor;
        }

        private void setPlayoutBarMsg(string msgText, string msgColor)
        {
            PlayoutMessageText = msgText;
            PlayoutMessageColor = msgColor;
        }

        private void sendCommandNoTransitions(PlayerCommand commandToSend)
        {
            _clockTalker.Talk(commandToSend);
        }
        
        private void reconnectPlayoutTimerElapsed(object sender, EventArgs e)
        {
            _compTalker.DataArrival -= compTalkerDataArrival;
            _compTalker.Connected -= compConnectedTo;
            _compTalker.ConnectionClosed -= compConnectionClosed;
            //_compTalker.ConnectionRefused -= compConnectionRefused;
            _compTalker.Dispose();
            _compTalker = null;

            _clockTalker.DataArrival -= clockTalkerDataArrival;
            _clockTalker.Connected -= clockConnectedTo;
            _clockTalker.ConnectionClosed -= clockConnectionClosed;
            _clockTalker.Dispose();
            _clockTalker = null;

            initializeTalker();
        }

        private void checkPlayoutConnectionTimerElapsed(object sender, EventArgs e)
        {
            _checkPlayoutConnectionTimer.IsEnabled = false;
            _checkPlayoutConnectionTimer.Tick -= checkPlayoutConnectionTimerElapsed;
            _checkPlayoutConnectionTimer = null;

            if (_playoutConnectionAlive == false)
            {
                setPlayoutBarMsg("Playout not connected", "Red");
                _reconnectPlayoutTimer.IsEnabled = true;
            }
        }

        private void importPlayers()
        {
            PromptMessage = "Import players from " + ConfigurationManager.AppSettings["PlayersDataFile"].ToString() + "?";
            AskImportPlayers = true;
        }

        private void importTeams()
        {
            PromptMessage = "Import teams from " + ConfigurationManager.AppSettings["TeamsDataFile"].ToString() + "?";
            AskImportTeams = true;
        }

        private void resetCycle()
        {
            PromptMessage = "Reset cycle?";
            AskResetCycle = true;
        }

        private void resetCycleNoPrompt()
        {
            _playlistTabVM.ResetPlaylists();
        }

        private void importPlayersAction(object parameter)
        {
            AskImportPlayers = false;

            DbConnection.ImportPlayers(ConfigurationManager.AppSettings["PlayerDataFile"].ToString());            
        }

        private void cancelImportPlayersAction(object parameter)
        {
            AskImportPlayers = false;
        }

        private void importTeamsAction(object parameter)
        {
            AskImportTeams = false;

            DbConnection.ImportTeams(ConfigurationManager.AppSettings["TeamsDataFile"].ToString());
        }

        private void cancelImportTeamsAction(object parameter)
        {
            AskImportTeams = false;
        }

        private void resetCycleAction(object parameter)
        {
            PlaylistTimerRunning = false;
            _playlistTabVM.ResetPlaylists();
            AskResetCycle = false;
        }

        private void cancelResetCycleAction(object parameter)
        {
            AskResetCycle = false;
        }

        private void initializePlayoutAction(object parameter)
        {
            AskInitializePlayout = false;

            PlayerCommand commandToSend = new PlayerCommand();
            
            //uncomment this if we want the admin to initialize the playout when the app starts...
            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "Initialize");
            commandToSend.CommandID = "INIT";
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateDirectory", ConfigurationManager.AppSettings["TemplateDirectory"].ToString()));
            commandToSend.Parameters.Add(new CommandParameter("WorkingDirectory", ConfigurationManager.AppSettings["TemplateDirectory"].ToString()));
            commandToSend.Parameters.Add(new CommandParameter("OutputType", ConfigurationManager.AppSettings["OutputType"].ToString()));

            sendCommandToPlayout(commandToSend);
            //*************************************************************************************

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            //commandToSend.Parameters.Add(new CommandParameter("TemplateDirectory", ConfigurationManager.AppSettings["TemplateDirectory"].ToString()));
            //commandToSend.Parameters.Add(new CommandParameter("WorkingDirectory", ConfigurationManager.AppSettings["TemplateDirectory"].ToString()));
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Background"));
            commandToSend.Parameters.Add(new CommandParameter("QueueCommand", "true"));

            sendCommandToPlayout(commandToSend);

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));
            commandToSend.Parameters.Add(new CommandParameter("QueueCommand", "true"));

            sendCommandToPlayout(commandToSend);

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Next"));
            commandToSend.Parameters.Add(new CommandParameter("QueueCommand", "true"));

            sendCommandToPlayout(commandToSend);

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "RightLogo"));

            sendCommandToPlayout(commandToSend);

            //nextL3Item();
            //nextRTItem();
        }

        private void sendCommandToPlayout(PlayerCommand commandToSend)
        {
            PlayoutOutgoingCommand = commandToSend.Command + "; " + commandToSend.TemplateData;

            _compTalker.Talk(commandToSend);
        }

        private void cancelInitializePlayoutAction(object parameter)
        {
            AskInitializePlayout = false;
        }

        private void getSchoolsFromSDRAction(object parameter)
        {
            AskGetSchoolsFromSDR = false;

            DbConnection.GetSchoolsFromSDR();
        }

        private void cancelGetSchoolsFromSDRAction(object parameter)
        {
            AskGetSchoolsFromSDR = false;
        }

        private void getTeamsFromSDRAction(object parameter)
        {
            AskGetTeamsFromSDR = false;

            DbConnection.GetProTeamsFromSDR();
        }

        private void cancelGetTeamsFromSDRAction(object parameter)
        {
            AskGetTeamsFromSDR = false;
        }

        private void togglePlaylistTimer()
        {
            if (_playlistTimerRunning)
            {
                PlaylistTimerRunning = false;
            }
            else
            {
                if (_playoutConnectionAlive)
                {
                    if (PlaylistTabVM.LoadedPlaylists != null)
                    {
                        PlaylistTimerRunning = true;

                        if (StatusMessageText == "No playlists loaded")
                        {
                            setStatusBarMsg("", "Green");
                        }
                    }
                    else
                    {
                        setStatusBarMsg("No playlists loaded.", "Red");
                    }
                }
            }
        }

        private void loadClockOverlays()
        {
            if (ClockOverlays == null)
            {
                ClockOverlays = new ObservableCollection<LogoChip>();
            }
            else
            {
                ClockOverlays.Clear();
            }

            ClockOverlays.Add(new LogoChip("<NONE>", null));

            DirectoryInfo dir = new DirectoryInfo(ConfigurationManager.AppSettings["ClockOverlayDirectory"].ToString());

            foreach (FileInfo file in dir.GetFiles())
            {
                Bitmap bmp = TargaImage.LoadTargaImage(file.FullName);
                var strm = new System.IO.MemoryStream();
                bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Bmp);

                BitmapImage logoBitmap = new BitmapImage();
                logoBitmap.BeginInit();
                logoBitmap.StreamSource = strm;
                logoBitmap.EndInit();

                ClockOverlays.Add(new LogoChip(file.Name, logoBitmap));
            }
        }

        private void loadRightLogos()
        {
            if (RightLogos == null)
            {
                RightLogos = new ObservableCollection<LogoChip>();
            }
            else
            {
                RightLogos.Clear();
            }

            RightLogos.Add(new LogoChip("<NONE>", null));

            DirectoryInfo dir = new DirectoryInfo(ConfigurationManager.AppSettings["RightLogoDirectory"].ToString());

            foreach (FileInfo file in dir.GetFiles())
            {
                Bitmap bmp = TargaImage.LoadTargaImage(file.FullName);
                var strm = new System.IO.MemoryStream();
                bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Bmp);

                BitmapImage logoBitmap = new BitmapImage();
                logoBitmap.BeginInit();
                logoBitmap.StreamSource = strm;
                logoBitmap.EndInit();

                RightLogos.Add(new LogoChip(file.Name, logoBitmap));
            }
        }

        private void loadPollChips()
        {
            if (PollChips == null)
            {
                PollChips = new ObservableCollection<LogoChip>();
            }
            else
            {
                PollChips.Clear();
            }

            DirectoryInfo dir = new DirectoryInfo(ConfigurationManager.AppSettings["PollChipDirectory"].ToString());

            foreach (FileInfo file in dir.GetFiles())
            {
                Bitmap bmp = TargaImage.LoadTargaImage(file.FullName);
                var strm = new System.IO.MemoryStream();
                bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Bmp);

                BitmapImage logoBitmap = new BitmapImage();
                logoBitmap.BeginInit();
                logoBitmap.StreamSource = strm;
                logoBitmap.EndInit();

                PollChips.Add(new LogoChip(file.Name, logoBitmap));
            }
        }

        private void showClockOverlay()
        {
            if (_selectedClockOverlay != null)
            {
                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));

                XmlDataRow xmlRow = new XmlDataRow();

                if (_selectedClockOverlay.FileName == "<NONE>")
                {
                    xmlRow.Add("CLOCK_OVERLAY", "ON THE CLOCK");
                }
                else
                {
                    xmlRow.Add("CLOCK_OVERLAY", "OVERLAY");
                    xmlRow.Add("CHIP_1", ConfigurationManager.AppSettings["ClockOverlayDirectory"].ToString() + "\\" + _selectedClockOverlay.FileName);
                }

                commandToSend.TemplateData = xmlRow.GetXMLString();
                commandToSend.CommandID = Guid.NewGuid().ToString();

                sendCommandToPlayout(commandToSend);
            }
        }

        private void showRightLogo()
        {
            if (_selectedRightLogo != null)
            {
                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "RightLogo"));

                XmlDataRow xmlRow = new XmlDataRow();

                if (_selectedRightLogo.FileName == "<NONE>")
                {
                    xmlRow.Add("CHIP_1", "");
                    xmlRow.Add("VISIBLE", "0");
                }
                else
                {
                    xmlRow.Add("CHIP_1", ConfigurationManager.AppSettings["RightLogoDirectory"].ToString() + "\\" + _selectedRightLogo.FileName);
                    xmlRow.Add("VISIBLE", "1");
                }

                commandToSend.TemplateData = xmlRow.GetXMLString();
                commandToSend.CommandID = Guid.NewGuid().ToString();

                sendCommandToPlayout(commandToSend);
            }
        }

        private void showClock()
        {
            _takeClock = true;
            _pickIsIn = false;

            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));

            XmlDataRow xmlRow = new XmlDataRow();

            xmlRow.Add("CLOCK_OVERLAY", "CLOCK");

            commandToSend.TemplateData = xmlRow.GetXMLString();

            sendCommandToPlayout(commandToSend);
        }

        private void draftPlayer(Int32 playerId)
        {
            _currentSelectionTabVM.CurrentPlayer = DbConnection.GetPlayer(playerId);
        }

        private void refreshPlayers()
        {
            PlayerTabVM.RefreshPlayers();
        }

        private void getPollData()
        {
            try
            {
                WebRequest request = WebRequest.Create(ConfigurationManager.AppSettings["PollUrl"].ToString());

                WebResponse response = request.GetResponse();

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);

                string fullText = reader.ReadToEnd();

                fullText = fullText.Replace("<br>", "\r\n");

                PollText = fullText;

                StringReader strReader = new StringReader(fullText);

                _pollTextLines = new List<string>();

                string line;

                while ((line = strReader.ReadLine()) != null)
                {
                    if (line.Trim() != "")
                    {
                        _pollTextLines.Add(line);
                    }
                }

                reader.Close();
                response.Close();

                DbConnection.SavePoll(_pollTextLines);
            }
            catch (WebException we)
            {

            }
            finally
            {

            }            
        }

        private void nextOnTheClock()
        {
            _takeClock = false;

            Global.GlobalCollections.Instance.LoadOnTheClock();

            PlayerCommand commandToSend;

            //send a command to change the clock on-air
            commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.CommandID = Guid.NewGuid().ToString();
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Clock"));

            XmlDataRow xmlRow = new XmlDataRow();

            if (GlobalCollections.Instance.OnTheClock != null)
            {
                xmlRow.Add("ROUND_1", Global.GlobalCollections.Instance.OnTheClock.Round.ToString());
                xmlRow.Add("PICK_1", Global.GlobalCollections.Instance.OnTheClock.OverallPick.ToString());
                xmlRow.Add("LOGO_1", Global.GlobalCollections.Instance.OnTheClock.Team.LogoTgaNoKey.LocalPath);
                xmlRow.Add("SWATCH_1", Global.GlobalCollections.Instance.OnTheClock.Team.SwatchTga.LocalPath);
                xmlRow.Add("ABBREV_4_1", Global.GlobalCollections.Instance.OnTheClock.Team.Tricode);
            }

            xmlRow.Add("CLOCK_OVERLAY", "ON THE CLOCK");
            xmlRow.Add("CLOCK", "");

            commandToSend.TemplateData = xmlRow.GetXMLString();
            commandToSend.CommandID = Guid.NewGuid().ToString();

            updateTotem(true);

            sendCommandToPlayout(commandToSend);

            //hide the current selection template
            commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "HidePage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "CurrentSelection"));
            //commandToSend.Parameters.Add(new CommandParameter("QueueCommand", "true"));

            sendCommandToPlayout(commandToSend);
        }

        private void updateTotem(bool changeTotem = false)
        {
            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.CommandID = Guid.NewGuid().ToString();
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Next"));

            XmlDataRow xmlRow = new XmlDataRow();

            Pick nextPick1;
            Pick nextPick2;

            nextPick1 = (Pick)Global.GlobalCollections.Instance.DraftOrder.SingleOrDefault(p => p.OverallPick == Global.GlobalCollections.Instance.OnTheClock.OverallPick + 1);
            nextPick2 = (Pick)Global.GlobalCollections.Instance.DraftOrder.SingleOrDefault(p => p.OverallPick == Global.GlobalCollections.Instance.OnTheClock.OverallPick + 2);

            if (nextPick1 != null)
            {
                xmlRow.Add("ABBREV_4_1", nextPick1.Team.Tricode);

                if (nextPick2 != null)
                {
                    xmlRow.Add("ABBREV_4_2", nextPick2.Team.Tricode);
                }
                else
                {
                    xmlRow.Add("ABBREV_4_2", "");
                }
            }
            else
            {
                xmlRow.Add("ABBREV_4_1", "");
                xmlRow.Add("ABBREV_4_2", "");
            } 

            if (changeTotem)
            {
                xmlRow.Add("CHANGE_TOTEM_FLAG", "1");
            }
            else
            {
                xmlRow.Add("CHANGE_TOTEM_FLAG", "");
            }

            commandToSend.TemplateData = xmlRow.GetXMLString();

            sendCommandToPlayout(commandToSend);
        }

        private void changePlaylist(string playlist)
        {
            _currentPlaylist = playlist;
        }

        private void deleteLastPick()
        {
            if (GlobalCollections.Instance.OnTheClock.OverallPick > 1)
            {
                PromptMessage = "Delete last pick?";
                AskDeleteLastPick = true;
            }
        }

        private void deleteAllPicks()
        {
            PromptMessage = "Delete ALL picks?";
            AskDeleteAllPicks = true;
        }

        private void deleteLastPickAction(object parameter)
        {
            AskDeleteLastPick = false;

            if (DbConnection.DeleteLastPick(GlobalCollections.Instance.OnTheClock.OverallPick - 1) == true)
            {
                //GlobalCollections.Instance.LoadPlayers();
                loadPlayers();

                Global.GlobalCollections.Instance.LoadOnTheClock();
            }
        }

        private void cancelDeleteLastPick(object parameter)
        {
            AskDeleteLastPick = false;
        }

        private void deleteAllPicksAction(object parameter)
        {
            AskDeleteAllPicks = false;

            if (DbConnection.DeleteAllPicks() == true)
            {
                //GlobalCollections.Instance.LoadPlayers();
                loadPlayers();
                Global.GlobalCollections.Instance.LoadOnTheClock();
            }
        }

        private void cancelDeleteAllPicks(object parameter)
        {
            AskDeleteAllPicks = false;
        }

        private void showPollQuestion()
        {
            string[] poll = DbConnection.GetPoll(false);

            if (poll != null)
            {
                string pollChip = ConfigurationManager.AppSettings["PollChipDirectory"].ToString() + "\\" + _selectedPollChip.FileName;

                if (File.Exists(pollChip))
                {
                    stopCycle();

                    PlayerCommand commandToSend;

                    commandToSend = new PlayerCommand();

                    commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                    commandToSend.CommandID = Guid.NewGuid().ToString();
                    commandToSend.Parameters = new List<CommandParameter>();
                    commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Poll"));

                    XmlDataRow xmlRow = new XmlDataRow();

                    xmlRow.Add("TITLE_1", poll[0]);
                    xmlRow.Add("TIDBIT_1", poll[1]);

                    xmlRow.Add("CHIP_1", pollChip);
                    xmlRow.Add("VENT_SWATCH_1", "Images\\Swatches\\black_swatch.tga");

                    commandToSend.TemplateData = xmlRow.GetXMLString();

                    sendCommandToPlayout(commandToSend);
                }
            }
        }

        private void showPollResults()
        {
            string[] poll = DbConnection.GetPoll(true);

            if (poll != null)
            {
                string pollChip = ConfigurationManager.AppSettings["PollChipDirectory"].ToString() + "\\" + _selectedPollChip.FileName;

                if (File.Exists(pollChip))
                {
                    stopCycle();

                    PlayerCommand commandToSend;

                    commandToSend = new PlayerCommand();

                    commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                    commandToSend.CommandID = Guid.NewGuid().ToString();
                    commandToSend.Parameters = new List<CommandParameter>();
                    commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Poll"));

                    XmlDataRow xmlRow = new XmlDataRow();

                    xmlRow.Add("TITLE_1", poll[0]);
                    xmlRow.Add("TIDBIT_1", poll[1]);

                    xmlRow.Add("CHIP_1", pollChip);
                    xmlRow.Add("VENT_SWATCH_1", "Images\\Swatches\\black_swatch.tga");

                    commandToSend.TemplateData = xmlRow.GetXMLString();

                    sendCommandToPlayout(commandToSend);
                }
            }
        }

        #endregion
        
        #region Public Methods

        #endregion

        #region Commands

        public ICommand ImportPlayersCommand
        {
            get
            {
                if (_importPlayersCommand == null)
                {
                    _importPlayersCommand = new DelegateCommand(importPlayers);
                }
                return _importPlayersCommand;
            }
        }

        public ICommand ImportTeamsCommand
        {
            get
            {
                if (_importTeamsCommand == null)
                {
                    _importTeamsCommand = new DelegateCommand(importTeams);
                }
                return _importTeamsCommand;
            }
        }

        public ICommand ResetCycleCommand
        {
            get
            {
                if (_resetCycleCommand == null)
                {
                    _resetCycleCommand = new DelegateCommand(resetCycle);
                }
                return _resetCycleCommand;
            }
        }

        public ICommand TogglePlaylistTimerCommand
        {
            get
            {
                if (_togglePlaylistTimerCommand == null)
                {
                    _togglePlaylistTimerCommand = new DelegateCommand(togglePlaylistTimer);
                }
                return _togglePlaylistTimerCommand;
            }
        }

        public ICommand InitializePlayoutCommand
        {
            get
            {
                if (_initializePlayoutCommand == null)
                {
                    _initializePlayoutCommand = new DelegateCommand(initializePlayout);
                }
                return _initializePlayoutCommand;
            }
        }

        public ICommand ShowClockOverlayCommand
        {
            get
            {
                if (_showClockOverlayCommand == null)
                {
                    _showClockOverlayCommand = new DelegateCommand(showClockOverlay);
                }
                return _showClockOverlayCommand;
            }
        }

        public ICommand ShowRightLogoCommand
        {
            get
            {
                if (_showRightLogoCommand == null)
                {
                    _showRightLogoCommand = new DelegateCommand(showRightLogo);
                }
                return _showRightLogoCommand;
            }
        }

        public ICommand ShowClockCommand
        {
            get
            {
                if (_showClockCommand == null)
                {
                    _showClockCommand = new DelegateCommand(showClock);
                }
                return _showClockCommand;
            }
        }

        public ICommand ConnectToPlayoutCommand
        {
            get
            {
                if (_connectToPlayoutCommand == null)
                {
                    _connectToPlayoutCommand = new DelegateCommand(connectToPlayout);
                }
                return _connectToPlayoutCommand;
            }
        }

        public ICommand NextOnTheClockCommand
        {
            get
            {
                if (_nextOnTheClockCommand == null)
                {
                    _nextOnTheClockCommand = new DelegateCommand(nextOnTheClock);
                }
                return _nextOnTheClockCommand;
            }
        }

        public ICommand DeleteLastPickCommand
        {
            get
            {
                if (_deleteLastPickCommand == null)
                {
                    _deleteLastPickCommand = new DelegateCommand(deleteLastPick);
                }
                return _deleteLastPickCommand;
            }
        }

        public ICommand DeleteAllPicksCommand
        {
            get
            {
                if (_deleteAllPicksCommand == null)
                {
                    _deleteAllPicksCommand = new DelegateCommand(deleteAllPicks);
                }
                return _deleteAllPicksCommand;
            }
        }

        public ICommand RefreshOverlaysCommand
        {
            get
            {
                if (_refreshOverlaysCommand == null)
                {
                    _refreshOverlaysCommand = new DelegateCommand(loadClockOverlays);
                }
                return _refreshOverlaysCommand;
            }
        }

        public ICommand RefreshRightLogosCommand
        {
            get
            {
                if (_refreshRightLogosCommand == null)
                {
                    _refreshRightLogosCommand = new DelegateCommand(loadRightLogos);
                }
                return _refreshRightLogosCommand;
            }
        }

        public ICommand RefreshPollChipsCommand
        {
            get
            {
                if (_refreshPollChipsCommand == null)
                {
                    _refreshPollChipsCommand = new DelegateCommand(loadPollChips);
                }
                return _refreshPollChipsCommand;
            }
        }
        
        public ICommand ShowPollQuestionCommand
        {
            get
            {
                if (_showPollQuestionCommand == null)
                {
                    _showPollQuestionCommand = new DelegateCommand(showPollQuestion);
                }
                return _showPollQuestionCommand;
            }
        }

        public ICommand ShowPollResultsCommand
        {
            get
            {
                if (_showPollResultsCommand == null)
                {
                    _showPollResultsCommand = new DelegateCommand(showPollResults);
                }
                return _showPollResultsCommand;
            }
        }

        #endregion
    }
}
