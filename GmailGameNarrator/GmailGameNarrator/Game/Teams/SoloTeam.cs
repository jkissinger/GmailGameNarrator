using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game.Teams
{
    public class SoloTeam : Team
    {
        public override bool KnowsTeammates
        {
            get
            {
                return false;
            }
        }

        public override int MinPercentComposition
        {
            get
            {
                return 0;
            }
        }

        public override string Name
        {
            get
            {
                return "Solo";
            }
        }
    }
}
