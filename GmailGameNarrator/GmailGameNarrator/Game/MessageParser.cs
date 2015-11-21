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
            if (message.NiceSubject().Equals("new game")) result = NewGame(message);
            if (message.NiceSubject().StartsWith("game ") || message.NiceSubject().StartsWith("re: game ")) result = ParseGameAction(message);

            if (!String.IsNullOrEmpty(result)) Gmail.EnqueueMessage(message.From, "Invalid request", result);
        }

        private string NewGame(SimpleMessage message)
        {
            Player overlord;
            try
            {
                string name = message.BodyAsLines()[0];
                string address = message.FromAddress();
                overlord = new Player(name, address);
            }
            catch (Exception e)
            {
                log.Error("Invalid request. ", e);
                return "Malformed body or message please try again.";
            }
            Game newGame = GameSystem.Instance.NewGame(overlord);
            if (newGame == null)
            {
                Game g = GameSystem.Instance.GetGameByPlayer(overlord);
                return "You are already playing in " + g.overlord.name + "'s game " + g.Id + "  , you may only play one game at a time.";
            }

            return "";
        }

        private string ParseGameAction(SimpleMessage message)
        {
            string subject = message.NiceSubject();
            
            int id;
            try {
                string strId = StringX.GetTextAfter(subject, "game ");
                id = Int32.Parse(strId);
            }
            catch (Exception e)
            {
                log.Warn("Malformed subject: " + subject + " from " + message.FromAddress() + " Error: " + e.Message);
                return "Invalid syntax in your subject, should be \"Game #\" where # is your game id.";
            }

            GameSystem gameSystem = GameSystem.Instance;

            string address = message.FromAddress();
            Player player = gameSystem.GetPlayerByAddress(address);
            if(player == null)
            {
                try
                {
                    string name = message.BodyAsLines()[0];
                    name = StringX.GetTextAfter(name, "join as ");
                    name = StringX.ToTitleCase(name);

                    player = new Player(name, address);
                } catch (Exception e)
                {
                    log.Warn("Malformed request: " + subject + " from " + message.FromAddress() + " Error: " + e.Message);
                    log.Debug("Body: " + message.Body);
                    return "Invalid syntax in your message, should be \"join as <name>\" where <name> is your name.";
                }
                
            }

            Game game = gameSystem.GetGameById(id);
            if(game == null)
            {
                log.Info("Unimplemented");
            }

            return "";
        }
    }
}
