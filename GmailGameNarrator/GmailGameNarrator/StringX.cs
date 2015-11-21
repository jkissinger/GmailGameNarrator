using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator
{
    class StringX
    {

        //Switch to adding string extension?
        //public static string GetTextAfter(this string str)
        public static string GetTextAfter(string str, string delimiter)
        {
            delimiter = delimiter.ToLowerInvariant();
            string result = "";
            int idx = str.IndexOf(delimiter);
            if (idx >= 0) result = str.Substring(idx + delimiter.Length, str.Length - delimiter.Length - idx);
            return result;
        }

        public static string GetTextBefore(string str, string delimiter)
        {
            delimiter = delimiter.ToLowerInvariant();
            string result = "";
            int idx = str.IndexOf(delimiter);
            if (idx >= 0) result = str.Substring(0, idx);
            return result;
        }

        public static string ToTitleCase(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str).Trim();
        }

        public static string AddSpaces(string str)
        {
            string result = "";
            foreach(char c in str)
            {
                if (Char.IsUpper(c)) result = result + " ";
                result = result + c;
            }
            return result.Trim();
        }
    }
}
