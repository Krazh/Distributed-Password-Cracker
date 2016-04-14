using System;

namespace PasswordCrackingDistributedServer
{
    public class MessageEventArgs : EventArgs
    {
        private readonly string _args;
        private readonly int _userId;

        public MessageEventArgs(string args, int userId)
        {
            this._args = args;
            this._userId = userId;
        }

        public string Args
        {
            get { return _args; }
        }

        public int UserId
        {
            get { return _userId; }
        }
    }
}
