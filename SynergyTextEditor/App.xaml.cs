using Microsoft.Extensions.DependencyInjection;
using SynergyTextEditor.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SynergyTextEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //AppThemeController.Instance.SetTheme("dark");

            var themeController = Program.AppHost.Services.GetRequiredService<AppThemeController>();
            themeController.SetTheme("dark");

            MainWindow = Program.AppHost.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }
    }
}
