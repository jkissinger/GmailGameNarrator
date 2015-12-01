using GmailGameNarrator.Narrator.Teams;
using System;

namespace GmailGameNarrator.Narrator.Roles
{
    public class Wannabe : Role
    {
        public Wannabe()
        {
            Name = "Wannabe";
            Team = new SoloTeam();
            ActionText = Name;
            Description = "The Illuminati said you could be one of them if you got the rest of the town to vote to cast you out.  If that happens, you win.";
            Instructions = "At night, send a message with " + Name.b() + " as the body.   Nothing will happen, but this is to ensure everyone has a night action.";
            NightActionPriority = 4;
            MaxPercentage = 20;
            Prevalence = 10;
            IsAttacker = false;
            IsInfectionImmune = false;
            Assignable = true;
        }

        /// <summary>
        /// Determines if this player has met their win conditions.  Wannabe's win if they are voted for by a majority of the town to be removed from the game.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public override bool HaveIWon(Player player, Game game)
        {
            if (!player.IsAlive && KilledBy == null)
            {
                Gmail.MessageAllPlayers(game, player.Name.b() + " won because you voted for them and they were a " + Name.b());
                return true;
            }
            return false;
        }

        public override void PerformNightActions(Player player, Game game) { }
    }
}
