using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitterLib;
using System.Windows.Input;
using DraftAdmin.Commands;
using System.Configuration;
using System.Globalization;
using System.Xml;

using DraftAdmin.Models;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;
using DraftAdmin.PlayoutCommands;
using DraftAdmin.Output;
using System.Drawing;
using DraftAdmin.Utilities;
using System.Windows.Media.Imaging;

namespace DraftAdmin.ViewModels
{
    public class TwitterViewModel : ViewModelBase
    {
        #region Private Members

        private Twitter _twitter;

        private ObservableCollection<TweetViewModel> _tweetVMs;
        private TweetViewModel _selectedTweet;

        private DelegateCommand _getFavoritesCommand;
        private DelegateCommand _showTweetCommand;
        private DelegateCommand _refreshLogosCommand;

        private bool _getFavoritesButtonEnabled = true;

        private string _selectedTweetText;
        private string _selectedTweetUsername;

        private ObservableCollection<LogoChip> _logos;
        private LogoChip _selectedLogo;

        #endregion

        #region Properties

        public bool GetFavoritesButtonEnabled
        {
            get { return _getFavoritesButtonEnabled; }
            set { _getFavoritesButtonEnabled = value; OnPropertyChanged("GetFavoritesButtonEnabled"); }
        }

        public ObservableCollection<TweetViewModel> TweetVMs
        {
            get { return _tweetVMs; }
            set { _tweetVMs = value; OnPropertyChanged("TweetVMs"); }
        }

        public string SelectedTweetText
        {
            get { return _selectedTweetText; }
            set { _selectedTweetText = value; OnPropertyChanged("SelectedTweetText"); }
        }

        public string SelectedTweetUsername
        {
            get { return _selectedTweetUsername; }
            set { _selectedTweetUsername = value; OnPropertyChanged("SelectedTweetUsername"); }
        }

        public TweetViewModel SelectedTweet
        {
            get { return _selectedTweet; }
            set 
            { 
                _selectedTweet = value; 
                OnPropertyChanged("SelectedTweet");

                if (_selectedTweet != null)
                {
                    SelectedTweetText = _selectedTweet.Tweet.Text;
                    SelectedTweetUsername = _selectedTweet.Tweet.User;
                }
                
            }
        }

        public ObservableCollection<LogoChip> Logos
        {
            get { return _logos; }
            set { _logos = value; OnPropertyChanged("Logos"); }
        }

        public LogoChip SelectedLogo
        {
            get { return _selectedLogo; }
            set { _selectedLogo = value; OnPropertyChanged("SelectedLogo"); }
        }

        #endregion

        #region Events

        //public event ShowTweetEventHandler OnShowTweet;

        public event StopCycleEventHandler OnStopCycle;

        #endregion

        #region Constructor

        public TwitterViewModel()
        {
            string consumerKey = ConfigurationManager.AppSettings["ConsumerKey"].ToString();
            string consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"].ToString();
            string token = ConfigurationManager.AppSettings["Token"].ToString();
            string tokenSecret = ConfigurationManager.AppSettings["TokenSecret"].ToString();

            _twitter = new TwitterLib.Twitter(consumerKey, consumerSecret, token, tokenSecret);

            _twitter.GotHomeTimeline += new Twitter.HomeTimelineEventHandler(GotHomeTimeline);
            _twitter.GotFavorites += new Twitter.FavoritesEventHandler(GotFavorites);

            loadLogos();
        }

        #endregion

        #region Private Methods

        private void getHomeTimeline()
        {
            _twitter.GetHomeTimeline(200);
        }

        private void getFavorites()
        {
            GetFavoritesButtonEnabled = false;

            _twitter.GetFavorites(200);
        }

        private void GotHomeTimeline(object sender, TwitterEventArgs e)
        {
            //textBlock1.Text = e.XmlData;
        }

        private void GotFavorites(object sender, TwitterEventArgs e)
        {
            List<Tweet> tweets = ParseTweets(e.XmlData);

            ObservableCollection<TweetViewModel> tweetVMs = new ObservableCollection<TweetViewModel>();

            foreach (Tweet tweet in tweets)
            {
                TweetViewModel tweetVM = new TweetViewModel(tweet);

                tweetVMs.Add(tweetVM);
            }

            TweetVMs = tweetVMs;

            GetFavoritesButtonEnabled = true;
        }

        public List<Tweet> ParseTweets(string xmlData)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            string tweetedAt = "";

            List<Tweet> tweets = new List<Tweet>();

            var isoDateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);

                XmlNodeList statusNodes = xmlDoc.SelectNodes("//status");

                foreach (XmlNode statusNode in statusNodes)
                {
                    days = 0;
                    hours = 0;
                    minutes = 0;
                    seconds = 0;
                    tweetedAt = "";

                    try
                    {
                        Tweet tweet = new Tweet();

                        XmlNode id = statusNode.SelectSingleNode("descendant::id");

                        tweet.TweetID = id.FirstChild.Value.ToString();                         

                        string createdAt = statusNode.SelectSingleNode("descendant::created_at").FirstChild.Value.ToString();
                        DateTime createdAtDate = DateTime.ParseExact(createdAt, "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture);

                        //tweet.CreatedAt = createdAtDate.ToString("yyyy-MM-dd HH:mm");

                        System.DateTime now = DateTime.Now;
                        System.TimeSpan diff1 = now.Subtract(createdAtDate);

                        days = diff1.Days;
                        hours = diff1.Hours;
                        minutes = diff1.Minutes;
                        seconds = diff1.Seconds;

                        if (days > 0)
                        {
                            if (days == 1)
                            {
                                tweetedAt = "1 day ago";
                            }
                            else
                            {
                                tweetedAt = days + " days ago";
                            }
                        }
                        else if (hours > 0)
                        {
                            if (hours == 1)
                            {
                                tweetedAt = "1 hour ago";
                            }
                            else
                            {
                                tweetedAt = hours + " hours ago";
                            }
                        }
                        else if (minutes > 0)
                        {
                            if (minutes == 1)
                            {
                                tweetedAt = "1 minute ago";
                            }
                            else
                            {
                                tweetedAt = minutes + " minutes ago";
                            }
                        }
 
                        tweet.CreatedAt = tweetedAt;

                        using (var client = new WebClient())
                        {
                            string profileImageUrl = statusNode.SelectSingleNode("descendant::profile_image_url").FirstChild.Value.ToString();
                            string fileName = profileImageUrl.Substring(profileImageUrl.LastIndexOf("/") + 1);
                            string localFolder = ConfigurationManager.AppSettings["LocalImageDirectory"].ToString() + "\\Twitter";
                            string localFile = localFolder + "\\" + id.FirstChild.Value.ToString() + "_normal.jpg";

                            if (Directory.Exists(localFolder) == false)
                            {
                                Directory.CreateDirectory(localFolder);
                            }

                            if (File.Exists(localFile) == false)
                            {
                                client.DownloadFile(profileImageUrl, localFile);
                            }

                            if (File.Exists(localFile.Replace("_normal", "_bigger")) == false)
                            {
                                client.DownloadFile(profileImageUrl.Replace("_normal", "_bigger"), localFile.Replace("_normal", "_bigger"));
                            }

                            tweet.Text = statusNode.SelectSingleNode("descendant::text").FirstChild.Value.ToString();
                            tweet.User = statusNode.SelectSingleNode("descendant::user/name").FirstChild.Value.ToString();
                            tweet.ScreenName = statusNode.SelectSingleNode("descendant::user/screen_name").FirstChild.Value.ToString();

                            tweet.UserImage = localFile;
                        }

                        //tweet.TweetType = tweetType;
                        
                        tweets.Add(tweet);
                    }
                    catch (Exception ex)
                    {
                       
                    }
                }

            }
            catch (Exception ex)
            {
             
            }

            return tweets;
        }

        private void showTweet()
        {
            if (_selectedTweetText != null && _selectedTweetText != "")
            {
                if (OnStopCycle != null) OnStopCycle();

                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (DraftAdmin.PlayoutCommands.CommandType)Enum.Parse(typeof(DraftAdmin.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.CommandID = Guid.NewGuid().ToString();
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", ConfigurationManager.AppSettings["TwitterTemplate"].ToString()));

                XmlDataRow xmlRow = new XmlDataRow();

                xmlRow.Add("TITLE_1", _selectedTweetUsername);
                xmlRow.Add("CHIP_1", ConfigurationManager.AppSettings["FranchiseChipDirectory"].ToString() + "\\" + _selectedLogo.FileName);
                xmlRow.Add("TIDBIT_1", _selectedTweetText);
                //xmlRow.Add("SWATCH_1", this.Category.SwatchFile.LocalPath);

                commandToSend.TemplateData = xmlRow.GetXMLString();

                OnSendCommand(commandToSend, null);
            }
        }

        private void loadLogos()
        {
            if (Logos == null)
            {
                Logos = new ObservableCollection<LogoChip>();
            }
            else
            {
                Logos.Clear();
            }

            Logos.Add(new LogoChip("<NONE>", null));

            DirectoryInfo dir = new DirectoryInfo(ConfigurationManager.AppSettings["FranchiseChipDirectory"].ToString());

            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Extension.ToLower() == ".tga")
                {
                    Bitmap bmp = TargaImage.LoadTargaImage(file.FullName);
                    var strm = new System.IO.MemoryStream();
                    bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Bmp);

                    BitmapImage logoBitmap = new BitmapImage();
                    logoBitmap.BeginInit();
                    logoBitmap.StreamSource = strm;
                    logoBitmap.EndInit();

                    Logos.Add(new LogoChip(file.Name, logoBitmap));
                }
            }
        }

        #endregion

        #region Commands

        public ICommand GetFavoritesCommand
        {
            get
            {
                if (_getFavoritesCommand == null)
                {
                    _getFavoritesCommand = new DelegateCommand(getFavorites);
                }
                return _getFavoritesCommand;
            }
        }

        public ICommand ShowTweetCommand
        {
            get
            {
                if (_showTweetCommand == null)
                {
                    _showTweetCommand = new DelegateCommand(showTweet);
                }
                return _showTweetCommand;
            }
        }

        public ICommand RefreshLogosCommand
        {
            get
            {
                if (_refreshLogosCommand == null)
                {
                    _refreshLogosCommand = new DelegateCommand(loadLogos);
                }
                return _refreshLogosCommand;
            }
        }

        #endregion
    }
}
