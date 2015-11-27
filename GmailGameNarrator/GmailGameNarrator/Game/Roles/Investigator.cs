namespace GmailGameNarrator.Game.Roles
{
    class Investigator : Sheeple
    {
        public override string Name
        {
            get
            {
                return "Investigator";
            }
        }

        public override int MaxPercentage
        {
            get
            {
                return 25;
            }
        }

        public override int MaxPlayers
        {
            get
            {
                return 3;
            }
        }

        public override int Prevalence
        {
            get
            {
                return 5;
            }
        }

        public override int NightActionPriority
        {
            get
            {
                return 3;
            }
        }

        public override string ActionText {
            get
            {
                return "check";
            }
        }

        public override string Instructions
        {
            get
            {
                return "At night, send a message with \"" + Name.b() + " " + ActionText + " player\" in the body.  You will look into that player's affairs closely and by morning should have an idea of their allegiance.";
            }
        }

        public override string DoNightActions(Player player, Game game)
        {
            string nomineeName = player.Actions[0].Parameter.GetTextAfter(ActionText + " ");
            Player nominee = game.GetPlayerByName(nomineeName);
            Gmail.EnqueueMessage(player.Address, game.Subject, "You have investigated " + nominee.Name.b() + " and determined their allegiance is with the " + nominee.Team.b());
            game.Summary.AddEvent((player.Name.b() + " investigated " + nominee.Name.b() + ". And found out their allegiance is with the " + nominee.Team.b()).tag("li"));
            return "";
        }

        private Player GetNominee(Player player, Game game)
        {
            string nomineeName = player.Actions[0].Parameter.GetTextAfter(ActionText + " ");
            Player newNominee = game.GetPlayerByName(nomineeName);
            return newNominee;
        }

        public override string ValidateAction(Player player, Action action, Game game)
        {
            string nomineeName = action.Parameter.GetTextAfter(ActionText + " ");
            Player nominee = game.GetPlayerByName(nomineeName);
            if (nominee == null) return nomineeName.b() + " is not a valid player in " + game.Title;
            else if (nominee.Equals(player)) return "You cannot " + ActionText + " yourself!";
            else if (!nominee.IsAlive) return "Choice rejected: " + nomineeName.b() + " is already dead!";
            else
            {
                Gmail.EnqueueMessage(player.Address, game.Subject, "Registered your night action to " + ActionText  + " " + nomineeName.b() + ".");
            }
            return "";
        }
    }
}
