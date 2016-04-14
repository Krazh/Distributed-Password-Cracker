using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingClient
{
    class Cracking
    {
        /// <summary>
        /// The algorithm used for encryption.
        /// Must be exactly the same algorithm that was used to encrypt the passwords in the password file
        /// </summary>
        private readonly HashAlgorithm _messageDigest;

        private List<string> _specialCharAndMoreList = new List<string>(); 

        public delegate void CrackedPasswordEventHandler(object sender, UserEventArgs args);

        public delegate void FinishChunkEventHandler(object sender, FinishedChunkEventArgs args);

        public event FinishChunkEventHandler FinishedChunk;
        public event CrackedPasswordEventHandler NewCrackedPassword;

        public Cracking()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
            //_messageDigest = new MD5CryptoServiceProvider();
            // seems to be same speed
            var list = new[] { "!", "#", "¤", "%", "&", "/", "(", ")", "=", "?", "´", "`", "|", "<", ">", "½", "@", "£", "$", "{", "[", "]", "}", "*", "-", "+", "_", ";", ":", ",", ".", "\\", " ", "ë", "é", "è", "ä", "ü", "á", "à", "ñ", "ö", "å", "æ", "ø", "ï", "í", "ì", "ÿ", "ý", "ó", "ò" };
            foreach (string s in list)
            {
                _specialCharAndMoreList.Add(s);
            }

            for (int i = 0; i < 100; i++)
            {
                _specialCharAndMoreList.Add(i.ToString());
            }

            for (int i = 1900; i < DateTime.Now.Year; i++)
            {
                _specialCharAndMoreList.Add(i.ToString());
            }
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

        /// <summary>
        /// Runs the password cracking algorithm
        /// </summary>
        public void RunCracking(List<string> wordList, List<User> users)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine();
            Console.WriteLine("Number of words: " + wordList.Count);
            List<UserInfo> userInfos = new List<UserInfo>();
            foreach (User user in users)
            {
                userInfos.Add(new UserInfo(user.Username, user.Password + "="));
            }

            List<UserInfoClearText> result = new List<UserInfoClearText>();

            foreach (string s in wordList)
            {
                IEnumerable<UserInfoClearText> partialResult = CheckWordWithVariations(s, userInfos);
                result.AddRange(partialResult);
            }

            stopwatch.Stop();
            Console.WriteLine(string.Join(", ", result));
            Console.WriteLine("Out of {0} password {1} was found ", userInfos.Count, result.Count);
            Console.WriteLine();
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
            OnFinishedChunk(stopwatch.Elapsed);
        }

        /// <summary>
        /// Generates a lot of variations, encrypts each of the and compares it to all entries in the password file
        /// </summary>
        /// <param name="dictionaryEntry">A single word from the dictionary</param>
        /// <param name="userInfos">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private IEnumerable<UserInfoClearText> CheckWordWithVariations(string dictionaryEntry, List<UserInfo> userInfos)
        {
            List<UserInfoClearText> result = new List<UserInfoClearText>(); //might be empty

            CheckKeywordWithoutVariation(dictionaryEntry, userInfos, result);
            CheckKeywordAllUpperCase(dictionaryEntry, userInfos, result);
            CheckKeywordCapitalizedFirstLastBoth(dictionaryEntry, userInfos, result);
            CheckReverseKeyword(dictionaryEntry, userInfos, result);
            CheckNumberInFrontAndAfterKeyword(dictionaryEntry, userInfos, result);
            CheckKeywordForEachCharWithList(dictionaryEntry, userInfos, result);
            CheckKeywordForEachCharUppercase(dictionaryEntry, userInfos, result);

            return result;
        }

        private void CheckKeywordForEachCharUppercase(string dictionaryEntry, List<UserInfo> userInfos,
            List<UserInfoClearText> result)
        {
            for (int i = 0; i < dictionaryEntry.Length; i++)
            {
                string a;
                string b;
                string c;

                if (i == 0)
                {
                    a = "";
                }
                else
                {
                    a = dictionaryEntry.Substring(0, i);
                }
                b = dictionaryEntry.Substring(i, 1).ToUpper();
                if ((i + 1 <= dictionaryEntry.Length))
                {
                    c = dictionaryEntry.Substring(i + 1, dictionaryEntry.Length - a.Length - b.Length);
                }
                else
                {
                    c = "";
                }
                CheckSingleWord(userInfos,a + b + c);
            }
        }

        private void CheckKeywordForEachCharWithList(string dictionaryEntry, List<UserInfo> userInfos,
            List<UserInfoClearText> result)
        {
            foreach (string s in _specialCharAndMoreList)
            {
                for (int i = 0; i < dictionaryEntry.Length + 1; i++)
                {
                    string a = dictionaryEntry.Substring(0, i);
                    string b = dictionaryEntry.Substring(i, dictionaryEntry.Length - a.Length);
                    string possiblePassword = a + s + b;
                    CheckSingleWord(userInfos, possiblePassword);
                }
            }
        }

        //private void CheckSpecialCharWithKeyword(string dictionaryEntry, List<UserInfo> userInfos, List<UserInfoClearText> result)
        //{
            
        //    foreach (string s in list)
        //    {
        //        IEnumerable<UserInfoClearText> partialResultSpecialCharAfter = CheckSingleWord(userInfos, dictionaryEntry + s);
        //        result.AddRange(partialResultSpecialCharAfter);

        //        IEnumerable<UserInfoClearText> partialResultSpecialCharBefore = CheckSingleWord(userInfos,
        //            s + dictionaryEntry);
        //        result.AddRange(partialResultSpecialCharBefore);


        //        for (int i = 0; i < dictionaryEntry.Length + 1; i++)
        //        {
        //            string a = dictionaryEntry.Substring(0, i);
        //            string b = dictionaryEntry.Substring(i, dictionaryEntry.Length - a.Length);
        //            string possiblePassword = a + s + b;
        //            CheckSingleWord(userInfos, possiblePassword);
        //        }
                
        //    }
        //}

        private void CheckNumberInFrontAndAfterKeyword(string dictionaryEntry, List<UserInfo> userInfos, List<UserInfoClearText> result)
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                    IEnumerable<UserInfoClearText> partialResultStartEndDigit = CheckSingleWord(userInfos,
                        possiblePasswordStartEndDigit);
                    result.AddRange(partialResultStartEndDigit);
                }
            }

            for (int i = 1900; i < DateTime.Now.Year; i++)
            {
                for (int j = 1900; j < DateTime.Now.Year; j++)
                {
                    String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                    IEnumerable<UserInfoClearText> partialResultStartEndDigit = CheckSingleWord(userInfos,
                        possiblePasswordStartEndDigit);
                    result.AddRange(partialResultStartEndDigit);
                }
            }
        }

        //private void CheckNumberAfterKeyword(string dictionaryEntry, List<UserInfo> userInfos, List<UserInfoClearText> result)
        //{
        //    for (int i = 0; i < 100; i++)
        //    {
        //        String possiblePasswordStartDigit = dictionaryEntry + i;
        //        IEnumerable<UserInfoClearText> partialResultStartDigit = CheckSingleWord(userInfos, possiblePasswordStartDigit);
        //        result.AddRange(partialResultStartDigit);
        //    }

        //    for (int i = 1900; i < DateTime.Now.Year; i++)
        //    {
        //        String possiblePasswordEndDigit = dictionaryEntry + i;
        //        IEnumerable<UserInfoClearText> partialResultEndDigit = CheckSingleWord(userInfos, possiblePasswordEndDigit);
        //        result.AddRange(partialResultEndDigit);
        //    }
        //}

        //private void CheckNumberInFrontOfKeyword(string dictionaryEntry, List<UserInfo> userInfos, List<UserInfoClearText> result)
        //{
        //    for (int i = 0; i < 100; i++)
        //    {
        //        String possiblePasswordEndDigit = i + dictionaryEntry;
        //        IEnumerable<UserInfoClearText> partialResultEndDigit = CheckSingleWord(userInfos, possiblePasswordEndDigit);
        //        result.AddRange(partialResultEndDigit);
        //    }

        //    for (int i = 1900; i < DateTime.Now.Year; i++)
        //    {
        //        String possiblePasswordEndDigit = i + dictionaryEntry;
        //        IEnumerable<UserInfoClearText> partialResultEndDigit = CheckSingleWord(userInfos, possiblePasswordEndDigit);
        //        result.AddRange(partialResultEndDigit);
        //    }
        //}

        private void CheckReverseKeyword(string dictionaryEntry, List<UserInfo> userInfos, List<UserInfoClearText> result)
        {
            string possiblePasswordReverse = StringUtilities.Reverse(dictionaryEntry);
            IEnumerable<UserInfoClearText> partialResultReverse = CheckSingleWord(userInfos, possiblePasswordReverse);
            result.AddRange(partialResultReverse);

            IEnumerable<UserInfoClearText> partialResultReverse2 = CheckSingleWord(userInfos, possiblePasswordReverse + dictionaryEntry);
            result.AddRange(partialResultReverse2);

            IEnumerable<UserInfoClearText> partialResultReverse3 = CheckSingleWord(userInfos, dictionaryEntry + possiblePasswordReverse);
            result.AddRange(partialResultReverse3);
            //CheckNumberInFrontOfKeyword(possiblePasswordReverse, userInfos, result);
            //CheckNumberAfterKeyword(possiblePasswordReverse, userInfos, result);
            //CheckNumberInFrontAndAfterKeyword(possiblePasswordReverse, userInfos, result);
        }

        private void CheckKeywordCapitalizedFirstLastBoth(string dictionaryEntry, List<UserInfo> userInfos, List<UserInfoClearText> result)
        {
            string possiblePasswordCapitalized = StringUtilities.Capitalize(dictionaryEntry);
            IEnumerable<UserInfoClearText> partialResultCapitalized = CheckSingleWord(userInfos, possiblePasswordCapitalized);
            result.AddRange(partialResultCapitalized);

            string possiblePasswordLastLetterCapitalized = StringUtilities.CapitalizeLastLetter(dictionaryEntry);
            IEnumerable<UserInfoClearText> partialResultLastCap = CheckSingleWord(userInfos,
                possiblePasswordLastLetterCapitalized);
            result.AddRange(partialResultLastCap);

            string possiblePasswordFirstAndLastCap = StringUtilities.CapitalizeLastLetter(possiblePasswordCapitalized);
            IEnumerable<UserInfoClearText> partialResultBothEndsCapped = CheckSingleWord(userInfos,
                possiblePasswordFirstAndLastCap);
            result.AddRange(partialResultBothEndsCapped);

            //CheckNumberInFrontOfKeyword(possiblePasswordCapitalized, userInfos, result);
            //CheckNumberAfterKeyword(possiblePasswordCapitalized, userInfos, result);
            //CheckNumberInFrontAndAfterKeyword(possiblePasswordCapitalized, userInfos, result);
        }

        private void CheckKeywordAllUpperCase(string dictionaryEntry, List<UserInfo> userInfos, List<UserInfoClearText> result)
        {
            String possiblePasswordUpperCase = dictionaryEntry.ToUpper();
            IEnumerable<UserInfoClearText> partialResultUpperCase = CheckSingleWord(userInfos, possiblePasswordUpperCase);
            result.AddRange(partialResultUpperCase);

            //CheckNumberInFrontOfKeyword(possiblePasswordUpperCase, userInfos, result);
            //CheckNumberAfterKeyword(possiblePasswordUpperCase, userInfos, result);
            //CheckNumberInFrontAndAfterKeyword(possiblePasswordUpperCase, userInfos, result);
        }

        private void CheckKeywordWithoutVariation(string dictionaryEntry, List<UserInfo> userInfos, List<UserInfoClearText> result)
        {
            String possiblePassword = dictionaryEntry;
            IEnumerable<UserInfoClearText> partialResult = CheckSingleWord(userInfos, possiblePassword);
            result.AddRange(partialResult);

            IEnumerable<UserInfoClearText> partialResult2 = CheckSingleWord(userInfos, possiblePassword + possiblePassword);
            result.AddRange(partialResult2);
        }

        /// <summary>
        /// Checks a single word (or rather a variation of a word): Encrypts and compares to all entries in the password file
        /// </summary>
        /// <param name="userInfos"></param>
        /// <param name="possiblePassword">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private IEnumerable<UserInfoClearText> CheckSingleWord(IEnumerable<UserInfo> userInfos, string possiblePassword)
        {
            char[] charArray = possiblePassword.ToCharArray();
            byte[] passwordAsBytes = Array.ConvertAll(charArray, PasswordFileHandler.GetConverter());

            byte[] encryptedPassword = _messageDigest.ComputeHash(passwordAsBytes);
            //string encryptedPasswordBase64 = System.Convert.ToBase64String(encryptedPassword);

            List<UserInfoClearText> results = new List<UserInfoClearText>();

            foreach (UserInfo userInfo in userInfos)
            {
                if (CompareBytes(userInfo.EntryptedPassword, encryptedPassword))  //compares byte arrays
                {
                    results.Add(new UserInfoClearText(userInfo.Username, possiblePassword));
                    Console.WriteLine(userInfo.Username + " " + possiblePassword);
                    OnNewCrackedPassword(new User(userInfo.Username, userInfo.EntryptedPasswordBase64), possiblePassword);
                    Console.WriteLine("Password cracked");
                }
            }
            return results;
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
