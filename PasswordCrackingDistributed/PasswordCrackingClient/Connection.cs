using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingClient
{
    class Connection
    {
        private TcpClient _tcpClient;
        private NetworkStream _ns;
        private StreamWriter _writer;
        private StreamReader _reader;

        public delegate void MessageEventHandler(object sender, MessageEventArgs args);

        public event MessageEventHandler NewMessage;

        public bool Running = true;

        public TcpClient TcpClient
        {
            get { return _tcpClient; }
            set { _tcpClient = value; }
        }

        public Connection(string ip, int port)
        {
            _tcpClient = new TcpClient(ip, port);
            _ns = _tcpClient.GetStream();
            _reader = new StreamReader(_ns);
            _writer = new StreamWriter(_ns);
        }

        protected virtual void OnNewMessage(string args)
        {
            NewMessage?.Invoke(this, new MessageEventArgs(args));
        }

        public void SendMessage(string message)
        {
            try
            {
                if (_tcpClient.Connected)
                {
                    _writer.WriteLine(message);
                    _writer.Flush();
                }
                else
                {
                    throw new Exception("Connection isn't active");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Listen()
        {
            try
            {
                while (Running)
                {
                    if (_tcpClient.Connected)
                    {
                        string receivedString = _reader.ReadLine();
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

        public void Dispose()
        {
            _ns.Close();
            _tcpClient.Close();
        }
    }
}
