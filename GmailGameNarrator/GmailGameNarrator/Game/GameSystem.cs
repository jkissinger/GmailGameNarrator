using GmailGameNarrator.Game.Roles;
using System;
using System.Collections.Generic;

namespace GmailGameNarrator.Game
{
    class GameSystem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            Vote
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
            foreach(Game g in this.Games)
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
            foreach(Player p in Players)
            {
                if (p.Address.Equals(address)) player = p;
            }
            return player;
        }

        public bool DoAction(Game game, Player player, Action action)
        {
            //Join game is not handled/called here, it's called by messageparser
            if (action.Name == GameSystem.ActionEnum.Status) Status(player, action, game);
            return false;
        }

        public void Status(Player player, Action action, Game game)
        {
            Gmail.EnqueueMessage(player.Address, "Re: Game " + game.Id, game.Status());
        }

        public void JoinGame(string address, List<Action> actions, Game game)
        {
            foreach(Action action in actions)
            {
                if(action.Name == ActionEnum.JoinAs)
                {
                    Player player = new Player(StringX.ToTitleCase(action.Parameter), address);
                    if(String.IsNullOrEmpty(player.Name))
                    {
                        if (game == null)
                        {
                            HandleBadAction("New Game", player, action, "You didn't specify the name you wished to join the game as. To do this reply to this message with \"Join as <name>\" where <name> is your name.");
                        }
                        else
                        {
                            HandleBadAction("Game " + game.Id, player, action, "You didn't specify the name you wished to join the game as. To do this reply to this message with \"Join as <name>\" where <name> is your name.");
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
                if (!game.AddPlayer(player))
                {
                    Game g = GetGameByPlayer(player);
                    HandleBadAction("Game " + game.Id, player, action, "You are already playing in game " + g.Id + ".");
                }
                else
                {
                    Players.Add(player);
                    Gmail.EnqueueMessage(player.Address, "Re: Game " + game.Id, "Successfully added you to the game, " + game.Overlord.Name + " is the Overlord.  I will notify you when the game has started.");
                }
            }
        }

        private void NewGame(Player overlord, Action action)
        {
            Game game = GetGameByPlayer(overlord);
            if (game != null)
            {
                HandleBadAction("New Game", overlord, action, "You are already playing in " + game.Overlord.Name + "'s game " + game.Id + "  , you may only play one game at a time.");
                return;
            }
            game = new Game(Games.Count, overlord);
            Games.Add(game);
            Players.Add(overlord);
            Gmail.EnqueueMessage(overlord.Address, "Game " + game.Id, "Game " + game.Id + " has been started, you are the Overlord."
                + "\nHave your friends join by sending a message with the subject \"Game " + game.Id + "\" and body \"Join as <name>\" where <name> is their name."
                + "\nTo start the game, reply to this message with \"Start\"."
                + "\nTo cancel, reply to this message with \"Cancel\"");
            log.Info("Started new game: " + game);
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
