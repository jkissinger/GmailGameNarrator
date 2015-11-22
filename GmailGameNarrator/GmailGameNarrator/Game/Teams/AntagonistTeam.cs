using System;

namespace GmailGameNarrator.Game.Teams
{
    class AntagonistTeam : Team
    {
        public override string Name
        {
            get { return "Illuminati"; }
        }

        public override int MinPercentage
        {
            get
            {
                return 20;
            }
        }

        public override bool KnowsTeammates
        {
            get
            {
                return true;
            }
        }
    }
}
