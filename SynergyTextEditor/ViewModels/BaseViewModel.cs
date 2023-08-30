using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable

namespace SynergyTextEditor.ViewModels
{
    public class BaseViewModel
    {
        protected CommandBindingCollection commandBindings = new();
        public CommandBindingCollection CommandBindings => commandBindings;
    }
}
