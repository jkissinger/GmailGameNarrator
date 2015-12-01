using System;
using System.Collections.Generic;

namespace GmailGameNarrator.Narrator
{
    public class MessageParser
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
            int id = ProcessSubject(message);

            if (id == -1)
            {
                ProcessActions(message, null);
            }
            else if (id >= 0)
            {
                Game game = GameSystem.Instance.GetGameById(id);
                if (game == null)
                {
                    HandleBadMessage(message, "You referenced an invalid game id: " + id);
                }
                else ProcessActions(message, game);
            }
        }

        /// <summary>
        /// Process the message subject, returning the id of the game referenced, or -1 if referencing a new game request.  Otherwise -2 for malformed requests.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private int ProcessSubject(SimpleMessage message)
        {
            try
            {
                string subject = message.Subject.Trim().ToLowerInvariant();
                if (subject.StartsWith("new game")) return -1;
                string strId = subject.GetTextAfter("game");
                if (strId.Contains(" ")) strId = strId.GetTextBefore(" ");
                if (String.IsNullOrEmpty(strId)) throw new Exception("No game Id found.");
                int id = Int32.Parse(strId);
                return id;
            }
            catch (Exception e)
            {
                HandleBadMessage(message, "Invalid syntax in your subject, should be \"Game #\" where # is your game id.", e);
                return -2;
            }
        }
        /// <summary>
        /// Processes all actions in the message, calls <see cref="ParseActions(SimpleMessage)"/> to get a list of valid actions.
        /// Passes each action to <see cref="GameSystem.PerformAction(Game, Player, Action)"/>.
        /// </summary>
        /// <param name="message">The message to process</param>
        /// <param name="game">The game the player is playing in</param>
        /// <seealso cref="ParseActions(SimpleMessage)"/>
        private void ProcessActions(SimpleMessage message, Game game)
        {
            GameSystem gameSystem = GameSystem.Instance;
            string address = message.From;
            Player player = null;
            if (game != null) player = game.GetPlayer("", address);
            List<Action> actions = ParseActions(message, player, game);

            if (game == null) gameSystem.NewGame(message, actions);
            else if (player == null) gameSystem.ValidateJoinAction(address, actions, game);
            else
            {
                foreach (Action action in actions)
                {
                    gameSystem.PerformAction(game, player, action);
                }
            }
        }

        private List<Action> ParseActions(SimpleMessage message, Player player, Game game)
        {
            List<Action> actions = new List<Action>();

            foreach (string line in message.BodyAsLines())
            {
                Action action = null;
                if (player != null && game != null && game.IsInProgress)
                {
                    action = ParseGameAction(line, player, game);
                }
                if (action == null)
                {
                    action = ParseSystemAction(line);
                }
                if (action != null) actions.Add(action);
            }

            return actions;
        }

        /// <summary>
        /// Parses a line of the body of a message for system commands.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private Action ParseSystemAction(string line)
        {
            foreach (var e in Enum.GetValues(typeof(GameSystem.ActionEnum)))
            {
                string eWithSpaces = e.ToString().AddSpaces();
                if (line.StartsWith(eWithSpaces.ToLowerInvariant()))
                {
                    GameSystem.ActionEnum actionEnum = (GameSystem.ActionEnum)Enum.Parse(typeof(GameSystem.ActionEnum), e.ToString());
                    return StringToAction(line, actionEnum, eWithSpaces.ToLowerInvariant(), null);
                }
            }
            return null;
        }

        /// <summary>
        /// Parses a line of the body for game commands.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        private Action ParseGameAction(string line, Player player, Game game)
        {
            string actionText = player.Role.ActionText.ToLowerInvariant();
            if (line.StartsWith(actionText) && game.ActiveCycle == Game.Cycle.Night)
            {
                return StringToAction(line, GameSystem.ActionEnum.Role, actionText, game);
            }
            if (line.StartsWith("vote") && player.IsAlive && game.ActiveCycle == Game.Cycle.Day)
            {
                return StringToAction(line, GameSystem.ActionEnum.Vote, "vote", game);
            }
            return null;
        }

        /// <summary>
        /// Converts a given line to an <see cref="Action"/> of type actionType.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="actionType"></param>
        /// <param name="actionText"></param>
        /// <returns></returns>
        public Action StringToAction(string line, GameSystem.ActionEnum actionType, string actionText, Game game)
        {
            string param = line.GetTextAfter(actionText).Trim();
            Player target = null;
            if (game != null) target = game.GetPlayer(param, "");
            Action action = new Action(actionType);
            if (target == null) action.Text = param;
            else action.Target = target;
            return action;
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
            Gmail.MessagePerson(message.From, message.Subject, error);
        }
    }
}
