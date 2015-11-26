using System;
using GmailGameNarrator.Game.Teams;

namespace GmailGameNarrator.Game.Roles
{
    public class Sheeple : Role
    {
        public override string Name
        {
            get
            {
                return "Citizen";
            }
        }

        public override int MaxPercentage
        {
            get
            {
                return 100;
            }
        }

        public override int Prevalence
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

        public override string Instructions
        {
            get
            {
                return "At night, send a message with " + Name.b() + " in the body.  Nothing will happen, but this is to ensure everyone has a night action.";
            }
        }

        public override int NightActionPriority
        {
            get
            {
                return 0;
            }
        }
    }
}
