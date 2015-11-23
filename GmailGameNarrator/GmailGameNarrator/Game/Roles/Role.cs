using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game.Roles
{
    abstract class Role
    {
        public abstract string Name { get; }
        public abstract Team Team { get; }
        /// <summary>
        /// The maximum number of players with this role in a game.
        /// </summary>
        public abstract int MaxPlayers { get; }
        /// <summary>
        /// Likelihood of assigning this role, to make the more fun roles more likely to be picked.
        /// </summary>
        public abstract int Priority { get; }
        public abstract string Instructions { get; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj.ToString().Equals(ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public virtual string ValidateAction(Player player, Action action, Game game)
        {
            return "";
        }

        /// <summary>
        /// Determines if this player has met their win conditions.  By default checks team win conditions, but can be overriden if necessary.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public bool HaveIWon(Player player, Game game)
        {
            return Team.HaveIWon(player, game);
        }

        /// <summary>
        /// Processes any night actions for given player in given game.  By default does nothing.
        /// </summary>
        /// <param name="player">The player performing the action</param>
        /// <param name="game">The game player is playing</param>
        /// <returns></returns>
        public virtual string DoNightActions(Player player, Game game)
        {
            return "";
        }
    }
}
