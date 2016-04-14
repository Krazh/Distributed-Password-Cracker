using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingClient
{
    public class MessageEventArgs : EventArgs
    {
        private readonly string _args;

        public MessageEventArgs(string args)
        {
            this._args = args;
        }

        public string Args
        {
            get { return _args; }
        }
    }
}
