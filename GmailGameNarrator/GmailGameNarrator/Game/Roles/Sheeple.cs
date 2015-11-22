using System;
using GmailGameNarrator.Game.Teams;

namespace GmailGameNarrator.Game.Roles
{
    class Sheeple : Role
    {
        public override string Name
        {
            get
            {
                return "Citizen";
            }
        }

        public override int MaxPlayers
        {
            get
            {
                return 1000;
            }
        }

        public override int Priority
        {
            get
            {
                return 1;
            }
        }

        public override Team Team
        {
            get
            {
                return new SheepleTeam();
            }
        }
    }
}
