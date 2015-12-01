namespace GmailGameNarrator.Narrator.Teams
{
    public class SheepleTeam : Team
    {
        public override bool HaveIWon(Player player, Game game)
        {
            foreach (Player p in game.GetLivingPlayers())
            {
                if (p.Role.IsAttacker && !p.Team.Equals(player.Team)) return false;
            }
            return true;
        }

        public SheepleTeam()
        {
            Name = "Town";
            KnowsTeammates = false;
            MinPercentage = 50;
            IsMajor = true;
        }
    }
}
