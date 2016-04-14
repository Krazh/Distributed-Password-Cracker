using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrackingClient
{
    public class Client
    {
        private Connection _connection;
        private bool _running = true;
        public ObservableCollection<User> Users = new ObservableCollection<User>();

        public Client()
        {
            string host = "127.0.0.1";
            string port = "6789";
            ConnectToServer(host, port);
            _connection.SendMessage("get_passwords");

            while (_running)
            {

            }
        }

        private void ConnectToServer(string host, string port)
        {
            _connection = new Connection(host, Convert.ToInt32(port), this);
            Task.Factory.StartNew(() => _connection.Listen());
        }

        public void HandleMessage(string message)
        {
            string[] splitStrings = message.Split('=');

            switch (splitStrings[0].ToLower())
            {
                case "passwords":
                    for (int i = 1; i < splitStrings.Count(); i++)
                    {
                        string[] d = splitStrings[i].Split('=');
                        App.Current.Dispatcher.Invoke(() => Users.Add(new User(d[0], d[1])));
                    }
                    foreach (User user in Users)
                    {
                        Console.WriteLine(user.Username);
                    }
                    break;
                case "chunk":
                    break;
            }
        }
    }
}
