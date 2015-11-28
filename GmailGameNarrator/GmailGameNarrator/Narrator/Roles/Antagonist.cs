using GmailGameNarrator.Narrator.Teams;
using System;
using System.Collections.Generic;

namespace GmailGameNarrator.Narrator.Roles
{
    public class Antagonist : Role
    {
        public override string Name
        {
            get
            {
                return "Enlightened";
            }
        }

        public override string Instructions
        {
            get
            {
                return "At night, vote for a player to cast out, like this \"" + ActionText.b() + " name".i() + "\" where " + "name".i() + " is the player you want to cast out.  You must have a consensus with your fellow " + Team + " or no one will be cast out!";
            }
        }

        public override int MaxPercentage
        {
            get
            {
                return 40;
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
                return new AntagonistTeam();
            }
        }

        public override int NightActionPriority
        {
            get
            {
                return 3;
            }
        }

        public override bool IsKiller
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
                return "vote";
            }
        }

        public override string Description
        {
            get
            {
                return "You've had enough of the Sheeple eating your food and using your shelter.  With the other members of your team, you vote and then cast out one of the Sheeple from the enclave, knowing they will die quickly outside.";
            }
        }

        public override string DoNightActions(Player player, Game game)
        {
            Player nominee = null;
            List<Player> teammates = game.GetLivingPlayersOnMyTeam(player);
            foreach(Player t in teammates)
            {
                Player newNominee = GetNominee(t, game);
                if (nominee == null) nominee = newNominee;
                else if (!nominee.Equals(newNominee))
                {
                    List<string> nominations = GetNominations(teammates, game);
                    string message = game.CycleTitle + " - The " + Team.Name.b() + " didn't have a consensus! You cast out nobody!<br />" + "Team voting results:<br />" + nominations.HtmlBulletList();
                    game.Summary.AddUniqueEvent(message.li());
                    Gmail.MessagePlayer(player, game, message);

                    return "";
                }
            }
            if (nominee.Kill(this))
            {
                string msg = game.CycleTitle + " - The " + Team.Name.b() + " cast out " + nominee.Name.b();
                game.Summary.AddUniqueEvent(msg.li());
                Gmail.MessagePlayer(player, game, msg);
                return nominee.Name.b() + " was caught in a house fire. Their burnt body was found near the stove, making popcorn. Grease fires are " + "so".b() + " dangerous.";
            }
            else
            {
                string msg = game.CycleTitle + " - The " + Team.Name.b() + " cast out " + nominee.Name.b() + " but they survived.";
                game.Summary.AddUniqueEvent(msg.li());
                Gmail.MessagePlayer(player, game, msg);
                return nominee.Name.b() + "'s house burnt down. But somehow they survived!";
            }
        }

        private List<string> GetNominations(List<Player> teammates, Game game)
        {
            List<string> nominations = new List<string>();
            foreach (Player teammate in teammates)
            {
                Player newNominee2 = GetNominee(teammate, game);
                nominations.Add(teammate.Name.b() + " voted for: " + newNominee2.Name.b());
            }
            return nominations;
        }

        private Player GetNominee(Player player, Game game)
        {
            string nomineeName = player.Actions[0].Parameter.GetTextAfter(ActionText);
            Player newNominee = game.GetPlayerByName(nomineeName);
            return newNominee;
        }

        public override string ValidateAction(Player player, Action action, Game game)
        {
            string nomineeName = action.Parameter.GetTextAfter(ActionText);
            Player nominee = game.GetPlayerByName(nomineeName);
            if (nominee == null) return nomineeName.b() + " is not a valid player in " + game.Title;
            else if (nominee.Team.Equals(player.Team)) return "You cannot vote for " + nomineeName.b() + ".  They are on your team!";
            else if (!nominee.IsAlive) return "Vote rejected. " + nomineeName.b() + " is already dead!";
            else
            {
                Gmail.MessagePlayer(player, game, "Registered your " + player.Role.Name.b() + " vote for " + nomineeName.b() + ".");
            }
            return "";
        }
    }
}
