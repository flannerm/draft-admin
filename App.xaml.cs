using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace DraftAdmin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        Views.MainView _view;

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            // define application exception handler
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(currentDispatcherUnhandledException);

            // Create the ViewModel and expose it using the View's DataContext
            _view = new Views.MainView();
            _view.DataContext = new ViewModels.MainViewModel();
            _view.Show();
        }


        void currentDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show("The following error occurred: " + e.Exception.Message, "NFL Draft",
                            MessageBoxButton.OK, MessageBoxImage.Error);

            //Close DBConnection
            //DbConnection.CloseDatabase();

            e.Handled = true;

        }

    }
}
