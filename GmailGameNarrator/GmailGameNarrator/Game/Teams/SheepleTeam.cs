namespace GmailGameNarrator.Game.Teams
{
    public class SheepleTeam : Team
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

        public override bool IsMajor
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get { return "Town"; }
        }

        public override bool HaveIWon(Player player, Game game)
        {
            foreach (Player p in game.Players)
            {
                if (p.IsAlive && p.Role.IsKiller && !p.Team.Equals(player.Team)) return false;
            }
            return true;
        }
    }
}
