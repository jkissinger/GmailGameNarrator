using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GmailGameNarrator.Narrator;

namespace GmailGameNarrator
{
    public static class StringX
    {
        public static string GetTextAfter(this string str, string delimiter)
        {
            delimiter = delimiter.ToLowerInvariant();
            string result = "";
            int idx = str.IndexOf(delimiter);
            if (idx >= 0) result = str.Substring(idx + delimiter.Length, str.Length - delimiter.Length - idx);
            return result.Trim();
        }

        public static string GetTextBefore(this string str, string delimiter)
        {
            delimiter = delimiter.ToLowerInvariant();
            string result = "";
            int idx = str.IndexOf(delimiter);
            if (idx >= 0) result = str.Substring(0, idx);
            return result;
        }

        public static string ToTitleCase(this string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str).Trim();
        }

        public static string AddSpaces(this string str)
        {
            string result = "";
            foreach(char c in str)
            {
                if (Char.IsUpper(c)) result = result + " ";
                result = result + c;
            }
            return result.Trim();
        }

        public static string b(this object obj)
        {
            return obj.ToString().b();
        }

        public static string b(this string str)
        {
            return str.tag("b");
        }

        public static string i(this string str)
        {
            return str.tag("i");
        }

        public static string li(this string str)
        {
            return str.tag("li");
        }

        public static string tag(this string str, string tag)
        {
            return "<" + tag + ">" + str + "</" + tag + ">";
        }

        public static string HtmlBulletList(this List<string> list)
        {
            if (list.Count == 0) return "";
            string result = "<ul>";
            foreach (string s in list)
            {
                result += "<li>" + s + "</li>";
            }
            return result + "</ul>";
        }
    }
}
