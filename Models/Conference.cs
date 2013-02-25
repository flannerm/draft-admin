using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using DraftAdmin.Utilities;

namespace DraftAdmin.Models
{
    public class Conference : ModelBase
    {

        #region Private Members

        private Int32 _Id;
        private string _name;
        private string _tricode;
        private Uri _logoTga;
        private Uri _logoPng;
        private ObservableCollection<Tidbit> _tidbits;
        private int _numOfRecruits;
        private List<string> _recruits;
        private bool _isDirty;

        #endregion

        #region Properties

        public Int32 ID
        {
            get { return _Id; }
            set { _Id = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Tricode
        {
            get { return _tricode; }
            set { _tricode = value; }
        }

        public Uri LogoTga
        {
            get { return _logoTga; }
            set
            {
                string logoFilePath = value.LocalPath.ToUpper();

                logoFilePath = logoFilePath.Replace("\\\\HEADSHOT01\\IMAGES", ConfigurationManager.AppSettings["ImsDirectory"].ToString());

                _logoTga = new Uri(logoFilePath);   

                string tgaPath = _logoTga.LocalPath.ToUpper();

                string newFile = tgaPath.Replace("\\\\HEADSHOT01\\IMAGES", ConfigurationManager.AppSettings["LocalImageDirectory"].ToString());
                newFile = newFile.Replace(".TGA", ".png");

                string newFolder = newFile.Substring(0, newFile.LastIndexOf("\\"));

                DirectoryInfo dir = new DirectoryInfo(newFolder);

                if (dir.Exists == false)
                {
                    dir.Create();
                }

                _logoPng = new Uri(newFile);

                //BitmapImage bitmapImage = null;
                Bitmap bitmap = null;

                FileInfo tgaFile = new FileInfo(_logoTga.LocalPath);
                FileInfo pngFile = new FileInfo(newFile);

                if (pngFile.Exists == false)
                {
                    if (tgaFile.Exists)
                    {
                        if (pngFile.Exists == false || (tgaFile.LastWriteTime > pngFile.LastWriteTime))
                        {
                            if (tgaFile.Exists)
                            {
                                try
                                {
                                    bitmap = TargaImage.LoadTargaImage(_logoTga.LocalPath);
                                    bitmap.Save(newFile, System.Drawing.Imaging.ImageFormat.Png);
                                }
                                finally
                                {

                                }                                
                            }
                        }
                    }
                }
                //bitmapImage = BitmapToBitmapImage.Convert(bitmap);                    

                //_logoBitmap = bitmapImage;
            }
        }

        public Uri LogoPng
        {
            get { return _logoPng; }
            set { _logoPng = value; }
        }

        public ObservableCollection<Tidbit> Tidbits
        {
            get { return _tidbits; }
            set { _tidbits = value; }
        }

        public int NumOfRecruits
        {
            get { return _numOfRecruits; }
            set { _numOfRecruits = value; }
        }

        public List<string> Recruits
        {
            get { return _recruits; }
            set { _recruits = value; }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        #endregion

    }
}
