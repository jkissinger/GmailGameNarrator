using GmailGameNarrator.Game.Roles;
using System.Collections.Generic;

namespace GmailGameNarrator.Game
{
    class GameSystem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IList<Game> Games = new List<Game>();
        private IList<Player> Players = new List<Player>();
        private IList<Role> Roles = new List<Role>();

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

        public Game NewGame(Player overlord)
        {
            if (GetGameByPlayer(overlord) != null) return null;
            Game game = new Game(this.Games.Count, overlord);
            this.Games.Add(game);
            Gmail.EnqueueMessage(overlord.address, "Game " + game.Id, "Game " + game.Id + " has been started, you are the Overlord."
                + "\nHave your friends join by sending an email with the subject \"Game " + game.Id + "\" and their name in the body to me."
                + "\nTo start the game, reply to this message with \"Start\"."
                + "\nTo cancel, reply to this message with \"Cancel\"");
            this.Players.Add(overlord);
            return game;
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
                if (p.address.Equals(address)) player = p;
            }
            return player;
        }


    }
}
