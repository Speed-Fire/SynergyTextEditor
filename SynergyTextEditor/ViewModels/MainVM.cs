using Microsoft.Win32;
using SynergyTextEditor.Classes;
using SynergyTextEditor.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

#nullable disable

namespace SynergyTextEditor.ViewModels
{
    public class MainVM : BaseViewModel
    {
        private readonly TextEditor textEditor;

        private string OpenedFile { get; set; } = "";

        private bool IsFileOpen => !string.IsNullOrWhiteSpace(OpenedFile);

        public MainVM(FlowDocument document)
        {
            textEditor = new TextEditor(document);
        }

        private RelayCommand openFile;
        public RelayCommand OpenFile => openFile ??
            (openFile = new RelayCommand(obj =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "RichText Files (*.rtf)|*.rtf|XAML/XML files (*.xml/*.xaml)|*.xml,*.xaml|Text files (*.txt)|*.txt|All files (*.*)|*.*";

                if(ofd.ShowDialog() == true)
                {
                    textEditor.Open(ofd.FileName);
                }
            }));

        private RelayCommand createFile;
        public RelayCommand CreateFile => createFile ??
            (createFile = new RelayCommand(obj =>
            {
                if (textEditor.TextChanged)
                {
                    var res = MessageBox.Show("Current file is not saved. Do you want to save it?",
                        "Attention",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes)
                        SaveFile.Execute(null);
                }

                textEditor.Create();
            }));

        private RelayCommand saveFile;
        public RelayCommand SaveFile => saveFile ??
            (saveFile = new RelayCommand(obj =>
            {
                if (!IsFileOpen)
                    SaveAsFile.Execute(null);
                else
                    textEditor.Save(OpenedFile);
            }));


        private RelayCommand saveAsFile;
        public RelayCommand SaveAsFile => saveAsFile ??
            (saveAsFile = new RelayCommand(obj =>
            {
                var sfd = new SaveFileDialog();
                sfd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|XAML Files (*.xaml)|*.xaml|All files (*.*)|*.*";
                if (sfd.ShowDialog() == true)
                {
                    textEditor.Save(sfd.FileName);
                }
            }));

        public void TextChanged()
        {
            textEditor.TextChanged = true;
        }
    }
}
