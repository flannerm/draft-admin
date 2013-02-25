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
        private ObservableCollection<Tidbit> _tidbits;
        private string _overallRecord;
        private string _confRecord;
        private int _lotteryPctRank;
        private int _lotteryOrder;
        private bool _isDirty;

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

                _logoTgaNoKey = new Uri(_logoTga.LocalPath.Replace("LOGOS", "LOGOS_NO_KEY"));

                //if (File.Exists(_logoTga.LocalPath))
                //{
                //    Bitmap bmp = TargaImage.LoadTargaImage(_logoTga.LocalPath);
                //    var strm = new System.IO.MemoryStream();
                //    bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Bmp);

                //    _logoBitmap = new BitmapImage();
                //    _logoBitmap.BeginInit();
                //    _logoBitmap.StreamSource = strm;
                //    _logoBitmap.EndInit();

                //    //_logoBitmap = img;
                //}
                //else
                //{
                //    _logoBitmap = null;
                //}

                string tgaPath = _logoTgaNoKey.LocalPath.ToUpper();

                string newFile = tgaPath.Replace(ConfigurationManager.AppSettings["ImsDirectory"].ToString().ToUpper(), ConfigurationManager.AppSettings["LocalImageDirectory"].ToString().ToUpper());

                newFile = newFile.Replace(".TGA", ".png");

                string newFolder = newFile.Substring(0, newFile.LastIndexOf("\\"));

                DirectoryInfo dir = new DirectoryInfo(newFolder);

                if (dir.Exists == false)
                {
                    dir.Create();
                }

                Bitmap bitmap = null;

                FileInfo tgaFile = new FileInfo(_logoTgaNoKey.LocalPath);
                FileInfo pngFile = new FileInfo(newFile);

                if (pngFile.Exists == false)
                {
                    if (tgaFile.Exists)
                    {
                        if (tgaFile.LastWriteTime > pngFile.LastWriteTime)
                        {                           
                            try
                            {
                                bitmap = TargaImage.LoadTargaImage(_logoTgaNoKey.LocalPath);
                                bitmap.Save(newFile, System.Drawing.Imaging.ImageFormat.Png);
                            }
                            finally
                            {

                            }
                            
                        }
                    }
                }

                FileInfo newFileInfo = new FileInfo(newFile);

                if (newFileInfo.Exists)
                {
                    _logoPng = new Uri(newFile);
                }
                else
                {
                    _logoPng = new Uri(ConfigurationManager.AppSettings["LocalImageDirectory"].ToString() + "\\IMS_IMAGES\\SD\\LOGOS_NO_KEY\\FLAGS\\COUNTRIES\\COUNTRY\\UNITED_STATES_256.png");
                }

                //bitmapImage = BitmapToBitmapImage.Convert(bitmap);                    

                //_logoBitmap = bitmapImage;
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

        //public BitmapImage LogoBitmap
        //{
        //    get 
        //    {
        //        //BitmapImage logo = (BitmapImage)Global.GlobalCollections.Instance.Logos.SingleOrDefault(l => l.Key == _Id).Value;
        //        return null;             
        //    }
        //    //set { _logoBitmap = value; OnPropertyChanged("LogoBitmap"); }
        //}

        public Uri SwatchTga
        {
            get { return _swatchTga; }
            set
            {
                string logoFilePath = value.LocalPath.ToUpper();

                logoFilePath = logoFilePath.Replace("\\\\HEADSHOT01\\IMAGES", ConfigurationManager.AppSettings["ImsDirectory"].ToString());

                _swatchTga = new Uri(logoFilePath);   

                //string tgaPath = _swatchTga.LocalPath.ToUpper();

                //string newFile = tgaPath.Replace(ConfigurationManager.AppSettings["ImsDirectory"].ToString().ToUpper(), ConfigurationManager.AppSettings["LocalImageDirectory"].ToString().ToUpper());

                //newFile = newFile.Replace(".TGA", ".png");

                //string newFolder = newFile.Substring(0, newFile.LastIndexOf("\\"));

                //DirectoryInfo dir = new DirectoryInfo(newFolder);

                //if (dir.Exists == false)
                //{
                //    dir.Create();
                //}

                //_swatchPng = new Uri(newFile);

                //Bitmap bitmap = null;

                //FileInfo tgaFile = new FileInfo(_swatchTga.LocalPath);
                //FileInfo pngFile = new FileInfo(newFile);

                //if (pngFile.Exists == false)
                //{
                //    if (tgaFile.Exists)
                //    {
                //        if (pngFile.Exists == false || (tgaFile.LastWriteTime > pngFile.LastWriteTime))
                //        {
                //            if (tgaFile.Exists)
                //            {
                //                try
                //                {
                //                    bitmap = TargaImage.LoadTargaImage(_swatchTga.LocalPath);
                //                    bitmap.Save(newFile, System.Drawing.Imaging.ImageFormat.Png);
                //                }
                //                finally
                //                {

                //                }                                
                //            }
                //        }
                //    }
                //}
            }
        }

        //public Uri SwatchPng
        //{
        //    get { return _swatchPng; }
        //    set { _swatchPng = value; }
        //}

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

        public ObservableCollection<Tidbit> Tidbits
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

        #endregion

    }
}
