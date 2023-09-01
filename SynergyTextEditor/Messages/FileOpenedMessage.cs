using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynergyTextEditor.Messages
{
    internal class FileOpenedMessage : ValueChangedMessage<string>
    {
        public FileOpenedMessage(string value) : base(value)
        {
            
        }
    }
}
