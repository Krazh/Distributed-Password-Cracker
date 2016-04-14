using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordCrackingClient
{
    class CrackAJack
    {
        private readonly HashAlgorithm _messageDigest;
        private List<User> _users;
        private List<string> _wordList;

        public delegate void CrackedPasswordEventHandler(object sender, UserEventArgs args);

        public delegate void FinishChunkEventHandler(object sender, FinishedChunkEventArgs args);

        public event FinishChunkEventHandler FinishedChunk;
        public event CrackedPasswordEventHandler NewCrackedPassword;

        public CrackAJack()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
            //_messageDigest = new MD5CryptoServiceProvider();
            // seems to be same speed
        }

        public void RunCracking(List<string> wordList, List<User> users)
        {
            _users = users;
            _wordList = wordList;
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Cracking Started");
            CheckWordWithVariations();
            stopwatch.Stop();
            OnFinishedChunk(stopwatch.Elapsed);
        }

        protected virtual void OnFinishedChunk(TimeSpan span)
        {
            FinishedChunk?.Invoke(this, new FinishedChunkEventArgs(span));
            Console.WriteLine("Finished chunk");
        }

        protected virtual void OnNewCrackedPassword(User u, string crackedPassword)
        {
            NewCrackedPassword?.Invoke(this, new UserEventArgs(u, crackedPassword));
            Console.WriteLine("Cracked password for user {0}: {1}", u.Username, crackedPassword);
        }

        private void CheckWordWithVariations()
        {
            foreach (string s in _wordList)
            {
                CheckSingleWord(s);
                CheckSingleWord(StringToUpper(s));
                CheckSingleWord(StringCapitalized(s));
                CheckSingleWord(StringReversed(s));

                for (int i = 0; i < 100; i++)
                {
                    CheckSingleWord(s + i.ToString());
                    CheckSingleWord(i.ToString() + s);
                }

                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        CheckSingleWord(i.ToString() + s + j);
                    }
                }
            }
            
        }

        private string StringToUpper(string s)
        {
            try
            {
                return s.ToUpper();
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to capitalize the entire string of " + s);
                return s;
            }
        }

        private string StringCapitalized(string s)
        {
            return StringUtilities.Capitalize(s);
        }

        private string StringReversed(string s)
        {
            return StringUtilities.Reverse(s);
        }
        
        private void CheckSingleWord(string possiblePassword)
        {
            char[] charArray = possiblePassword.ToCharArray();

            byte[] passwordAsBytes = Array.ConvertAll(charArray, Converter);

            byte[] encryptedPossiblePassword = _messageDigest.ComputeHash(passwordAsBytes);
            foreach (User u in _users)
            {

                byte[] bytes = Encoding.UTF8.GetBytes(u.Password);
                if (CompareBytes(bytes, encryptedPossiblePassword))  //compares byte arrays
                {
                    OnNewCrackedPassword(u, possiblePassword);
                    Console.WriteLine(u.Username + ": " + possiblePassword);
                }
            }
        }

        private byte Converter(char input)
        {
            return Convert.ToByte(input);
        }

        /// <summary>
        /// Compares two byte arrays. Encrypted words are byte arrays
        /// </summary>
        /// <param name="firstArray"></param>
        /// <param name="secondArray"></param>
        /// <returns></returns>
        private static bool CompareBytes(IList<byte> firstArray, IList<byte> secondArray)
        {
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("firstArray");
            //}
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("secondArray");
            //}
            if (firstArray.Count != secondArray.Count)
            {
                return false;
            }
            for (int i = 0; i < firstArray.Count; i++)
            {
                if (firstArray[i] != secondArray[i])
                    return false;
            }
            return true;
        }
    }
}
