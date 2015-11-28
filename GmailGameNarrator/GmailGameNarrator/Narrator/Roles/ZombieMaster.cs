using GmailGameNarrator.Narrator.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Narrator.Roles
{
    class ZombieMaster : Role
    {
        public static int ChanceToTurn = 75;
        public override string Description
        {
            get
            {
                return "You are immmune to the zombie infection and have dubbed yourself the " + Name.b() + ".  Each night you may bite someone, there is a " + ChanceToTurn + "% chance they will turn into a zombie the next night, and a " + ChanceToTurn + "% chance they will bite someone else.";
            }
        }

        public override string Instructions
        {
            get
            {
                return "At night, send a message with \"" + ActionText.b() + " name".i() + "\" where " + "name".i() + " is the player you want to " + ActionText.b() + ".  There is a " + ChanceToTurn + "% chance they will turn into a zombie at the beginning of the next night.";
            }
        }

        public override int MaxPercentage
        {
            get
            {
                return 10;
            }
        }

        public override string Name
        {
            get
            {
                return "Zombie Master";
            }
        }

        public override int Prevalence
        {
            get
            {
                return 10;
            }
        }

        public override Team Team
        {
            get
            {
                return new SoloTeam();
            }
        }

        public override bool IsKiller
        {
            get
            {
                return true;
            }
        }

        public override bool IsInfectionImmune
        {
            get
            {
                return true;
            }
        }

        public override string ActionText
        {
            get
            {
                return "bite";
            }
        }

        public override int NightActionPriority
        {
            get
            {
                return 3;
            }
        }

        /// <summary>
        /// Returns true if this player is the only remaining player alive.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public override bool HaveIWon(Player player, Game game)
        {
            if (game.GetLivingPlayers().Count == 1 && player.IsAlive) return true;
            return false;
        }

        public override string DoNightActions(Player player, Game game)
        {
            string victimName = player.Actions[0].Parameter.GetTextAfter(ActionText);
            Player nominee = game.GetPlayerByName(victimName);
            string message = "";
            string result = "";
            if (nominee.IsProtected)
            {
                message += "You were unable to bite " + nominee.Name.b() + " because someone was protecting them!";
                result = " tried to bite " + nominee.Name.b() + " But they were protected.";
            }
            else
            {
                nominee.BittenRound = game.RoundCounter;
                message += "You bit " + nominee.Name.b() + "! There is a " + ChanceToTurn + "% chance they will turn into a zombie tomorrow night!";
                result = " bit " + nominee.Name.b() + ".";
            }
            game.Summary.AddEventLi(game.CycleTitle + " - " + player.Name.b() + result);
            Gmail.MessagePlayer(player, game, message);
            return "";
        }

        public override string ValidateAction(Player player, Action action, Game game)
        {
            string nomineeName = action.Parameter.GetTextAfter(ActionText);
            Player nominee = game.GetPlayerByName(nomineeName);
            if (nominee == null) return nomineeName.b() + " is not a valid player in " + game.Title;
            else if (nominee.Equals(player)) return "You cannot bite yourself!";
            else if (!nominee.IsAlive) return "Cannot bite " + nomineeName.b() + ", they are already dead!";
            else
            {
                Gmail.MessagePlayer(player, game, "Registered your " + player.Role.Name.b() + " action to " + ActionText.b() + " " + nomineeName.i() + ".");
            }
            return "";
        }
    }
}
