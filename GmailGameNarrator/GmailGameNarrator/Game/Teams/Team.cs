namespace GmailGameNarrator.Game
{
    abstract class Team
    {
        abstract public string Name { get; }
        abstract public bool KnowsTeammates { get; }
        abstract public int MinPercentage { get; }

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
