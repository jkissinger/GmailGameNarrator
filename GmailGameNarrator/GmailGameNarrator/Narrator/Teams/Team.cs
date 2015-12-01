namespace GmailGameNarrator.Narrator
{
    public abstract class Team
    {
        public string Name { get; protected set; }
        public bool KnowsTeammates { get; protected set; }
        /// <summary>
        /// The minimum percentage of players that must be on the team to be effective.
        /// </summary>
        public int MinPercentage { get; protected set; }
        public bool IsMajor { get; protected set; }

        public abstract bool HaveIWon(Player player, Game game);
        public virtual string ValidateAction(Player player, Action action, Game game)
        {
            return "";
        }

        public Team()
        {
            //Name = 
            KnowsTeammates = false;
            MinPercentage = 1;
            IsMajor = false;

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
