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
using DraftAdmin.ViewModels;

namespace DraftAdmin.Views
{
    /// <summary>
    /// Interaction logic for PlayersTabView.xaml
    /// </summary>
    public partial class PlayerTabView : UserControl
    {
        public PlayerTabView()
        {
            InitializeComponent();
        }

        private void listPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //PlayerViewModelBase player = (PlayerViewModelBase)listPlayers.SelectedItem;
        }
    }
}
