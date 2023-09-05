using CommunityToolkit.Mvvm.Messaging.Messages;
using SynergyTextEditor.Classes.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynergyTextEditor.Messages
{
    public class FileChangedMessage : ValueChangedMessage<TextContentChangedEventArgs>
    {
        public FileChangedMessage(TextContentChangedEventArgs value) : base(value)
        {
        }
    }
}
