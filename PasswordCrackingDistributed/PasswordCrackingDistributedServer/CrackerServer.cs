using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingDistributedServer
{
    class CrackerServer
    {
        private TcpClient _client = new TcpClient();
        private IPAddress _ip = IPAddress.Any;
        private int _port = 7000;
        private bool _running = true;
        private string passwordFile = "passwords.txt";
        private string dictionaryFilePath = "webster-dictionary.txt";
        private List<string> _listOfWords = new List<string>(); 
        private List<Chunk> _chunks = new List<Chunk>();
        private List<User> Users = new List<User>(); 
        private int _lastIndexUsed;
        private int baseChunkSize = 1000;
        private int baseTime = 60;

        public List<Connection> Connections = new List<Connection>(); 

        public CrackerServer()
        {
            TcpListener listener = new TcpListener(_ip, _port);
            listener.Start();
            List<string> dictionaryFile = File.ReadAllLines(dictionaryFilePath).ToList();
            //List<string> danishDictionaryFile = File.ReadAllLines("words-da").ToList();
            //List<string> combinedList = dictionaryFile;
            //combinedList.AddRange(dictionaryFile);
            //combinedList.AddRange(danishDictionaryFile);
            _listOfWords = dictionaryFile.OrderBy(x => x).ToList();

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(_listOfWords[i]);
            }
            
            while (true)
            {
                _client = listener.AcceptTcpClient();
                Random rnd = new Random();
                int uId = (Connections.Count + 1) * rnd.Next(1, 50000);
                Connection conn = new Connection(_client, uId);
                conn.NewMessage += MessageEventHandler;
                Console.WriteLine("Client {0} connected", conn.UserId);
                Connections.Add(conn);
                Task.Factory.StartNew(() => conn.Listen());
            }
        }

        private void MessageEventHandler(object sender, MessageEventArgs args)
        {
            IncomingMessage(args.Args, args.UserId);
        }

        public void IncomingMessage(string message, int userId)
        {
            try
            {
                string[] splitStrings = message.Split(' ');
                int i = GetIndexOfConnectionFromUserId(userId);
                switch (splitStrings[0].ToLower())
                {
                    case "get_passwords":
                        var passwordList = File.ReadAllLines(passwordFile);
                        string passwordFileList = "passwords=";
                        foreach (string s in passwordList)
                        {
                            passwordFileList += s + " ";
                        }
                        Connections[i].Writer.WriteLine(passwordFileList);
                        break;
                    case "get_chunk":
                        Chunk chunk = GetChunk(TimeSpan.Parse(splitStrings[1]), userId);
                        string list = "chunk";
                        foreach (string s in chunk.WordList)
                        {
                            list += "=" + s;
                        }
                        Connections[i].Writer.WriteLine(list);
                        Console.WriteLine("Chunk sent to client {0}, last chunk took {1} seconds.", userId, splitStrings[1]);
                        break;
                    case "add_progress":
                        Connections[i].AddProgress(Convert.ToInt32(splitStrings[1]), Convert.ToInt32(splitStrings[2]));
                        break;
                    case "submit_success":
                        Users.Add(new User(splitStrings[1], splitStrings[2]));
                        Console.WriteLine("Password found for user {0}: {1}", splitStrings[1], splitStrings[2]);
                        break;
                    case "end_program":
                        Connections[i].Writer.WriteLine("end_program");
                        Connections[i].Dispose();
                        Connections.RemoveAt(i);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        private int GetIndexOfConnectionFromUserId(int userId)
        {
            var found = Connections.FirstOrDefault(x => x.UserId == userId);
            return Connections.IndexOf(found);
        }

        private Chunk GetChunk(TimeSpan timeElapsed, int userId)
        {
            Chunk thisChunk = new Chunk();
            thisChunk.UserId = userId;
            thisChunk.ElapsedTime = timeElapsed;
            double multiplier;
            int numberOfWords = 0;
            lock (_listOfWords)
            {
                if (_lastIndexUsed == 0)
                {
                    for (int i = _lastIndexUsed; i < baseChunkSize; i++)
                    {
                        thisChunk.WordList.Add(_listOfWords[i]);
                        numberOfWords++;
                    }
                    _chunks.Add(thisChunk);
                    _lastIndexUsed = numberOfWords;
                }
                else
                {
                    if (timeElapsed.TotalSeconds < 1)
                    {
                        multiplier = 30;
                    }
                    else
                    {
                        multiplier = baseTime / timeElapsed.TotalSeconds;
                    }
                    int highestIndex = baseChunkSize + _lastIndexUsed;
                    if (!IsFirstChunkForUser(userId))
                    {
                        var found = _chunks.FindLast(x => x.UserId == userId);
                        int l = _chunks.IndexOf(found);
                        int lastChunkSize = _chunks[l].WordList.Count;
                        int possibleHighestIndex = Convert.ToInt32(lastChunkSize * multiplier) + _lastIndexUsed;
                        if (possibleHighestIndex < _listOfWords.Count)
                        {
                            highestIndex = possibleHighestIndex;
                        }
                        else
                        {
                            highestIndex = _listOfWords.Count;
                        }
                    }

                    for (int i = _lastIndexUsed + 1; i < highestIndex; i++)
                    {
                        thisChunk.WordList.Add(_listOfWords[i]);
                        numberOfWords++;
                    }
                    _chunks.Add(thisChunk);
                    _lastIndexUsed += numberOfWords;
                }
            }
            return thisChunk;
        }

        private bool IsFirstChunkForUser(int userId)
        {
            var found = _chunks.FindAll(x => x.UserId == userId);
            if (found.Count == 0)
                return true;
            return false;
        }
    }
}
