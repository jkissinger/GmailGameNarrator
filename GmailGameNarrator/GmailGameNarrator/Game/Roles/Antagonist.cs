﻿using GmailGameNarrator.Game.Teams;
using System;

namespace GmailGameNarrator.Game.Roles
{
    class Antagonist : Role
    {
        public override string Name
        {
            get
            {
                return "Enlightened";
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
                return new AntagonistTeam();
            }
        }
    }
}
