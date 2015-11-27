using GmailGameNarrator.Game.Roles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GmailGameNarrator.Game
{
    public class Player
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Name { get; }
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
        public Vote Vote { get; set; }
        //GAME Switch this to Actions, and then have each role check the game's cycle, to validate.  Then have "DoNightActions" kick off every cycle, again checking the cycle in the role.
        public ReadOnlyCollection<Action> Actions
        {
            get
            {
                return new ReadOnlyCollection<Action>(MyActions);
            }
        }
        private IList<Action> MyActions = new List<Action>();

        public Player(string name, string address)
        {
            Name = name;
            Address = address.Trim().ToLowerInvariant();
            IsAlive = true;
        }

        public bool Kill(Role killerRole)
        {
            if (!IsProtected)
            {
                Role.KilledBy(killerRole);
                IsAlive = false;
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

        public string AddNightAction(Action action, Game game)
        {
            string result = Role.ValidateAction(this, action, game);
            if (String.IsNullOrEmpty(result))
            {
                MyActions.Add(action);
            }
            return result;
        }

        public string DoActions(Game game)
        {
            string result = "";
            if (game.ActiveCycle == Game.Cycle.Night) Role.DoNightActions(this, game);
            if (game.ActiveCycle == Game.Cycle.Day) Role.DoDayActions(this, game);
            return result;
        }

        public void ClearNightActions()
        {
            MyActions.Clear();
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
