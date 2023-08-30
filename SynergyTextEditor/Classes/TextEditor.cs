using Microsoft.Win32;
using SynergyTextEditor.Classes.SavingStrategies;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

#nullable disable

namespace SynergyTextEditor.Classes
{
    public class TextEditor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<string, ISavingStrategy> SavingStrategies = new();

        public bool TextChanged { get; set; } = false;

        private readonly FlowDocument document;

        public TextEditor(FlowDocument document)
        {
            InitSavingStrategies();
            this.document = document;
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

            TextChanged = false;
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
        }

        public bool Create(bool force = false)
        {
            if (TextChanged && !force) return false;

            document.Blocks.Clear();

            return true;
        }

        private void HighlightText()
        {

        }

        private void InitSavingStrategies()
        {

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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
