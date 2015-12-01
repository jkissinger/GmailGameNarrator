using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Narrator.Teams
{
    public class SoloTeam : Team
    {
        public override bool HaveIWon(Player player, Game game)
        {
            //I'm the sole survivor, I win
            if (player.IsAlive && game.GetLivingPlayers().Count == 1) return true;
            return false;
        }

        public SoloTeam()
        {
            Name = "Solo";
            KnowsTeammates = false;
            MinPercentage = 0;
            IsMajor = false;
        }
    }
}
