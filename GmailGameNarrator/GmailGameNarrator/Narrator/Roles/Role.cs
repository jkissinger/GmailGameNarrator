using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Narrator.Roles
{
    public abstract class Role
    {
        public string Name { get; protected set; }
        public Team Team { get; protected set; }
        /// <summary>
        /// The text that identifies this role's actions.  Default: the name of the role.
        /// </summary>
        public string ActionText { get; protected set; }
        public string Description { get; protected set; }
        public string Instructions { get; protected set; }
        /// <summary>
        /// The priority for night actions, 0 first, 4 last.  Default is 4.
        /// <para />0=Preprocesing; 1=Protecters/Blockers; 2=Killers/Attackers; 3=Informational; 4=Postprocessing/No actions
        /// </summary>
        public int NightActionPriority { get; protected set; }
        /// <summary>
        /// Maximum percentage of players with this role on a team. Default 10.
        /// </summary>
        public int MaxPercentage { get; protected set; }
        /// <summary>
        /// Likelihood of assigning this role, to make the more fun roles more likely to be picked.  Must be > 0, default 1.
        /// </summary>
        public int Prevalence { get; protected set; }
        /// <summary>
        /// Whether or not this role can attack other players.  Default is false.
        /// </summary>
        public bool IsAttacker { get; protected set; }
        /// <summary>
        /// Whether or not this role is immune to the zombie infection.  Default is false.
        /// </summary>
        public bool IsInfectionImmune { get; protected set; }
        /// <summary>
        /// Whether or not this role can be assigned at the beginning of the game.  Default is true.
        /// </summary>
        public bool Assignable { get; protected set; }
        /// <summary>
        /// The role that killed the player with this role.  Null if by day vote.
        /// </summary>
        public Role KilledBy { get; set; }

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
        /// Player performing the action, action being performed, and the game this is all taking place in.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="action"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public virtual string AddAction(Player player, Action action, Game game)
        {
            Player nominee = action.Target;
            if (nominee == null) return action.Text.b() + " is not a valid player in " + game.Title;
            else if (nominee.Equals(player)) return "You cannot " + ActionText + " yourself!";
            else if (!nominee.IsAlive) return "Choice rejected: " + nominee.Name.b() + " is already dead!";
            else
            {
                player.AddAction(action);
                Gmail.MessagePlayer(player, game, "Registered your night action to " + ActionText + " " + nominee.Name.b() + ".");
            }
            return "";
        }

        /// <summary>
        /// Processes any night actions for given player in given game.
        /// </summary>
        /// <param name="player">The player performing the action</param>
        /// <param name="game">The game player is playing</param>
        /// <returns></returns>
        public abstract void PerformNightActions(Player player, Game game);

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
