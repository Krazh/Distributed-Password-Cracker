using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordCrackingClient
{
    class Client
    {
        private Connection _connection;
        private bool _running = true;
        public List<User> Users = new List<User>();
        private List<string> _wordList = new List<string>();
        private Cracking _crackAJack;

        public Client()
        {
            Console.WriteLine("Host: ");
            string host = Console.ReadLine();
            string port = "7000";
            _crackAJack = new Cracking();
            _crackAJack.NewCrackedPassword += CrackedUserEventHandler;
            _crackAJack.FinishedChunk += FinishChunkEventHandler;
            ConnectToServer(host, port);
            _connection.NewMessage += MessageEventHandler;
            _connection.SendMessage("get_passwords");
            GetChunkFromServer();
            while (_running)
            {
                Thread.Sleep(1000);
            }
        }

        private void GetChunkFromServer(TimeSpan timeSpan = default(TimeSpan))
        {
            string message = "get_chunk" + " " + timeSpan;
            _connection.SendMessage(message);
        }

        private void FinishChunkEventHandler(object sender, FinishedChunkEventArgs args)
        {
            _wordList.Clear();
            GetChunkFromServer(args.TimeSpent);
        }

        private void CrackedUserEventHandler(object sender, UserEventArgs args)
        {
            UploadCrackedUser(args.User, args.CrackedPassword);
        }

        private void MessageEventHandler(object sender, MessageEventArgs args)
        {
            HandleMessage(args.Args);
        }

        private void ConnectToServer(string host, string port)
        {
            _connection = new Connection(host, Convert.ToInt32(port));
            Task.Factory.StartNew(() => _connection.Listen());
        }

        private void UploadCrackedUser(User u, string crackedPassword)
        {
            string success = "submit_success " + u.Username + " " + crackedPassword;
            _connection.SendMessage(success);
        }

        public void HandleMessage(string message)
        {
            try
            {
                string[] splitStrings = message.Split('=');

                switch (splitStrings[0].ToLower())
                {
                    case "passwords":
                        if (splitStrings.Count() < 2)
                        {
                            throw new Exception("No passwords received");
                        }
                        for (int i = 1; i < splitStrings.Count(); i++)
                        {
                            if (splitStrings[i].Count() > 10)
                            {
                                string[] d = splitStrings[i].Split(':');
                                d[0] = d[0].TrimStart(' ');
                                Users.Add(new User(d[0], d[1]));
                            }
                        }
                        break;
                    case "chunk":
                        if (splitStrings.Count() < 2)
                        {
                            throw new Exception("No wordlist received");
                        }
                        for (int i = 1; i < splitStrings.Count(); i++)
                        {
                            _wordList.Add(splitStrings[i]);
                        }
                        _crackAJack.RunCracking(_wordList, Users);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
