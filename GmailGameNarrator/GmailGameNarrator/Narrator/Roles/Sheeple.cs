using System;
using GmailGameNarrator.Narrator.Teams;

namespace GmailGameNarrator.Narrator.Roles
{
    public class Sheeple : Role
    {
        public Sheeple()
        {
            Name = "Citizen";
            Team = new SheepleTeam();
            ActionText = Name;
            Description = "You are a Citizen of the Town, an enclave.  You know there are illuminati amongst you, trying to cast out the lesser folk such as yourself.  Find your allies and stop them.";
            Instructions = "At night, send a message with " + Name.b() + " in the body.  Nothing will happen, but this is to ensure everyone has a night action.";
            NightActionPriority = 4;
            MaxPercentage = 80;
            Prevalence = 1;
            IsKiller = false;
            IsInfectionImmune = false;
            Assignable = true;
        }
    }
}
