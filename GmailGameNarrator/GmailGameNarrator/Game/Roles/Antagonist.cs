using GmailGameNarrator.Game.Teams;
using System;
using System.Collections.Generic;

namespace GmailGameNarrator.Game.Roles
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
                return "At night, vote for a player to cast out, like this \"" + Name.b() + " vote " + "name".i() + "\" where " + "name".i() + " is the player you want to cast out.";
            }
        }

        public override int MaxPercentage
        {
            get
            {
                return 1000;
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
                    List<string> nominations = new List<string>();
                    foreach(Player t2 in teammates)
                    {
                        Player newNominee2 = GetNominee(t, game);
                        nominations.Add(t2.Name.b() + " voted for: " + newNominee2.Name.b());
                    }
                    Gmail.EnqueueMessage(player.Address, game.Subject, "The " + Team.Name.b() + " didn't have a consensus! You cast out nobody!<br /><br />Team voting results:<br />" + nominations.HtmlBulletList());

                    return "";
                }
            }
            nominee.Kill(this);
            Gmail.EnqueueMessage(player.Address, game.Subject, Team.Name.b() + " cast out " + nominee.Name.b());
            return nominee.Name.b() + " was caught in a house fire. Their burnt body was found near the stove, making popcorn. Grease fires are " + "so".b() + " dangerous.";
        }

        private Player GetNominee(Player player, Game game)
        {
            string nomineeName = player.Actions[0].Parameter.GetTextAfter("vote ");
            Player newNominee = game.GetPlayerByName(nomineeName);
            return newNominee;
        }

        public override string ValidateAction(Player player, Action action, Game game)
        {
            string nomineeName = action.Parameter.GetTextAfter("vote ");
            Player nominee = game.GetPlayerByName(nomineeName);
            if (nominee == null) return nomineeName.b() + " is not a valid player in " + game.Title;
            else if (nominee.Team.Equals(player.Team)) return "You cannot vote for " + nomineeName.b() + ".  They are on your team!";
            else if (!nominee.IsAlive) return "Vote rejected. " + nomineeName.b() + " is already dead!";
            else
            {
                Gmail.EnqueueMessage(player.Address, game.Subject, "Registered your " + player.Role.Name.b() + " vote for " + nomineeName.b() + ".");
            }
            return "";
        }
    }
}
