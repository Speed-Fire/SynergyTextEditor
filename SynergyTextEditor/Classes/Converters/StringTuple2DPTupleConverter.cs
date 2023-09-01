using SynergyTextEditor.Classes.DPConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace SynergyTextEditor.Classes.Converters
{
    internal class StringTuple2DPTupleConverter :
        IConverter<TupleSerializable<string, string>, Tuple<DependencyProperty, object>>
    {
        private static readonly Dictionary<string, DependencyProperty> _properties = new()
        {
            { "ForegroundProperty", TextElement.ForegroundProperty },
            { "FontWeightProperty", TextElement.FontWeightProperty },
        };

        private static readonly Dictionary<DependencyProperty, IDPConverter> _converters = new()
        {
            {TextElement.ForegroundProperty, new TEForegroundConverter() },
            {TextElement.FontWeightProperty, new TEFontWeightConverter() },
        };

        Tuple<DependencyProperty, object> IConverter<TupleSerializable<string, string>, Tuple<DependencyProperty, object>>.Convert(TupleSerializable<string, string> from)
        {
            var dp = _properties[from.Item1];
            var val = _converters[dp].Convert(from.Item2);

            return new Tuple<DependencyProperty, object>(dp, val);
        }
    }
}
