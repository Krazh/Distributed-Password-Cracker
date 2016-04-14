using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingClient
{
    class StringUtilities
    {
        public static string Capitalize(String str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (str.Trim().Length == 0)
            {
                return str;
            }
            String firstLetterUppercase = str.Substring(0, 1).ToUpper();
            String theRest = str.Substring(1);
            return firstLetterUppercase + theRest;
        }

        public static string Reverse(String str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (str.Trim().Length == 0)
            {
                return str;
            }
            StringBuilder reverseString = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                reverseString.Append(str.ElementAt(str.Length - 1 - i));
            }
            return reverseString.ToString();
        }

        public static string CapitalizeLastLetter(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (str.Trim().Length == 0)
            {
                return str;
            }
            string lastLetterUppercase = str.Substring(str.Length - 1, 1).ToUpper();
            string theRest = str.Substring(0, str.Length - 1);
            return theRest + lastLetterUppercase;
        }
    }
}
