using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace SynergyTextEditor.Classes.SavingStrategies
{
    public class XamlSavingStrategy : ISavingStrategy
    {
        public void Save(FlowDocument document, string path)
        {
            TextRange doc = new TextRange(document.ContentStart, document.ContentEnd);
            using (FileStream fs = File.Create(path))
            {
                doc.Save(fs, DataFormats.Xaml);
            }
        }
    }
}
