using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Labb3_Databaser_NET22.Stores;
using Labb3_Databaser_NET22.ViewModels;

namespace Labb3_Databaser_NET22
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NavigationStore _navigationStore;
        private readonly DataStore _dataStore;
        public App()
        {
            _navigationStore = new NavigationStore();
            _dataStore = new DataStore();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore);

            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(_navigationStore)
            };

            MainWindow.Show();
        }
    }
}
