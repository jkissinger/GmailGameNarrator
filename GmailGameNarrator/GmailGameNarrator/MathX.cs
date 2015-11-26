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
        private static Random DoNotUseThisRandom = new Random();
        /// <summary>
        /// Random is not documented as thread safe so there are thread locks on the methods using <see cref="Rand"/>
        /// </summary>
        static object lockRandom = new object();

        /// <summary>
        /// On second thought, not sure this works, need to investigate.
        /// </summary>
        /// TODO: Investigate if this is thread safe
        private static Random Rand
        {
            get
            {
                lock (lockRandom)
                {
                    return DoNotUseThisRandom;
                }
            }
        }

        public static object PickOne(this IEnumerable<object> objs)
        {
            int idx = Rand.Next(0, objs.Count() - 1);
            return objs.ElementAt(idx);
        }

        public static string RandomString(int length)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                b.Append(((char)Rand.Next('A', 'Z')).ToString());
                b.Append(((char)Rand.Next('a', 'z')).ToString());
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
            randomPlayers.AddRange(players.OrderBy(x => Rand.Next()).Take(players.Count));
            return randomPlayers;
        }
    }
}
