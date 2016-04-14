using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CrackingClient
{
    public class Connection
    {
        private TcpClient _tcpClient;
        private NetworkStream _ns;
        private StreamWriter _writer;
        private StreamReader _reader;
        private Client _client;

        public bool Running = true;

        public TcpClient TcpClient
        {
            get { return _tcpClient; }
            set { _tcpClient = value; }
        }

        public Connection(string ip, int port, Client client)
        {
            _tcpClient = new TcpClient(ip, port);
            _client = client;
            _ns = _tcpClient.GetStream();
            _reader = new StreamReader(_ns);
            _writer = new StreamWriter(_ns);
            _writer.AutoFlush = true;
        }

        public void SendMessage(string message)
        {
            try
            {
                if (_tcpClient.Connected)
                {
                    _writer.WriteLine(message);
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
                        _client.HandleMessage(receivedString);
                    }
                }
            }
            catch (Exception)
            {
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
