using System;

namespace GmailGameNarrator.Narrator.Teams
{
    public class AntagonistTeam : Team
    {
        public override bool HaveIWon(Player player, Game game)
        {
            int numTeammates = 0;
            int numOpponents = 0;
            foreach(Player p in game.GetLivingPlayers())
            {
                //If there are any killing roles not on this team, we haven't won
                if (!p.Team.Equals(player.Team) && p.Role.IsAttacker) return false;
                //Count teammates and opponents
                if (p.Team.Equals(player.Team)) numTeammates++;
                else numOpponents++;
            }
            //If our team is greater than or equal to the opposition, then we have won
            if (numTeammates >= numOpponents) return true;
            else return false;
        }

        public AntagonistTeam()
        {
            Name = "Illuminati";
            KnowsTeammates = true;
            MinPercentage = 20;
            IsMajor = true;
        }
    }
}
