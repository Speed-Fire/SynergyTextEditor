using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;



namespace SynergyTextEditor.ViewModels
{
    public abstract class BaseViewModel :
        INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected CommandBindingCollection commandBindings = new();

        public CommandBindingCollection CommandBindings => commandBindings;

        protected abstract void RegisterMessages();
        protected virtual void RegisterCommandBindings() { }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
