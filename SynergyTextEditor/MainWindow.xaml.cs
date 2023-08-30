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

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new MainVM(Editor.Document);
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            viewModel.TextChanged();
        }
    }
}
