using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SynergyTextEditor.Classes.MenuItemRadioControllers
{
    public interface IMenuItemRadioController
    {
        public void Fill(MenuItem root);

        public void Select();
    }
}
