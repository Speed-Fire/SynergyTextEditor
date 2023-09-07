using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SynergyTextEditor.Classes;
using SynergyTextEditor.Classes.Blockers;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;



namespace SynergyTextEditor.ViewModels
{
    public class MainVM :
        BaseViewModel,
        IRecipient<FileChangedMessage>
    {
        private readonly TextEditor textEditor;
        private readonly IKeywordLanguageSelector languageSelector;
        private readonly AppThemeController appThemeController;

        private string programTitle = "SynergyPad - New";
        public string ProgramTitle
        {
            get
            {
                return programTitle;
            }
            set
            {
                programTitle = value;
                OnPropertyChanged(nameof(ProgramTitle));
            }
        }

        private string openedFile = "";
        public string OpenedFile
        {
            get
            {
                return openedFile;
            }
            set
            {
                openedFile = value;
                OnPropertyChanged(nameof(OpenedFile));
            }
        }

        private bool isFileSaved = false;
        public bool IsFileSaved
        {
            get
            {
                return isFileSaved;
            }
            set
            {
                isFileSaved = value;
                OnPropertyChanged(nameof(IsFileSaved));
            }
        }

        private bool IsFileOpen => !string.IsNullOrWhiteSpace(OpenedFile);

        public MainVM(FlowDocument document)
        {
            languageSelector = Program.AppHost.Services.GetRequiredService<IKeywordLanguageSelector>();
            appThemeController = Program.AppHost.Services.GetRequiredService<AppThemeController>();

            textEditor = new TextEditor(document);

            WeakReferenceMessenger.Default.Register(this);

            RegisterMessages();
            RegisterCommandBindings();

            App.Current.Exit += (sender, e) =>
            {
                RequestSaving(sender, null);
            };

            PropertyChanged += MainVM_PropertyChanged;
        }

        #region BaseViewModel

        protected override void RegisterMessages()
        {
            WeakReferenceMessenger.Default.Register<OpenedFileNameRequestMessage>(this, (r, m) =>
            {
                m.Reply(OpenedFile);
            });

            WeakReferenceMessenger.Default.Register<SelectSyntaxCommandRequestMessage>(this, (r, m) =>
            {
                m.Reply(SelectSyntax);
            });

            WeakReferenceMessenger.Default.Register<SetThemeCommandRequestMessage>(this, (r, m) =>
            {
                m.Reply(SetTheme);
            });
        }

        protected override void RegisterCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenFile));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, CreateFile));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveFile));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAsFile));
        }

        #endregion

        #region Application commands handlers

        private void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            RequestSaving(sender, e);

            OpenFileDialog ofd = new OpenFileDialog()
            {
                Multiselect = false
            };
            ofd.Filter = "RichText Files (*.rtf)|*.rtf|XAML/XML files (*.xml; *.xaml)|*.xml;*.xaml|Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                WeakReferenceMessenger.Default.Send(new BlockTextEditorChangeStateMessage(true));

                textEditor.Open(ofd.FileName);

                WeakReferenceMessenger.Default.Send(new FileOpenedMessage(ofd.FileName));

                WeakReferenceMessenger.Default.Send(new BlockTextEditorChangeStateMessage(false));

                isFileSaved = true;
                OpenedFile = ofd.FileName;
            }
        }

        private void CreateFile(object sender, ExecutedRoutedEventArgs e)
        {
            RequestSaving(sender, e);

            isFileSaved = false;
            OpenedFile = "";

            textEditor.Create(true);
        }

        private void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            if (!IsFileOpen)
                SaveAsFile(sender, e);
            else
            {
                IsFileSaved = true;
                textEditor.Save(OpenedFile);
            }
        }

        private void SaveAsFile(object sender, ExecutedRoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|XAML Files (*.xaml)|*.xaml|All files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
                IsFileSaved = true;
                textEditor.Save(sfd.FileName);
            }
        }

        #endregion

        #region Commands

        private RelayCommand<string> setTheme;
        public RelayCommand<string> SetTheme => setTheme ??
            (setTheme = new RelayCommand<string>(theme =>
            {
                appThemeController.SetTheme(theme);
            }));

        private RelayCommand uploadSyntax;
        public RelayCommand UploadSyntax => uploadSyntax ??
            (uploadSyntax = new RelayCommand(() =>
            {
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Multiselect = false
                };
                ofd.Filter = "XAML/XML files (*.xml; *.xaml)|*.xml;*.xaml|Text files (*.txt)|*.txt";

                if(ofd.ShowDialog() == true)
                {
                    Exception ex;

                    if(languageSelector.UploadLanguage(ofd.FileName, out ex))
                    {
                        MessageBox.Show("Syntax is successful uploaded.",
                            "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to upload syntax:\n{ex.Message}.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }));

        private RelayCommand<string> selectSyntax;
        public RelayCommand<string> SelectSyntax => selectSyntax ??
            (selectSyntax = new RelayCommand<string>(langName =>
            {
                WeakReferenceMessenger.Default.Send(new SelectKeywordLanguageMessage(langName));
            }));

        private RelayCommand<bool> enableBracketBlockHighlighting;
        public RelayCommand<bool> EnableBracketBlockHighlighting => enableBracketBlockHighlighting ??
            (enableBracketBlockHighlighting = new RelayCommand<bool>(isEnable =>
            {
                WeakReferenceMessenger.Default.Send(new EnableBracketBlockHighlightingMessage(isEnable));
            }));

        #endregion

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

        private void MainVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(OpenedFile):
                    {
                        var programName = "SynergyPad - ";

                        var fileName = string.IsNullOrEmpty(OpenedFile) ? "New" : Path.GetFileName(OpenedFile);

                        var end = IsFileSaved ? "" : "*";

                        ProgramTitle = programName + fileName + end;
                    }
                    break;
                case nameof(IsFileSaved):
                    {
                        if(IsFileSaved && ProgramTitle.EndsWith('*'))
                        {
                            ProgramTitle = ProgramTitle[..^1]; // .Substring(0, ProgramTitle.Length - 1)
                        }
                        else if(!IsFileSaved && !ProgramTitle.EndsWith('*'))
                        {
                            ProgramTitle += "*";
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void Receive(FileChangedMessage message)
        {
            IsFileSaved = false;
        }
    }
}
