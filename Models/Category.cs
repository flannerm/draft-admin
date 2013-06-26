using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using DraftAdmin.Utilities;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Collections.ObjectModel;

namespace DraftAdmin.Models
{
    public class Category : ModelBase
    {

        #region Private Members

        private Int32 _Id;
        private string _fullName;
        private string _tricode;
        private Uri _logoTga;
        private Uri _logoPng;
        private BitmapImage _logoBitmap;
        private Uri _swatchFile;
        private List<Tidbit> _tidbits;
        private string _template;
        private bool _isDirty;

        #endregion

        #region Properties

        public Int32 ID
        {
            get { return _Id; }
            set { _Id = value; }
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

        public Uri LogoTga
        {
            get { return _logoTga; }
            set
            {
                string logoFilePath = value.LocalPath.ToUpper();

                logoFilePath = logoFilePath.Replace("\\\\HEADSHOT01\\IMAGES", ConfigurationManager.AppSettings["ImsDirectory"].ToString());

                _logoTga = new Uri(logoFilePath);

                if (File.Exists(_logoTga.LocalPath))
                {
                    Bitmap bmp = TargaImage.LoadTargaImage(_logoTga.LocalPath);
                    var strm = new System.IO.MemoryStream();
                    bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Bmp);

                    _logoBitmap = new BitmapImage();
                    _logoBitmap.BeginInit();
                    _logoBitmap.StreamSource = strm;
                    _logoBitmap.EndInit();
                }
                else
                {
                    _logoBitmap = null;
                }

                //string tgaPath = _logoTga.LocalPath.ToUpper();
                //string tgaFileName = tgaPath.Substring(tgaPath.LastIndexOf("\\") + 1);
                //string newFile = "";

                //if (tgaPath.IndexOf("HEADSHOT01\\IMAGES") > -1)
                //{
                //    newFile = tgaPath.Replace("\\\\HEADSHOT01\\IMAGES", ConfigurationManager.AppSettings["LocalImageDirectory"].ToString());
                //}
                //else
                //{
                //    newFile = ConfigurationManager.AppSettings["LocalImageDirectory"].ToString() + "\\" + tgaFileName.ToUpper().Replace(".TGA", ".png");
                //}
    
                //string newFolder = newFile.Substring(0, newFile.LastIndexOf("\\"));

                //DirectoryInfo dir = new DirectoryInfo(newFolder);

                //if (dir.Exists == false)
                //{
                //    dir.Create();
                //}

                //_logoPng = new Uri(newFile);

                //Bitmap bitmap = null;

                //FileInfo tgaFile = new FileInfo(_logoTga.LocalPath);
                //FileInfo pngFile = new FileInfo(newFile);

                //if (pngFile.Exists == false)
                //{
                //    if (tgaFile.Exists)
                //    {
                //        if (pngFile.Exists == false || (tgaFile.LastWriteTime > pngFile.LastWriteTime))
                //        {
                //            try
                //            {
                //                bitmap = TargaImage.LoadTargaImage(_logoTga.LocalPath);
                //                bitmap.Save(newFile, System.Drawing.Imaging.ImageFormat.Png);
                //            }
                //            finally
                //            {

                //            }
                //        }
                //    }
                //}

                //bitmapImage = BitmapToBitmapImage.Convert(bitmap);                    

                //_logoBitmap = bitmapImage;
            }
        }

        public Uri LogoPng
        {
            get { return _logoPng; }
            set { _logoPng = value; }
        }


        public BitmapImage LogoBitmap
        {
            get { return _logoBitmap; }
            set { _logoBitmap = value; }
        }

        public Uri SwatchFile
        {
            get { return _swatchFile; }
            set { _swatchFile = value; }
        }

        public List<Tidbit> Tidbits
        {
            get { return _tidbits; }
            set { _tidbits = value; }
        }

        public string Template
        {
            get { return _template; }
            set { _template = value; }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        #endregion


    }
}
