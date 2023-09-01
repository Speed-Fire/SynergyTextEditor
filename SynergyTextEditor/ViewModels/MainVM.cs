﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SynergyTextEditor.Classes;
using SynergyTextEditor.Messages;
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
        private readonly IKeywordLanguageSelector languageSelector;

        private string OpenedFile { get; set; } = "";

        private bool IsFileOpen => !string.IsNullOrWhiteSpace(OpenedFile);

        public MainVM(FlowDocument document)
        {
            languageSelector = Program.AppHost.Services.GetRequiredService<IKeywordLanguageSelector>();

            textEditor = new TextEditor(document);

            //WeakReferenceMessenger.Default.Register<TextChangedMessage>(textEditor, (r, m) =>
            //{
            //    textEditor.OnTextChanged(m.Value);
            //});

            WeakReferenceMessenger.Default.Register<OpenedFileNameRequestMessage>(this, (r, m) =>
            {
                m.Reply(OpenedFile);
            });

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenFile));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, CreateFile));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveFile));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAsFile));

            App.Current.Exit += (sender, e) =>
            {
                RequestSaving(sender, null);
            };
        }

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
                //WeakReferenceMessenger.Default.Unregister<TextChangedMessage>(textEditor);
                WeakReferenceMessenger.Default.Send(new BlockTextEditorChangeStateMessage(true));

                textEditor.Open(ofd.FileName);

                WeakReferenceMessenger.Default.Send(new FileOpenedMessage(ofd.FileName));

                WeakReferenceMessenger.Default.Send(new BlockTextEditorChangeStateMessage(false));
                //WeakReferenceMessenger.Default.Register<TextChangedMessage>(textEditor, (r, m) =>
                //{
                //    textEditor.OnTextChanged(m.Value);
                //});

                OpenedFile = ofd.FileName;
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

        #endregion

        #region Commands

        private RelayCommand<string> setTheme;
        public RelayCommand<string> SetTheme => setTheme ??
            (setTheme = new RelayCommand<string>(theme =>
            {
                AppThemeController.Instance.SetTheme(theme as string);
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
    }
}
