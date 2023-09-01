using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SynergyTextEditor.Classes.DPConverters
{
    internal interface IDPConverter
    {
        internal object Convert(string value);
    }
}
