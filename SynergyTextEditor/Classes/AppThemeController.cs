using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

#nullable disable

namespace SynergyTextEditor.Classes
{
    public class AppThemeController : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private AppThemeController() { }

        private static AppThemeController instance;
        public static AppThemeController Instance
        {
            get
            {
                if(instance == null)
                    instance = new AppThemeController();

                return instance;
            }
        }

        private ResourceDictionary CurrentTheme { get; set; }

        private string currentThemeName;
        public string CurrentThemeName
        {
            get
            {
                return currentThemeName;
            }
            set
            {
                currentThemeName = value;
                OnPropertyChanged(nameof(CurrentThemeName));
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
                case "light":
                    {
                        CurrentTheme.Source = new Uri("pack://application:,,,/Themes/LightTheme.xaml");
                        CurrentThemeName = "light";
                    }
                    break;

                case "dark":
                default:
                    {
                        CurrentTheme.Source = new Uri("pack://application:,,,/Themes/DarkTheme.xaml");
                        CurrentThemeName = "dark";
                    }
                    break;
            }

            App.Current.Resources.MergedDictionaries.Add(CurrentTheme);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
