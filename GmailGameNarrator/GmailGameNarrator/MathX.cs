using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using GmailGameNarrator.Narrator;

namespace GmailGameNarrator
{
    public static class MathX
    {
        /// <summary>
        /// Random is not thread safe, but the only thread that should access this is CheckMessagesTask and that thread is locked.
        /// </summary>
        private static Random r = new Random();


        public static object PickOne(this IEnumerable<object> objs)
        {
            //The upper bound of Random.Next is exclusive
            int max = objs.Count();
            int idx = r.Next(0, max);
            return objs.ElementAt(idx);
        }

        public static string RandomString(int length)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                b.Append(((char)r.Next('A', 'Z')).ToString());
                b.Append(((char)r.Next('a', 'z')).ToString());
            }
            return b.ToString();
        }

        /// <summary>
        /// The percentage of total in integer form rounded up.  Example: (100, 20), would return 20, (101, 20), would return 21.
        /// </summary>
        /// <param name="total"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public static int Percent(int total, int percentage)
        {
            int result = total * percentage / 100;
            if ((total * percentage % 100) > 0) result += 1;
            return result;
        }

        public static IEnumerable<Player> RandomizedList(ReadOnlyCollection<Player> players)
        {
            List<Player> randomPlayers = new List<Player>();
            randomPlayers.AddRange(players.OrderBy(x => r.Next()).Take(players.Count));
            return randomPlayers;
        }

        public static bool PercentChance(int percent)
        {
            int chance = r.Next(1, 101);
            if (chance <= percent)
            {
                return true;
            }
            return false;
        }
    }
}
