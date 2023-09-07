using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace SynergyTextEditor.Classes.Helpers
{
    public static class SymbolHelper
    {
        public static double GetCharWidth(TextBoxBase _tb, char symbol)
        {
            return GetStringWidth(_tb, new string(symbol, 1));
        }

        public static double GetStringWidth(TextBoxBase _tb, string str)
        {
            var formattedText = new FormattedText(
                str,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(_tb.FontFamily, _tb.FontStyle, _tb.FontWeight, _tb.FontStretch),
                _tb.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return formattedText.Width;
        }
    }
}
