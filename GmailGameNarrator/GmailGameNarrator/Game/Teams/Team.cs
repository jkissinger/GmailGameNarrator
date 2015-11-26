namespace GmailGameNarrator.Game
{
    public abstract class Team
    {
        public abstract string Name { get; }
        public abstract bool KnowsTeammates { get; }
        /// <summary>
        /// The minimum percentage of players that must be on the team to be effective.
        /// </summary>
        public abstract int MinPercentComposition { get; }
        public virtual bool IsMajor
        {
            get
            {
                return false;
            }
        }

        public bool HaveIWon(Player player, Game game)
        {
            foreach (Player p in game.Players)
            {
                if (p.IsAlive && !p.Team.Equals(player.Team)) return false;
            }
            return true;
        }

        public virtual string ValidateAction(Player player, Action action, Game game)
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
