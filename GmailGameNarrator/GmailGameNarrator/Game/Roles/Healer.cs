using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game.Roles
{
    public class Healer : Sheeple
    {
        public override string Name
        {
            get
            {
                return "Doctor";
            }
        }

        public override int MaxPercentage
        {
            get
            {
                return 25;
            }
        }

        public override int MaxPlayers
        {
            get
            {
                return 1;
            }
        }

        public override int Prevalence
        {
            get
            {
                return 5;
            }
        }

        public override int NightActionPriority
        {
            get
            {
                return 1;
            }
        }

        public override string ActionText
        {
            get
            {
                return "protect";
            }
        }

        public override string Instructions
        {
            get
            {
                return "At night, send a message with \"" + Name.b() + " protect player\" in the body.  You will do everything you can to prevent that player from dying that night.";
            }
        }

        public override string DoNightActions(Player player, Game game)
        {
            string nomineeName = player.Actions[0].Parameter.GetTextAfter("protect ");
            Player nominee = game.GetPlayerByName(nomineeName);
            nominee.IsProtected = true;
            Gmail.MessagePlayer(player, game, "You are protecting " + nominee.Name.b());
            game.Summary.AddEventLi(game.CycleTitle + " - " + player.Name.b() + " is protecting " + nominee.Name.b() + ".");
            return "";
        }

        private Player GetNominee(Player player, Game game)
        {
            string nomineeName = player.Actions[0].Parameter.GetTextAfter("protect ");
            Player newNominee = game.GetPlayerByName(nomineeName);
            return newNominee;
        }

        public override string ValidateAction(Player player, Action action, Game game)
        {
            string nomineeName = action.Parameter.GetTextAfter("protect ");
            Player nominee = game.GetPlayerByName(nomineeName);
            if (nominee == null) return nomineeName.b() + " is not a valid player in " + game.Title;
            else if (nominee.Equals(player)) return "You cannot protect yourself!";
            else if (!nominee.IsAlive) return "Choice rejected: " + nomineeName.b() + " is already dead!";
            else
            {
                Gmail.MessagePlayer(player, game, "Registered your night action to protect " + nomineeName.b() + ".");
            }
            return "";
        }
    }
}
