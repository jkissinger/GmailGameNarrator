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
        public static string GetTextAfter(string str, string delimiter)
        {
            string result = "";
            int idx = str.IndexOf(delimiter);
            result = str.Substring(idx + delimiter.Length, str.Length - delimiter.Length - idx);
            return result;
        }

        public static string ToTitleCase(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }
    }
}
