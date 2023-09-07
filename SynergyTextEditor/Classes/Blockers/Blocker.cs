using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynergyTextEditor.Classes.Blockers
{
    public class Blocker : IBlocker
    {
        private volatile bool _isBlocked = false;
        private readonly object _locker = new();

        public bool IsBlocked()
        {
            lock (_locker)
            {
                return _isBlocked;
            }
        }

        public void SetState(bool isBlocked)
        {
            lock(_locker)
            {
                _isBlocked = isBlocked;
            }
        }
    }
}
