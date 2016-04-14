using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingClient
{
    class FinishedChunkEventArgs : EventArgs
    {
        private readonly TimeSpan _timeSpent;

        public FinishedChunkEventArgs(TimeSpan timeSpent)
        {
            _timeSpent = timeSpent;
        }

        public TimeSpan TimeSpent
        {
            get { return _timeSpent; }
        }
    }
}
