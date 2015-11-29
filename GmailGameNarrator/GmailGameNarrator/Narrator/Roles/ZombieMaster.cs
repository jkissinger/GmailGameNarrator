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

        public ZombieMaster()
        {
            Name = "Zombie Master";
            Team = new SoloTeam();
            ActionText = "Bite";
            Description = "You are immmune to the zombie infection and have dubbed yourself the " + Name.b() + ".  Each night you may bite someone, there is a " + ChanceToTurn + "% chance they will turn into a zombie the next night, and a " + ChanceToTurn + "% chance they will bite someone else.";
            Instructions = "At night, send a message with \"" + ActionText.b() + " name".i() + "\" where " + "name".i() + " is the player you want to " + ActionText.b() + ".  There is a " + ChanceToTurn + "% chance they will turn into a zombie at the beginning of the next night.";
            NightActionPriority = 3;
            MaxPercentage = 20;
            Prevalence = 10;
            IsKiller = true;
            IsInfectionImmune = true;
        }

        /// <summary>
        /// Returns true if this player is the only remaining player alive.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public override bool HaveIWon(Player player, Game game)
        {
            //TODO Could result in a stalemate between zombie masters, do they "band together"?
            if (game.GetLivingPlayers().Count == 1 && player.IsAlive) return true;
            return false;
        }

        public override string DoNightActions(Player player, Game game)
        {
            string victimName = player.Actions[0].Parameter;
            Player nominee = game.GetPlayer(victimName, "");
            string message = "";
            string result = "";
            if (nominee.Attack(this, false))
            {

                nominee.BittenRound = game.RoundCounter;
                message += "You bit " + nominee.Name.b() + "! There is a " + ChanceToTurn + "% chance they will turn into a zombie tomorrow night!";
                result = " bit " + nominee.Name.b() + ".";
            }
            else
            {
                message += "You were unable to bite " + nominee.Name.b() + " because someone was protecting them!";
                result = " tried to bite " + nominee.Name.b() + " But they were protected.";
            }
            game.Summary.AddEventLi(game.CycleTitle + " - " + player.Name.b() + result);
            Gmail.MessagePlayer(player, game, message);
            return "";
        }

        public override string ValidateAction(Player player, Action action, Game game)
        {
            string nomineeName = action.Parameter;
            Player nominee = game.GetPlayer(nomineeName, "");
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
