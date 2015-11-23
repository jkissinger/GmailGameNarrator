namespace GmailGameNarrator.Game.Teams
{
    class AntagonistTeam : Team
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

        public override string ValidateAction(Player player, Action action, Game game)
        {
            string nomineeName = action.Parameter.GetTextAfter("vote ");
            Player nominee = game.GetPlayerByName(nomineeName);
            if (nominee == null) return nomineeName.b() + " is not a valid player in " + game.Title;
            else if (nominee.Team.Equals(player.Team)) return "You cannot vote for " + nomineeName.b() + ".  They are on your team!";
            else
            {
                Gmail.EnqueueMessage(player.Address, game.Subject, "Registered your " + player.Role.Name.b() + " vote for " + nomineeName.b() + ".");
            }
            return "";
        }
    }
}
