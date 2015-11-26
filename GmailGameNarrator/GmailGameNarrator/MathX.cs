using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using GmailGameNarrator.Game;

namespace GmailGameNarrator
{
    public static class MathX
    {
        private static Random R = new Random();
        /// <summary>
        /// Random is not documented as thread safe so there are thread locks on the methods using <see cref="R"/>
        /// </summary>
        static object lockRandom = new object();

        public static object PickOne(this IEnumerable<object> objs)
        {
            int idx = R.Next(0, objs.Count());
            return objs.ElementAt(idx);
        }

        public static string RandomString(int length)
        {
            lock (lockRandom)
            {
                StringBuilder b = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    b.Append(((char)R.Next('A', 'Z')).ToString());
                    b.Append(((char)R.Next('a', 'z')).ToString());
                }
                return b.ToString();
            }
        }

        /// <summary>
        /// The percentage of total in integer form rounded up.  Example: (100, 20), would return 20, (101, 20), would return 21.
        /// </summary>
        /// <param name="total"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public static int Percent(int total, int percentage)
        {
            lock (lockRandom)
            {
                int result = total * percentage / 100;
                if ((total * percentage % 100) > 0) result += 1;
                return result;
            }
        }

        public static IEnumerable<Player> RandomizedList(ReadOnlyCollection<Player> players)
        {
            List<Player> randomPlayers = new List<Player>();
            randomPlayers.AddRange(players.OrderBy(x => R.Next()).Take(players.Count));
            return randomPlayers;
        }
    }
}
