namespace GmailGameNarrator.Game.Teams
{
    class SheepleTeam : Team
    {
        public override bool KnowsTeammates
        {
            get
            {
                return false;
            }
        }

        public override int MinPercentComposition
        {
            get
            {
                return 60;
            }
        }

        public override string Name
        {
            get { return "Town"; }
        }
    }
}
