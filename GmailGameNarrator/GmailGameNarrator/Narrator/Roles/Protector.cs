using GmailGameNarrator.Narrator.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Narrator.Roles
{
    public class Protector : Sheeple
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
            IsKiller = false;
            IsInfectionImmune = false;
            Assignable = true;
        }

        public override string DoNightActions(Player player, Game game)
        {
            string nomineeName = player.Actions[0].Parameter;
            Player nominee = game.GetPlayerByName(nomineeName);
            nominee.IsProtected = true;
            Gmail.MessagePlayer(player, game, "You are protecting " + nominee.Name.b());
            game.Summary.AddEventLi(game.CycleTitle + " - " + player.Name.b() + " is protecting " + nominee.Name.b() + ".");
            return "";
        }

        private Player GetNominee(Player player, Game game)
        {
            string nomineeName = player.Actions[0].Parameter.GetTextAfter(ActionText);
            Player newNominee = game.GetPlayerByName(nomineeName);
            return newNominee;
        }

        public override string ValidateAction(Player player, Action action, Game game)
        {
            string nomineeName = action.Parameter;
            Player nominee = game.GetPlayerByName(nomineeName);
            if (nominee == null) return nomineeName.b() + " is not a valid player in " + game.Title;
            else if (nominee.Equals(player)) return "You cannot protect yourself!";
            else if (!nominee.IsAlive) return "Choice rejected: " + nomineeName.b() + " is already dead!";
            else
            {
                Gmail.MessagePlayer(player, game, "Registered your night action to " + ActionText + " " + nomineeName.b() + ".");
            }
            return "";
        }
    }
}
