using GmailGameNarrator.Narrator.Roles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GmailGameNarrator.Narrator
{
    public class Player
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Name { get; }
        //TODO Add a variable or function that is just the email address in case players change their email client's name part way through
        public string Address { get; }
        public bool IsAlive { get; private set; }
        public bool IsProtected { get; set; }
        public Role Role { get; set; }
        public Team Team
        {
            get
            {
                return Role.Team;
            }
        }
        /*
        //May reimplement this at some point if a role has more than one action possible at a time
        //For now it just unnecessarily complicates things
        public ReadOnlyCollection<Action> Actions
        {
            get
            {
                return new ReadOnlyCollection<Action>(MyActions);
            }
        }
        private IList<Action> MyActions = new List<Action>();
        */
        public Action MyAction { get; private set; }

        public Player(string name, string address)
        {
            Name = name;
            Address = address.Trim().ToLowerInvariant();
            IsAlive = true;
        }

        /// <summary>
        /// Returns true if the attack succeeds, false otherwise.
        /// </summary>
        /// <param name="attackerRole">The role of the player making the attack</param>
        /// <param name="isDeadly">Whether or not the attack will kill the player</param>
        /// <returns>True if the attack succeeds, false otherwise.</returns>
        public bool Attack(Role attackerRole, bool isDeadly, bool indefensible)
        {
            if (!IsProtected || indefensible)
            {
                if (isDeadly)
                {
                    Role.KilledBy = attackerRole;
                    IsAlive = false;
                }
                return true;
            }
            return false;
        }

        public void Quit()
        {
            IsAlive = false;
        }

        public bool HaveIWon(Game game)
        {
            return Role.HaveIWon(this, game);
        }

        /// <summary>
        /// Adds a actions and votes to this player's actions.  Validation performed by <see cref="GameSystem.Vote(Player, Action, Game)"/>
        /// and <see cref="Role.AddAction(Player, Action, Game)"/> respectively.
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(Action action)
        {
            MyAction = action;
        }

        public string PerformActions(Game game)
        {
            string result = "";
            if (game.ActiveCycle == Game.Cycle.Night)
            {
                Role.PerformNightActions(this, game);
            }
            else Role.DoDayActions(this, game);
            return result;
        }

        public void Reset()
        {
            MyAction = null;
            IsProtected = false;
        }

        //==========================
        //= Object overrides below =
        //==========================
        public override string ToString()
        {
            return Address + " playing as " + Name;
        }

        public override bool Equals(object obj)
        {
            try
            {
                Player p = (Player)obj;
                if (p.Address.Equals(Address)) return true;
            }
            catch (InvalidCastException e)
            {
                log.Warn("Attempted to compare non player object to a player. Error: " + e.Message);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
    }
}
