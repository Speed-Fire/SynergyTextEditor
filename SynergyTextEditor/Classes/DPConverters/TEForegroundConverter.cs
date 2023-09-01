using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SynergyTextEditor.Classes.DPConverters
{
    internal class TEForegroundConverter : IDPConverter
    {
        object IDPConverter.Convert(string value)
        {
            var colors = GetColors(value);

            switch(GetBrushName(value))
            {
                case "solid":
                    {
                        return new SolidColorBrush(colors[0]);
                    }
                case "lingrad":
                    {
                        return new LinearGradientBrush(colors[0], colors[1], 45);
                    }
                default:
                    {
                        throw new InvalidCastException();
                    }
            }
        }

        private Color[] GetColors(string value)
        {
            var colors = new Color[2];

            var first = value.IndexOf('(');
            var last = value.LastIndexOf(')');

            if(first == -1 ||  last == -1)
            {
                throw new ArgumentException("Wrong color description!");
            }

            var val = value
                .Substring(first + 1, last - first - 1)
                .Trim()
                .Replace(" ", "");

            if (val[0] == '(')
            {
                var strs = val.Split("),(");
                strs[0] = strs[0].TrimStart('(');
                strs[1] = strs[1].TrimEnd(')');

                for(int i = 0; i < colors.Length; i++)
                    colors[i] = GetColor(strs[i]);
            }
            else
            {
                colors[0] = GetColor(val);
            }

            return colors;
        }

        private Color GetColor(string value)
        {
            var vals = value
                .Split(',')
                .ToList()
                .ConvertAll(byte.Parse);

            if (vals.Count < 3 || vals.Count > 4)
            {
                throw new ArgumentException("Wrong color description!");
            }

            if (vals.Count == 3)
                return Color.FromRgb(vals[0], vals[1], vals[2]);
            else
                return Color.FromArgb(vals[0], vals[1], vals[2], vals[3]);
        }

        private string GetBrushName(string value)
        {
            var end = value.IndexOf("(");

            return value
                .Substring(0, end)
                .Trim()
                .ToLower();
        }
    }
}
