using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SynergyTextEditor.Messages
{
    public class TextChangedMessage : ValueChangedMessage<TextChangedEventArgs>
    {
        public TextChangedMessage(TextChangedEventArgs value) : base(value)
        {
        }
    }
}
