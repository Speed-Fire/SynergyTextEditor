using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SynergyTextEditor.Classes.DPConverters
{
    internal class TEFontWeightConverter : IDPConverter
    {
        private static readonly Dictionary<string, FontWeight> _weights = new()
        {
            {"Bold", FontWeights.Bold},
            {"UltraBlack", FontWeights.UltraBlack},
            {"Black", FontWeights.Black},
            {"Thin", FontWeights.Thin},
            {"UltraBold", FontWeights.UltraBold},
            {"ExtraBlack", FontWeights.ExtraBlack},
            {"DemiBold", FontWeights.DemiBold},
            {"ExtraBold", FontWeights.ExtraBold},
            {"ExtraLight", FontWeights.ExtraLight},
            {"Light", FontWeights.Light},
            {"UltraLight", FontWeights.UltraLight},
            {"Normal", FontWeights.Normal},
            {"Heavy", FontWeights.Heavy},
            {"Medium", FontWeights.Medium},
            {"Regular", FontWeights.Regular},
            {"SemiBold", FontWeights.SemiBold}
        };

        object IDPConverter.Convert(string value)
        {
            return _weights[value];
        }
    }
}
