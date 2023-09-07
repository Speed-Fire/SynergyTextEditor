using CommunityToolkit.Mvvm.Messaging;
using SynergyTextEditor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynergyTextEditor.Classes.Blockers
{
    internal class TextChangedBlocker 
        :
        IBlocker,
        IRecipient<StopTextChangedMessage>
    {
        private volatile bool _isBlocked = false;
        private object _locker = new();

        public TextChangedBlocker()
        {
            WeakReferenceMessenger.Default.Register(this);
        }

        public bool IsBlocked()
        {
            lock (_locker)
            {
                var res = _isBlocked;

                return _isBlocked;
            }
        }

        public void Receive(StopTextChangedMessage message)
        {
            lock (_locker)
            {
                _isBlocked = message.Value;
            }
        }
    }
}
