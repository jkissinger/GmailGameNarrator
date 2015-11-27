using System;
using GmailGameNarrator.Narrator.Teams;

namespace GmailGameNarrator.Narrator.Roles
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
                return 80;
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

        public override string Description
        {
            get
            {
                return "You are a Citizen of the Town, an enclave.  You know there are illuminati amongst you, trying to cast out the lesser folk such as yourself.  Find your allies and stop them.";
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
