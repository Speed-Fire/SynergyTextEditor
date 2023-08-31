using SynergyTextEditor.Classes;
using SynergyTextEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SynergyTextEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainVM viewModel;

        private TextHighlighter textHighlighter;

        public MainWindow()
        {
            InitializeComponent();

            switch (AppThemeController.Instance.CurrentThemeName)
            {
                case "light":
                    LightThemeSetting.IsChecked = true;
                    break;
                case "dark":
                    DarkThemeSetting.IsChecked = true;
                    break;
            }

            var vm = new MainVM(Editor.Document);

            Editor.TextChanged += vm.OnTextChanged;

            DataContext = viewModel = vm;
            CommandBindings.AddRange(viewModel.CommandBindings);

            textHighlighter = new TextHighlighter(Editor);     
        }

        private void MenuSettingsThemeLight_Click(object sender, RoutedEventArgs e)
        {
            LightThemeSetting.IsChecked = true;
        }

        private void MenuSettingsThemeDark_Click(object sender, RoutedEventArgs e)
        {
            DarkThemeSetting.IsChecked = true;
        }
    }
}
