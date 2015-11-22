using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game
{
    /// <summary>
    /// This is intended to be a dynamic class at some point, using a theme and picking random snippets of descriptive text to make the game more varied and fun.
    /// For now it's static.
    /// </summary>
    class FlavorText
    {
        public static string GetStartGameMessage()
        {
            return "The game has been started.";
        }

        public static string PlayerOutcastMessage
        {
            get
            {
                return "You voted and {0} has been cast out of the town, they will quickly die in the wilderness.";
            }
        }

        public static string Divider = "<br /><hr><br />";

        public static string HtmlBulletList(List<string> list)
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
