using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynergyTextEditor.Messages
{
    public class ThemeChangedMessage : ValueChangedMessage<string>
    {
        public ThemeChangedMessage(string value) : base(value)
        {
        }
    }
}
