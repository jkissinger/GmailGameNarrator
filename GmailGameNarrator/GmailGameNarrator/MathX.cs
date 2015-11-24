using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GmailGameNarrator
{
    static class MathX
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

        public static int Percent(int total, int percentage)
        {
            lock (lockRandom)
            {
                int result = total * percentage / 100;
                if ((total * percentage % 100) > 0) result += 1;
                return result;
            }
        }
    }
}
