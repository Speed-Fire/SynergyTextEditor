using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable

namespace SynergyTextEditor.ViewModels
{
    public abstract class BaseViewModel
    {
        protected CommandBindingCollection commandBindings = new();
        public CommandBindingCollection CommandBindings => commandBindings;

        protected abstract void RegisterMessages();
        protected virtual void RegisterCommandBindings() { }
    }
}
