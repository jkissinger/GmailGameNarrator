using GmailGameNarrator.Game.Roles;
using System.Collections.Generic;

namespace GmailGameNarrator.Game
{
    class NarratorSystem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //These should probably be concurrent collection types?
        private IList<Game> Games = new List<Game>();
        private IList<Player> Players = new List<Player>();
        private IList<Role> Roles = new List<Role>();

        private static NarratorSystem instance;

        private NarratorSystem() { }

        public static NarratorSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    log.Info("Initializing NarratorSystem instance.");
                    instance = new NarratorSystem();
                }
                return instance;
            }
        }

        public Game NewGame(Player overlord)
        {
            if (GetGameByPlayer(overlord) != null) return null;
            Game game = new Game(this.Games.Count, overlord);
            this.Games.Add(game);
            Gmail.EnqueueEmail(overlord.email, "Game " + game.Id, "Game " + game.Id + " has been started, you are the Overlord."
                + "\nHave your friends join by sending an email with the subject \"Game " + game.Id + "\" and their name in the body to me."
                + "\nTo start the game, reply to this message with \"Start\"."
                + "\nTo cancel, reply to this message with \"Cancel\"");
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


    }
}
