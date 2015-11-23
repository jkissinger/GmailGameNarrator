using System;
using System.Collections.Generic;
using System.Linq;

namespace GmailGameNarrator
{
    static class MathX
    {
        private static Random R = new Random();

        public static object PickOne(this IEnumerable<object> objs)
        {
            int idx = R.Next(0, objs.Count());
            return objs.ElementAt(idx);
        }

        public static int Percent(int total, int percentage)
        {
            int result = total * percentage / 100;
            if ((total * percentage % 100) > 0) result += 1;
            return result;
        }
    }
}
