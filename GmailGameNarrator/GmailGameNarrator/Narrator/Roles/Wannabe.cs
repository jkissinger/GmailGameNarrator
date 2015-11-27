using GmailGameNarrator.Narrator.Teams;
using System;

namespace GmailGameNarrator.Narrator.Roles
{
    public class Wannabe : Role
    {
        public override string Instructions
        {
            get
            {
                return "You are a " + Name.b() + ". You want to be cast out.  If you convince the rest of the players to cast you out, you win.";
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
                return "Wannabe";
            }
        }

        public override int Prevalence
        {
            get
            {
                return 3;
            }
        }

        public override Team Team
        {
            get
            {
                return new SoloTeam();
            }
        }

        private bool Winner = false;

        /// <summary>
        /// Determines if this player has met their win conditions.  Wannabe's win if they are voted for by a majority of the town to be removed from the game.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public override bool HaveIWon(Player player, Game game)
        {
            if (Winner) Gmail.MessageAllPlayers(game, player.Name.b() +" won because you voted for them and they were a " + Name.b());
            return Winner;
        }

        public override void KilledBy(Role role)
        {
            //If I wasn't killed by a role, I was killed by the town vote, I win
            if (role == null) Winner = true;
        }
    }
}
