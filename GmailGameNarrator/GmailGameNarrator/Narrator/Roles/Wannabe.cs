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
                return "At night, send a message with " + Name.b() + " as the body.   Nothing will happen, but this is to ensure everyone has a night action.";
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
                return 10;
            }
        }

        public override string Description
        {
            get
            {
                return "The Illuminati said you could be one of them if you got the rest of the town to vote to cast you out.  If that happens, you win.";
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
