using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Configuration;
using DraftAdmin.ViewModels;
using DraftAdmin.Output;
using DraftAdmin.DataAccess;
using DraftAdmin.Sockets;

namespace DraftAdmin.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {

        public MainView()
        {
            InitializeComponent();
   
            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            string fullName = assembly.FullName;

            string[] fullNameElements = fullName.Split(',');

            if (fullNameElements.Length > 1)
            {
                string[] versionElements = fullNameElements[1].Split('=');

                if (versionElements.Length > 1)
                {
                    int eqLoc;
                    int scLoc;

                    string sdrDbConn = ConfigurationManager.ConnectionStrings["SDRDbConn"].ConnectionString;
                    string mySqlDbConn = ConfigurationManager.ConnectionStrings["MySqlDbConn"].ConnectionString;

                    eqLoc = sdrDbConn.LastIndexOf("=");
                    //scLoc = sdrDbConn.IndexOf(";") - 1;
                    string sdrServer = sdrDbConn.Substring(eqLoc + 1);

                    eqLoc = mySqlDbConn.IndexOf("=");
                    scLoc = mySqlDbConn.IndexOf(";") - 1;
                    string mySqlServer = mySqlDbConn.Substring(eqLoc + 1, scLoc - eqLoc);

                    this.Title = "Draft Compression Admin - " + versionElements[1].ToString() + " - SDR Database:  " + sdrServer + ", MySQL Database:  " + mySqlServer;
                }
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void axClockCtl_ClockChange(object sender, AxClockControl.__ClockCtl_ClockChangeEvent e)
        {
            string clockStr = e.sClock.ToString();

            MainViewModel mainVM = (MainViewModel)this.DataContext;

            AxClockControl.AxClockCtl clockCtl = (AxClockControl.AxClockCtl)sender;

            mainVM.ClockSeconds = clockCtl.Seconds;

            mainVM.Clock = clockStr;
        }

    }
}
