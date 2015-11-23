using GmailGameNarrator.Game.Roles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GmailGameNarrator.Game
{
    class Player
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Name { get; }
        public string Address { get; }
        public bool IsAlive { get; set; }
        public Role Role { get; set; }
        public Team Team
        {
            get
            {
                return Role.Team;
            }
        }
        public Vote Vote { get; set; }
        public ReadOnlyCollection<Action> NightActions
        {
            get
            {
                return new ReadOnlyCollection<Action>(MyNightActions);
            }
        }
        private IList<Action> MyNightActions = new List<Action>();

        public Player(string name, string address)
        {
            Name = name;
            Address = address.Trim().ToLowerInvariant();
            IsAlive = true;
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
                MyNightActions.Add(action);
            }
            return result;
        }

        public string DoNightActions(Game game)
        {
            string result = Role.DoNightActions(this, game);
            return result;
        }

        public void ClearNightActions()
        {
            MyNightActions.Clear();
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
