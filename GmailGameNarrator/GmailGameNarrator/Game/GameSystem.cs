using GmailGameNarrator.Game.Roles;
using System;
using System.Collections.Generic;

namespace GmailGameNarrator.Game
{
    class GameSystem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly log4net.ILog gameLog = log4net.LogManager.GetLogger("Game." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IList<Game> Games = new List<Game>();
        private IList<Player> Players = new List<Player>();
        private IList<Role> Roles = new List<Role>();

        public enum ActionEnum
        {
            JoinAs,
            Start,
            Cancel,
            Quit,
            Status,
            Choose,
            Vote,
            Kick,
            Ban
        }

        public string Commands(Game game, Player player)
        {
            string commands = "<ul>";
            if (game.IsOverlord(player) && !game.IsInProgress()) commands += "<li>To start the game: <b>Start</b>.</li>";
            if (game.IsOverlord(player)) commands += "<li>To cancel the game: <b>Cancel</b></li>";
            if (game.IsOverlord(player)) commands += "<li>To kick a player from the game: <b>Kick</b> <i>name</i></li>";
            if (game.IsOverlord(player)) commands += "<li>To ban a player from the game: <b>Ban</b> <i>name</i></li>";
            if (!game.IsOverlord(player)) commands += "<li>To quit the game: <b>Quit</b></li>";
            commands += "<li>To see the game status: <b>Status</b></li>";
            commands += "</ul>";
            commands += "<br />To use a command, reply to this email as indicated above.";
            return commands;
        }

        private static GameSystem instance;

        private GameSystem() { }

        public static GameSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    log.Info("Initializing GameSystem instance.");
                    instance = new GameSystem();
                }
                return instance;
            }
        }

        public Game GetGameById(int id)
        {
            Game game = null;
            foreach (Game g in this.Games)
            {
                if (g.Id == id) game = g;
            }
            return game;
        }

        public Game GetGameByPlayer(Player player)
        {
            Game game = null;
            foreach (Game g in this.Games)
            {
                if (g.IsPlaying(player)) game = g;
            }
            return game;
        }

        public Player GetPlayerByAddress(string address)
        {
            Player player = null;
            foreach (Player p in Players)
            {
                if (p.Address.Equals(address)) player = p;
            }
            return player;
        }

        public bool DoAction(Game game, Player player, Action action)
        {
            if (action.Name == ActionEnum.JoinAs)
            {
                //If JoinAs gets hit here, it's only because the player is already playing in a different game
                Game g = GetGameByPlayer(player);
                HandleBadAction(g.Title, player, action, "Failed to join " + game.Title + ". You are already playing in " + g.Title + ".");
            }
            else if (action.Name == ActionEnum.Status) Status(player, action, game);
            else if (action.Name == ActionEnum.Cancel) Cancel(player, action, game);
            else if (action.Name == ActionEnum.Start) Start(player, action, game);
            else if (action.Name == ActionEnum.Quit) Quit(player, action, game);
            //if (action.Name == ActionEnum.Kick) Kick(player, action, game);
            //if (action.Name == ActionEnum.Ban) Ban(player, action, game);
            //if (action.Name == ActionEnum.Help) Help(player, action, game);
            return false;
        }

        public void Status(Player player, Action action, Game game)
        {
            gameLog.Info("Sending Status of " + game + " to " + player.Address);
            gameLog.Debug("Status Message: " + game.Status(player));
            Gmail.EnqueueMessage(player.Address, "Re: " + game.Title, game.Status(player));
        }

        public void Cancel(Player player, Action action, Game game)
        {
            if (game.IsOverlord(player))
            {
                gameLog.Info("Cancelling " + game);
                RemoveGame(game);
                Gmail.EnqueueMessage(player.Address, "Re: " + game.Title, game.Title + " has been cancelled.");
            }
        }

        public void RemoveGame(Game game)
        {
            foreach (Player p in game.Players)
            {
                Players.Remove(p);
            }
            Games.Remove(game);
        }

        public void Start(Player player, Action action, Game game)
        {
            if (game.IsOverlord(player))
            {
                gameLog.Info("Starting " + game);
                game.Start();
            }
        }

        public void Quit(Player player, Action action, Game game)
        {
            if (game.IsInProgress())
            {
                player.IsAlive = false;
                Gmail.EnqueueMessage(player.Address, "Re: " + game.Title, "Your character in " + game.Title + ", " + player.Name + " is now dead.");
                gameLog.Info(player + " is dead because they quit " + game);
            }
            else
            {
                game.RemovePlayer(player);
                Gmail.EnqueueMessage(player.Address, "Re: " + game.Title, "You have been removed from " + game);
                gameLog.Info(player + " is quitting " + game);
            }
        }

        public void JoinGame(string address, List<Action> actions, Game game)
        {
            foreach (Action action in actions)
            {
                if (action.Name == ActionEnum.JoinAs)
                {
                    Player player = new Player(StringX.ToTitleCase(action.Parameter), address);
                    if (String.IsNullOrEmpty(player.Name))
                    {
                        if (game == null)
                        {
                            HandleBadAction("New Game", player, action, "You didn't specify the name you wished to join the game as. To do this reply to this message with \"Join as <i>name</i>\" where <i>name</i> is your name.");
                        }
                        else
                        {
                            HandleBadAction(game.Title, player, action, "You didn't specify the name you wished to join the game as. To do this reply to this message with \"Join as <i>name</i>\" where <i>name</i> is your name.");
                        }
                        return;
                    }
                    JoinGame(player, action, game);
                    return;
                }
            }
            Player p = new Player("", address);
            Action a = new Action(ActionEnum.JoinAs, "");
            HandleBadAction("New Game", p, a, "You aren't playing any games, the only action you may take is joining a game.  To do this reply to this message with \"Join as <name>\" where <name> is your name.");
        }

        private void JoinGame(Player player, Action action, Game game)
        {
            if (game == null) NewGame(player, action);
            else
            {
                if(game.IsInProgress())
                {
                    HandleBadAction(game.Title, player, action, "You cannot join " + game + " because it is in progress.");
                }
                if (!game.AddPlayer(player))
                {
                    //This probably can't be hit
                    Game g = GetGameByPlayer(player);
                    HandleBadAction(game.Title, player, action, "Failed to join " + game.Title + ". You are already playing in " + g.Title + ".");
                }
                else
                {
                    Players.Add(player);
                    Gmail.EnqueueMessage(player.Address, "Re: " + game.Title, "Successfully added you to the game, <b>" + game.Overlord + "</b> is the Overlord.<br />"
                        + "I will notify you when the game has started."
                        + "<br /><br />"
                        + Commands(game, player));
                    Gmail.EnqueueMessage(game.Overlord.Address, "Re: " + game.Title, "<b>" + player + "</b> has joined " + game.Title);
                    gameLog.Info("Added player " + player + " to " + game);
                }
            }
        }

        private void NewGame(Player overlord, Action action)
        {
            Game game = GetGameByPlayer(overlord);
            if (game != null)
            {
                HandleBadAction("New Game", overlord, action, "You are already playing in " + game + " , you may only play one game at a time.");
                return;
            }
            game = new Game(Properties.Settings.Default.NumGames, overlord);
            Properties.Settings.Default.NumGames++;
            Properties.Settings.Default.Save();
            Games.Add(game);
            Players.Add(overlord);
            Gmail.EnqueueMessage(overlord.Address, game.Title, game.Title + " has been started, you are the <b>Overlord</b>."
                + "<br /><br />Have your friends join by sending a message with the subject \"" + game.Title + "\" and body \"Join as <i>name</i>\" where <i>name</i> is their name.<br />"
                + Commands(game, overlord)
                + "<br /><hr><br />" + Program.License.Replace('\n'.ToString(), "<br />")
                + "<br /><br />Download this program on " + Program.GitHub);
            gameLog.Info("Started new game: " + game);
        }

        private void HandleBadAction(string subject, Player player, Action action, string error, Exception e)
        {
            log.Warn("Exception thrown when parsing action: " + e.Message);
            HandleBadAction(subject, player, action, error);
        }

        private void HandleBadAction(string subject, Player player, Action action, string error)
        {
            log.Warn("Malformed action " + action.Name + " with param " + action.Parameter + " from " + player.Address);
            Gmail.EnqueueMessage(player.Address, subject, error);
        }
    }
}
