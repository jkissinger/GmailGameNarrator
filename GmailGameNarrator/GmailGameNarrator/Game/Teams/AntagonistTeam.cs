namespace GmailGameNarrator.Game.Teams
{
    public class AntagonistTeam : Team
    {
        public override string Name
        {
            get { return "Illuminati"; }
        }

        public override int MinPercentComposition
        {
            get
            {
                return 20;
            }
        }

        public override bool KnowsTeammates
        {
            get
            {
                return true;
            }
        }

        public override bool IsMajor
        {
            get
            {
                return true;
            }
        }
    }
}
