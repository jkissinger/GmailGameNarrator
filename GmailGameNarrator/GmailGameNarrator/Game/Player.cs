using System;

namespace GmailGameNarrator.Game
{
    class Player
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Name { get; }
        public string Address { get; }
        public bool IsAlive { get; set; }

        public Player(string name, string address)
        {
            Name = name;
            Address = address.Trim().ToLowerInvariant();
        }

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
