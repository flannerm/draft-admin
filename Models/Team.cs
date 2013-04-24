using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using DraftAdmin.Utilities;
using System.Windows.Interop;
using System.Windows;
using System.Configuration;
using System.Collections.ObjectModel;

namespace DraftAdmin.Models
{
    public class Team : ModelBase
    {

        #region Private Members

        private Int32 _Id;
        private int _rank;
        private string _fullName;
        private string _tricode;
        private string _city;
        private string _name;
        private Uri _logoTga;
        private Uri _logoTgaNoKey;
        private Uri _logoPng;
        //private BitmapImage _logoBitmap;
        private Uri _swatchTga;
        //private Uri _swatchPng;
        private Uri _pickPlateTga;
        private string _league;
        private Conference _conference;
        private List<Tidbit> _tidbits;
        private string _overallRecord;
        private string _confRecord;
        private int _lotteryPctRank;
        private int _lotteryOrder;
        private bool _isDirty;
        private string _hashtag;

        #endregion

        #region Properties

        public Int32 ID
        {
            get { return _Id; }
            set { _Id = value; }
        }

        public int Rank
        {
            get { return _rank; }
            set { _rank = value; }
        }

        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; }
        }

        public string Tricode
        {
            get { return _tricode; }
            set { _tricode = value; }
        }

        public string City
        {
            get { return _city; }
            set { _city = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string OverallRecord
        {
            get { return _overallRecord; }
            set { _overallRecord = value; }
        }

        public string ConferenceRecord
        {
            get { return _confRecord; }
            set { _confRecord = value; }
        }

        public int LotteryPctRank
        {
            get { return _lotteryPctRank; }
            set { _lotteryPctRank = value; }
        }

        public int LotteryOrder
        {
            get { return _lotteryOrder; }
            set { _lotteryOrder = value; }
        }

        public Uri LogoTga
        {
            get { return _logoTga; }
            set
            {
                string logoFilePath = value.LocalPath.ToUpper();

                logoFilePath = logoFilePath.Replace("\\\\HEADSHOT01\\IMAGES", ConfigurationManager.AppSettings["ImsDirectory"].ToString());

                _logoTga = new Uri(logoFilePath);

                FileInfo tgaFile = new FileInfo(_logoTga.LocalPath);

                if (tgaFile.Exists == false)
                {
                    switch (_league)
                    {
                        case "NCAAF":
                        case "NCF23":
                            _logoTga = new Uri("\\\\HEADSHOT01\\IMAGES\\IMS_IMAGES\\SD\\LOGOS\\FOOTBALL\\COLLEGE\\DIVISION_1\\NCAA_LOGO_256.TGA");
                            break;
                        case "NFL":
                            _logoTga = new Uri("\\\\HEADSHOT01\\IMAGES\\IMS_IMAGES\\SD\\LOGOS\\FOOTBALL\\NFL\\NFL_SHEILD_256.TGA");
                            break;
                        default:
                            _logoTga = new Uri("\\\\HEADSHOT01\\IMAGES\\IMS_IMAGES\\SD\\LOGOS_NO_KEY\\FLAGS\\COUNTRIES\\COUNTRY\\UNITED_STATES_256.TGA");
                            break;
                    }                    
                }

                _logoTgaNoKey = new Uri(_logoTga.LocalPath.ToUpper().Replace("LOGOS", "LOGOS_NO_KEY"));

                tgaFile = new FileInfo(_logoTgaNoKey.LocalPath);

                if (tgaFile.Exists == false)
                {
                    _logoTgaNoKey = _logoTga;
                }

                _logoPng = new Uri(createPngFile(_logoTgaNoKey.LocalPath));                
            }
        }

        public Uri LogoTgaNoKey
        {
            get { return _logoTgaNoKey; }
            set { _logoTgaNoKey = value; }
        }

        public Uri LogoPng
        {
            get { return _logoPng; }
            set { _logoPng = value; }
        }

        public Uri SwatchTga
        {
            get { return _swatchTga; }
            set
            {
                string logoFilePath = value.LocalPath.ToUpper();

                logoFilePath = logoFilePath.Replace("\\\\HEADSHOT01\\IMAGES", ConfigurationManager.AppSettings["ImsDirectory"].ToString());

                _swatchTga = new Uri(logoFilePath);   
            }
        }

        public Uri PickPlateTga
        {
            get { return _pickPlateTga; }
            set { _pickPlateTga = value; }
        }

        public Conference Conference
        {
            get { return _conference; }
            set { _conference = value; }
        }

        public List<Tidbit> Tidbits
        {
            get { return _tidbits; }
            set { _tidbits = value; }
        }

        public string League
        {
            get { return _league; }
            set { _league = value; }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        public string Hashtag
        {
            get { return _hashtag; }
            set { _hashtag = value; }
        }

        #endregion

        #region Private Methods

        private string createPngFile(string tgaPath)
        {
            string newFile = tgaPath.ToUpper().Replace(ConfigurationManager.AppSettings["ImsDirectory"].ToString().ToUpper(), ConfigurationManager.AppSettings["LocalImageDirectory"].ToString().ToUpper());

            try
            {
                newFile = newFile.Replace(".TGA", ".png");

                FileInfo tgaFile = new FileInfo(tgaPath);
                FileInfo pngFile = new FileInfo(newFile);

                if (tgaFile.Exists)
                {
                    if (pngFile.Exists == false || tgaFile.CreationTime > pngFile.CreationTime)
                    {
                        try
                        {
                            string newFolder = newFile.Substring(0, newFile.LastIndexOf("\\"));

                            DirectoryInfo dir = new DirectoryInfo(newFolder);

                            if (dir.Exists == false)
                            {
                                dir.Create();
                            }

                            Bitmap bitmap = null;

                            bitmap = TargaImage.LoadTargaImage(_logoTgaNoKey.LocalPath);
                            bitmap.Save(newFile, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        finally
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            { }

            return newFile;
        }

        #endregion
    }
}
