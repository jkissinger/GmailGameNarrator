using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GmailGameNarrator.Game.Roles;

namespace GmailGameNarrator
{
    class MathX
    {
        private static Random R = new Random();

        public static object PickOne(IEnumerable<object> objs)
        {
            int idx = R.Next(0, objs.Count());
            return objs.ElementAt(idx);
        }

        public static int Percent(int total, int percentage)
        {
            double result = total * percentage / 100;
            return (int)Math.Ceiling(result);
        }
    }
}
