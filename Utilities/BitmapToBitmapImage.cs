using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Interop;
using System.Windows;

namespace DraftAdmin.Utilities
{
    public class BitmapToBitmapImage
    {

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapImage Convert(Bitmap image)
        {
            BitmapImage bitmapImage = null;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            IntPtr hBitmap = image.GetHbitmap();

            try
            {
                
                BitmapSource imageSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                                                                    IntPtr.Zero,
                                                                    Int32Rect.Empty,
                                                                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                bitmapImage = new BitmapImage();

                encoder.Frames.Add(BitmapFrame.Create(imageSource));
                encoder.Save(memoryStream);

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
                bitmapImage.EndInit();

                memoryStream.Close();
            }
            catch (Exception ex) 
            { 
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return bitmapImage;
        }


    }
}
