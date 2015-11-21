using System;

namespace GmailGameNarrator.Game
{
    class MessageParser
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static MessageParser instance;

        private MessageParser() { }

        public static MessageParser Instance
        {
            get
            {
                if (instance == null)
                {
                    log.Info("Initializing MessageParser instance.");
                    instance = new MessageParser();
                }
                return instance;
            }
        }

        public void ParseMessage(SimpleMessage message)
        {
            string result = "";
            if (message.Subject.Equals("New Game", StringComparison.InvariantCultureIgnoreCase)) result = NewGame(message);

            if (!String.IsNullOrEmpty(result)) Gmail.EnqueueEmail(message.From, "Invalid request", result);
        }

        private string NewGame(SimpleMessage message)
        {
            Player overlord;
            try
            {
                string name = message.BodyAsLines()[0];
                string email = message.FromEmailAddress();
                overlord = new Player(name, email);
            }
            catch (Exception e)
            {
                log.Error("Invalid request. ", e);
                return "Malformed body or message please try again.";
            }
            Game newGame = NarratorSystem.Instance.NewGame(overlord);
            if (newGame == null)
            {
                Game g = NarratorSystem.Instance.GetGameByPlayer(overlord);
                return "You are already playing in " + g.overlord.name + "'s game " + g.Id + "  , you may only play one game at a time.";
            }

            return "";
        }
    }
}
