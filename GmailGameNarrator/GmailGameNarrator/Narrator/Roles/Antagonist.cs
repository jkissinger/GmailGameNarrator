using GmailGameNarrator.Narrator.Teams;
using System;
using System.Collections.Generic;

namespace GmailGameNarrator.Narrator.Roles
{
    public class Antagonist : Role
    {
        public Antagonist()
        {
            Name = "Enlightened";
            Team = new AntagonistTeam();
            ActionText = "vote";
            Description = "You've had enough of the Sheeple eating your food and using your shelter.  With the other members of your team, you vote and then cast out one of the Sheeple from the enclave, knowing they will die quickly outside.";
            Instructions = "At night, vote for a player to cast out, like this \"" + ActionText.b() + " name".i() + "\" where " + "name".i() + " is the player you want to cast out.  You must have a consensus with your fellow " + Team + " or no one will be cast out!"; 
            NightActionPriority = 2;
            MaxPercentage = 40;
            Prevalence = 1;
            IsAttacker = true;
            IsInfectionImmune = false;
            Assignable = true;
        }

        public override void PerformNightActions(Player player, Game game)
        {
            if (!player.IsAlive) return;
            Player nominee = null;
            List<Player> teammates = game.GetLivingPlayersOnMyTeam(player);
            foreach(Player t in teammates)
            {
                Player newNominee = t.MyAction.Target;
                if (nominee == null) nominee = newNominee;
                else if (!nominee.Equals(newNominee))
                {
                    List<string> nominations = GetNominations(teammates, game);
                    string message = game.CycleTitle + " - The " + Team.Name.b() + " didn't have a consensus! You cast out nobody!<br />" + "Team voting results:<br />" + nominations.HtmlBulletList();
                    game.Summary.AddUniqueEvent(message.li());
                    Gmail.MessagePlayer(player, game, message);

                    return;
                }
            }
            if (nominee.Attack(this, true, false))
            {
                string msg = game.CycleTitle + " - The " + Team.Name.b() + " cast out " + nominee.Name.b();
                game.Summary.AddUniqueEvent(msg.li());
                Gmail.MessagePlayer(player, game, msg);
                game.NightEvents.Add(nominee.Name.b() + " was caught in a house fire. Their burnt body was found near the stove, making popcorn. Grease fires are " + "so".b() + " dangerous.");
            }
            else
            {
                string msg = game.CycleTitle + " - The " + Team.Name.b() + " cast out " + nominee.Name.b() + " but they survived.";
                game.Summary.AddUniqueEvent(msg.li());
                Gmail.MessagePlayer(player, game, msg);
                game.NightEvents.Add(nominee.Name.b() + "'s house burnt down. But somehow they survived!");
            }
        }

        private List<string> GetNominations(List<Player> teammates, Game game)
        {
            List<string> nominations = new List<string>();
            foreach (Player teammate in teammates)
            {
                Player newNominee2 = teammate.MyAction.Target;
                nominations.Add(teammate.Name.b() + " voted for: " + newNominee2.Name.b());
            }
            return nominations;
        }

        public override string AddAction(Player player, Action action, Game game)
        {
            Player nominee = action.Target;
            if (nominee != null && nominee.Team.Equals(player.Team)) return "You cannot vote for " + nominee.Name.b() + ".  They are on your team!";
            return base.AddAction(player, action, game);
        }
    }
}
