using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace DraftAdmin.Models
{
    public class LogoChip
    {
        public string FileName { get; set; }
        public BitmapImage Image { get; set; }

        public LogoChip(string fileName, BitmapImage image)
        {
            FileName = fileName;
            Image = image;
        }
    }
}
