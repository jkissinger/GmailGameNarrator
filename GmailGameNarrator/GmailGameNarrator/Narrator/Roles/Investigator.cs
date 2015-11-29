using GmailGameNarrator.Narrator.Teams;

namespace GmailGameNarrator.Narrator.Roles
{
    class Investigator : Sheeple
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
            IsKiller = false;
            IsInfectionImmune = false;
            Assignable = true;
        }

        public override string DoNightActions(Player player, Game game)
        {
            string nomineeName = player.Actions[0].Parameter;
            Player nominee = game.GetPlayerByName(nomineeName);
            Gmail.MessagePlayer(player, game, "You have investigated " + nominee.Name.b() + " and determined their allegiance is with the " + nominee.Team.b());
            game.Summary.AddEventLi(game.CycleTitle + " - " + player.Name.b() + " investigated " + nominee.Name.b() + ". And found out their allegiance is with the " + nominee.Team.b());
            return "";
        }

        public override string ValidateAction(Player player, Action action, Game game)
        {
            string nomineeName = action.Parameter;
            Player nominee = game.GetPlayerByName(nomineeName);
            if (nominee == null) return nomineeName.b() + " is not a valid player in " + game.Title;
            else if (nominee.Equals(player)) return "You cannot " + ActionText + " yourself!";
            else if (!nominee.IsAlive) return "Choice rejected: " + nomineeName.b() + " is already dead!";
            else
            {
                Gmail.MessagePlayer(player, game, "Registered your night action to " + ActionText  + " " + nomineeName.b() + ".");
            }
            return "";
        }
    }
}
