using GmailGameNarrator.Narrator.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Narrator.Roles
{
    public class Protector : Role
    {
        public Protector()
        {
            Name = "Bodyguard";
            Team = new SheepleTeam();
            ActionText = "Protect";
            Description = "You live in the enclave with the rest of the Citizens, but you are a bodyguard by night.  You stake out someone's house and spend all night protecting them.";
            Instructions = "At night, send a message with \"" + ActionText.b() + " player\" in the body.  You will do everything you can to prevent that player from dying that night.";
            NightActionPriority = 1;
            MaxPercentage = 20;
            Prevalence = 8;
            IsAttacker = false;
            IsInfectionImmune = false;
            Assignable = true;
        }

        public override void PerformNightActions(Player player, Game game)
        {
            if (player.IsAlive)
            {
                Player nominee = player.MyAction.Target;
                nominee.IsProtected = true;
                Gmail.MessagePlayer(player, game, "You are protecting " + nominee.Name.b());
                game.Summary.AddEventLi(game.CycleTitle + " - " + player.Name.b() + " is protecting " + nominee.Name.b() + ".");
            }
        }
    }
}
