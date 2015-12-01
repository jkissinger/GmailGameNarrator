using GmailGameNarrator.Narrator.Teams;

namespace GmailGameNarrator.Narrator.Roles
{
    class Investigator : Role
    {
        public Investigator()
        {
            Name = "Investigator";
            Team = new SheepleTeam();
            ActionText = "Check";
            Description = "Before the world came to an end, you worked as a private investigator.  Using your deductive skills, you analyze one person each night and are usually able to determine their allegiance.";
            Instructions = "At night, send a message with \"" + ActionText + " player\" in the body.  You will look into that player's affairs closely and by morning should have an idea of their allegiance.";
            NightActionPriority = 3;
            MaxPercentage = 25;
            Prevalence = 5;
            IsAttacker = false;
            IsInfectionImmune = false;
            Assignable = true;
        }

        public override void PerformNightActions(Player player, Game game)
        {
            if (player.IsAlive)
            {
                Player nominee = player.MyAction.Target;
                Gmail.MessagePlayer(player, game, "You have investigated " + nominee.Name.b() + " and determined their allegiance is with the " + nominee.Team.b());
                game.Summary.AddEventLi(game.CycleTitle + " - " + player.Name.b() + " investigated " + nominee.Name.b() + ". And found out their allegiance is with the " + nominee.Team.b());
            }
        }
    }
}
