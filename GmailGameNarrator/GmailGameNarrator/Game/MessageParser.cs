using System;
using System.Collections.Generic;

namespace GmailGameNarrator.Game
{
    class MessageParser
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly log4net.ILog gameLog = log4net.LogManager.GetLogger("Game." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            string subject = message.NiceSubject().Replace("re: ", "");

            if (subject.Equals("new game"))
            {
                DoActions(message, null);
            }
            else if(subject.StartsWith("game "))
            {
                Game game = GetGameByMessage(message);
                //Errors are handled by GetGameByMessage above, don't do anything here
                if(game != null) DoActions(message, game);
            }
        }

        private Game GetGameByMessage(SimpleMessage message)
        {
            string subject = message.NiceSubject();

            int id;
            try
            {
                string strId = StringX.GetTextAfter(subject, "game ");
                id = Int32.Parse(strId);
            }
            catch (Exception e)
            {
                HandleBadMessage(message, "Invalid syntax in your subject, should be \"Game #\" where # is your game id.", e);
                return null;
            }

            Game game = GameSystem.Instance.GetGameById(id);
            if (game == null)
            {
                HandleBadMessage(message, "You referenced an invalid game id: " + id);
            }

            return game;
        }
        /// <summary>
        /// Processes all actions in the message, calls <see cref="ParseActions(SimpleMessage)"/> to get a list of valid actions.
        /// Passes each action to <see cref="GameSystem.DoAction(Game, Player, Action)"/>.
        /// </summary>
        /// <param name="message">The message to process</param>
        /// <param name="game">The game the player is playing in</param>
        /// <seealso cref="ParseActions(SimpleMessage)"/>
        private void DoActions(SimpleMessage message, Game game)
        {
            GameSystem gameSystem = GameSystem.Instance;
            string address = message.FromAddress();
            Player player = gameSystem.GetPlayerByAddress(address);
            List<Action> actions = ParseActions(message);

            if (player == null)
            {
                gameSystem.JoinGame(address, actions, game);
            }
            else
            {
                foreach (Action action in actions)
                {
                    gameSystem.DoAction(game, player, action);
                }
            }
        }

        private List<Action> ParseActions(SimpleMessage message)
        {
            List<Action> actions = new List<Action>();
            foreach (string line in message.BodyAsLines())
            {
                foreach (var e in Enum.GetValues(typeof(GameSystem.ActionEnum)))
                {
                    string eWithSpaces = StringX.AddSpaces(e.ToString());
                    if (line.Contains(eWithSpaces.ToLowerInvariant()))
                    {
                        GameSystem.ActionEnum actionEnum = (GameSystem.ActionEnum)Enum.Parse(typeof(GameSystem.ActionEnum), e.ToString());
                        string parameter = StringX.GetTextAfter(line, eWithSpaces).Trim();
                        Action action = new Action(actionEnum, parameter);
                        actions.Add(action);
                    }
                }
            }

            return actions;
        }

        private void HandleBadMessage(SimpleMessage message, string error, Exception e)
        {
            log.Warn("Exception thrown when parsing message: " + e.Message);
            HandleBadMessage(message, error);
        }

        private void HandleBadMessage(SimpleMessage message, string error)
        {
            log.Warn("Malformed request: " + message.Subject + " from " + message.From);
            log.Debug("Body: " + message.Body);
            Gmail.EnqueueMessage(message.From, message.Subject, error);
        }
    }
}
