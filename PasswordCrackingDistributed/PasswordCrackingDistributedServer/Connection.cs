using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingDistributedServer
{
    class Connection
    {
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private NetworkStream _ns;

        public delegate void MessageEventHandler(object sender, MessageEventArgs args);

        public event MessageEventHandler NewMessage;
        private int _progress = 0;
        private int _seconds = 0;
        
        public int UserId;
         
        public bool Running = true;
        public TcpClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        public StreamReader Reader
        {
            get { return _reader; }
            set { _reader = value; }
        }

        public StreamWriter Writer
        {
            get { return _writer; }
            set { _writer = value; }
        }

        public NetworkStream PasswordStream
        {
            get { return _ns; }
            set { _ns = value; }
        }

        public Connection(TcpClient client, int userId)
        {
            Client = client;
            UserId = userId;
            _ns = Client.GetStream();
            Reader = new StreamReader(_ns);
            Writer = new StreamWriter(_ns);
            Writer.AutoFlush = true;
        }

        protected virtual void OnNewMessage(string args)
        {
            NewMessage?.Invoke(this, new MessageEventArgs(args, UserId));
        }

        public void Dispose()
        {
            Reader.Close();
            Writer.Close();
            _ns.Dispose();
            Client.Close();
        }

        public void Listen()
        {
            try
            {
                while (Running)
                {
                    if (Client.Connected)
                    {
                        string receivedString = Reader.ReadLine();
                        OnNewMessage(receivedString);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Dispose();
            }
        }



        public void AddProgress(int progress, int seconds)
        {
            _progress += progress;
            _seconds = seconds;
            Console.WriteLine("User {0} has completed {1}% of the work assigned in {2} seconds", UserId, _progress, _seconds);
        }
    }
}
