using Microsoft.Win32;
using SynergyTextEditor.Classes;
using SynergyTextEditor.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

#nullable disable

namespace SynergyTextEditor.ViewModels
{
    public class MainVM : BaseViewModel
    {
        private readonly TextEditor textEditor;

        private event Action<TextChangedEventArgs> TextChanged;

        private string OpenedFile { get; set; } = "";

        private bool IsFileOpen => !string.IsNullOrWhiteSpace(OpenedFile);

        public MainVM(FlowDocument document)
        {
            textEditor = new TextEditor(document);

            TextChanged += textEditor.OnTextChanged;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenFile));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, CreateFile));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveFile));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAsFile));

            App.Current.Exit += (sender, e) =>
            {
                RequestSaving(sender, null);
            };
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            RequestSaving(sender, e);

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "RichText Files (*.rtf)|*.rtf|XAML/XML files (*.xml/*.xaml)|*.xml,*.xaml|Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                TextChanged -= textEditor.OnTextChanged;

                textEditor.Open(ofd.FileName);

                TextChanged += textEditor.OnTextChanged;
            }
        }

        private void CreateFile(object sender, ExecutedRoutedEventArgs e)
        {
            RequestSaving(sender, e);

            textEditor.Create();
        }

        private void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            if (!IsFileOpen)
                SaveAsFile(sender, e);
            else
                textEditor.Save(OpenedFile);
        }


        private void SaveAsFile(object sender, ExecutedRoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|XAML Files (*.xaml)|*.xaml|All files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
                textEditor.Save(sfd.FileName);
            }
        }

        public void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(e);
        }

        private void RequestSaving(object sender, ExecutedRoutedEventArgs e)
        {
            if (textEditor.IsTextChanged)
            {
                var res = MessageBox.Show("Current file is not saved. Do you want to save it?",
                    "Attention",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (res == MessageBoxResult.Yes)
                    SaveFile(sender, e);
            }
        }
    }
}
