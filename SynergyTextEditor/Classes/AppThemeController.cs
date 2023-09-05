using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace SynergyTextEditor.Classes
{
    public class AppThemeController
    {
        private List<string> _themes = new() { "Light", "Dark" };
        public IEnumerable<string> Themes => _themes;

        public AppThemeController()
        {
            
        }

        private ResourceDictionary CurrentTheme { get; set; }

        private string currentThemeName;
        public string CurrentThemeName
        {
            get
            {
                return currentThemeName;
            }
            private set
            {
                currentThemeName = value;
            }
        }

        public void SetTheme(string themeName)
        {
            if(CurrentThemeName == themeName) return;

            if (CurrentTheme != null)
                App.Current.Resources.MergedDictionaries.Remove(CurrentTheme);

            CurrentTheme = new ResourceDictionary();

            switch (themeName)
            {
                case "Light":
                    {
                        CurrentTheme.Source = new Uri("pack://application:,,,/Themes/LightTheme.xaml");
                        CurrentThemeName = "Light";
                    }
                    break;

                case "Dark":
                default:
                    {
                        CurrentTheme.Source = new Uri("pack://application:,,,/Themes/DarkTheme.xaml");
                        CurrentThemeName = "Dark";
                    }
                    break;
            }

            App.Current.Resources.MergedDictionaries.Add(CurrentTheme);

            WeakReferenceMessenger.Default.Send(new ThemeChangedMessage(themeName));
        }
    }
}
