using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game.Roles
{
    public abstract class Role
    {
        public abstract string Name { get; }
        public abstract Team Team { get; }
        /// <summary>
        /// The priority for night actions, between 0 and 4, first processed is 0, last 4.
        /// </summary>
        public virtual int NightActionPriority
        {
            get
            {
                return 4;
            }
        }
        /// <summary>
        /// Maximum percentage of players with this role on a team.
        /// </summary>
        public abstract int MaxPercentage { get; }
        /// <summary>
        /// Maximum number of players with this role in a game.
        /// </summary>
        public virtual int MaxPlayers
        {
            get
            {
                return 1000;
            }
        }
        /// <summary>
        /// Likelihood of assigning this role, to make the more fun roles more likely to be picked.  Must be at least 1.
        /// </summary>
        public abstract int Prevalence { get; }
        public abstract string Instructions { get; }

        public virtual void KilledBy(Role role) { }

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
        public virtual bool HaveIWon(Player player, Game game)
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

        /// <summary>
        /// Processes any day actions for given player in given game.  By default does nothing.
        /// </summary>
        /// <param name="player">The player performing the action</param>
        /// <param name="game">The game player is playing</param>
        /// <returns></returns>
        public virtual string DoDayActions(Player player, Game game)
        {
            return "";
        }

        //==========================
        //= Object overrides below =
        //==========================

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
    }
}
