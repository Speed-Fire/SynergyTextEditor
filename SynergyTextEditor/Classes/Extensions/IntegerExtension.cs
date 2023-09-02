using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynergyTextEditor.Classes.Extensions
{
    internal static class IntegerExtension
    {
        public static int GetCipherCount(this int i)
        {
            if (i == 0) return 1;

            var copy = i;
            var count = 0;

            while (copy > 0)
            {
                count++;
                copy /= 10;
            }

            return count;
        }
    }
}
