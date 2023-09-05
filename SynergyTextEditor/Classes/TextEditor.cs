using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using SynergyTextEditor.Classes.SavingStrategies;
using SynergyTextEditor.Classes.UIControls;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;



namespace SynergyTextEditor.Classes
{
    public class TextEditor :
        INotifyPropertyChanged,
        IRecipient<FileChangedMessage>,
        IRecipient<BlockTextEditorChangeStateMessage>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<string, ISavingStrategy> SavingStrategies = new();

        public bool IsTextChanged { get; private set; } = false;

        private volatile bool isTextChangedBlocked;
        private bool IsTextChangedBlocked { get => isTextChangedBlocked; 
            set
            {
                isTextChangedBlocked = value;
            }
        }


        private readonly FlowDocument document;

        public TextEditor(FlowDocument document)
        {
            InitSavingStrategies();
            this.document = document;

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public TextEditor(FlowDocument document, string path)
            :
            this(document)
        {
            Open(path);
        }

        public void Save(string path)
        {
            var strategy = GetSavingStrategy(path);

            strategy.Save(document, path);

            IsTextChanged = false;
        }

        public void Open(string path)
        {
            var doc = new TextRange(document.ContentStart, document.ContentEnd);

            var ext = Path.GetExtension(path).ToLower();

            using (var sr = new FileStream(path, FileMode.Open))
            {
                switch (ext)
                {
                    case ".xaml":
                        {
                            doc.Load(sr, DataFormats.Xaml);
                        }
                        break;
                    case ".rtf":
                        {
                            doc.Load(sr, DataFormats.Rtf);
                        }
                        break;
                    case ".txt":
                    default:
                        {
                            doc.Load(sr, DataFormats.Text);
                        }
                        break;
                }
            }

            IsTextChanged = false;
        }

        public bool Create(bool force = false)
        {
            if (IsTextChanged && !force) return false;

            document.Blocks.Clear();

            return true;
        }

        private void InitSavingStrategies()
        {
            SavingStrategies.Add(".txt", new TextSavingStrategy());

            var xamlSave = new XamlSavingStrategy();

            SavingStrategies.Add(".xml", xamlSave);
            SavingStrategies.Add(".xaml", xamlSave);
        }

        private ISavingStrategy GetSavingStrategy(string path)
        {
            var format = Path.GetExtension(path).ToLower();

            if(SavingStrategies.TryGetValue(format, out ISavingStrategy strategy))
            {
                return strategy;
            }
            else
            {
                return SavingStrategies[".txt"];
            }
        }

        private void BlockOnTextChanged(bool val)
        {
            IsTextChangedBlocked = val;
        }

        public void OnTextChanged(TextContentChangedEventArgs e)
        {
            if(IsTextChangedBlocked) return;

            IsTextChanged = true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Message handlers

        void IRecipient<FileChangedMessage>.Receive(FileChangedMessage message)
        {
            OnTextChanged(message.Value);
        }

        void IRecipient<BlockTextEditorChangeStateMessage>.Receive(BlockTextEditorChangeStateMessage message)
        {
            BlockOnTextChanged(message.Value);
        }

        #endregion
    }
}
