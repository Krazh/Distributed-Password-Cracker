using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingClient
{
    class UserEventArgs : EventArgs
    {
        private readonly User _user;
        private readonly string _crackedPassword;

        public UserEventArgs(User user, string crackedPassword)
        {
            _user = user;
            _crackedPassword = crackedPassword;
        }

        public User User
        {
            get { return _user; }
        }

        public string CrackedPassword
        {
            get { return _crackedPassword; }
        }
    }
}
