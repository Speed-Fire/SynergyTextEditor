using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynergyTextEditor.Classes.Converters
{
    internal interface IConverter<From, To>
    {
        internal To Convert(From from);
    }
}
