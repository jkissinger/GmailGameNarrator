using GmailGameNarrator.Narrator.Roles;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GmailGameNarrator.Narrator
{
    public class GameSystem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly log4net.ILog gameLog = log4net.LogManager.GetLogger("Game." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IList<Game> Games = new List<Game>();

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
            Ban,
            Team,
            Role,
            GameStart,
            Help
        }

        /// <summary>
        /// Uses the assembly to generate a list of all Role classes.
        /// Implements <see cref="Role.Prevalence"/> by including <code>Prevalence</code> number of that Role's type.
        /// </summary>
        /// <returns>List containing each type of <see cref="Role"/></returns>
        public List<Type> GetRoleTypes()
        {
            List<Type> RoleTypes = new List<Type>();
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.Namespace == "GmailGameGame.Roles")
                {
                    if (!t.IsAbstract)
                    {
                        Role role = (Role)Activator.CreateInstance(t);
                        //Implements the Prevalence property
                        for(int i=0;i<role.Prevalence;i++) RoleTypes.Add(t);
                    }
                }
            }
            return RoleTypes;
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
            foreach (Game game in Games)
            {
                foreach (Player p in game.Players)
                {
                    if (p.Address.Equals(address)) player = p;
                }
            }
            return player;
        }

        public void DoAction(Game game, Player player, Action action)
        {
            if (action.Name == ActionEnum.JoinAs)
            {
                //If JoinAs gets hit here, it's only because the player is already playing in a different game
                Game g = GetGameByPlayer(player);
                HandleBadAction(g, player, action, "Failed to join " + game.Title + ". You are already playing in " + g.Title + ".");
            }
            else if (action.Name == ActionEnum.Status) Status(player, action, game);
            else if (action.Name == ActionEnum.Cancel) Cancel(player, action, game);
            else if (action.Name == ActionEnum.Start) Start(player, action, game);
            else if (action.Name == ActionEnum.Quit) Quit(player, action, game);
            else if (action.Name == ActionEnum.Vote) Vote(player, action, game);
            else if (action.Name == ActionEnum.Help) Help(player, action, game);
            else if (action.Name == ActionEnum.Role) RoleAction(player, action, game);
            //if (action.Name == ActionEnum.Kick) Kick(player, action, game);
            //if (action.Name == ActionEnum.Ban) Ban(player, action, game);
        }

        private void RoleAction(Player player, Action action, Game game)
        {
            string result = player.AddNightAction(action, game);
            if (!String.IsNullOrEmpty(result))
            {
                HandleBadAction(game, player, action, result);
                return;
            }
            List<string> actions = new List<string>();
            foreach(Action a in player.Actions)
            {
                actions.Add(player.Role.Name.b() + a.Parameter);
            }
            Gmail.MessagePlayer(player, game, "Your registered actions:<br />" + actions.HtmlBulletList());
            game.CheckEndOfCycle();
        }

        private void Vote(Player player, Action action, Game game)
        {
            if(game.ActiveCycle == Game.Cycle.Night)
            {
                HandleBadAction(game, player, action, "You can only vote during the day, your vote for <b>" + action.Parameter + "</b> was discarded.");
            }
            Player nominee = game.GetPlayerByName(action.Parameter);
            if(nominee == null)
            {
                if (action.Parameter.Equals("no one") || action.Parameter.Equals("nobody"))
                {
                    Player NoOne = new Player("No one", "nobody");
                    nominee = NoOne;
                }
                else
                {
                    HandleBadAction(game, player, action, "You voted for an invalid player <b>" + action.Parameter + "</b> see the player's names below." + FlavorText.Divider + game.Status());
                    return;
                }
            }
            if (nominee.Equals(player))
            {
                HandleBadAction(game, player, action, "You cannot vote for yourself.");
            }
            else if (!nominee.IsAlive)
            {
                HandleBadAction(game, player, action, nominee.Name.b() + " is already dead. See who's alive below:" + FlavorText.Divider + game.Status());
            }
            else
            {
                Vote vote = new Vote(action, nominee);
                player.Vote = vote;
                Gmail.MessagePlayer(player, game, "Registered your vote for <b>" + nominee.Name + "</b>.");
                game.CheckEndOfCycle();
            }
        }

        public void Help(Player player, Action action, Game game)
        {
            gameLog.Info("Sending Help message for " + game + " to " + player.Address);
            Gmail.MessagePlayer(player, game, game.Help(player));
        }

        public void Status(Player player, Action action, Game game)
        {
            gameLog.Info("Sending Status of " + game + " to " + player.Address);
            Gmail.MessagePlayer(player, game, game.Status());
        }

        public void Cancel(Player player, Action action, Game game)
        {
            if (game.IsOverlord(player))
            {
                gameLog.Info("Cancelling " + game);
                RemoveGame(game);
                Gmail.MessagePlayer(player, game, game.Title + " has been cancelled.");
            }
        }

        public void RemoveGame(Game game)
        {
            Games.Remove(game);
        }

        public void Start(Player player, Action action, Game game)
        {
            if (game.IsOverlord(player))
            {
                gameLog.Info("Attempting to start " + game);
                if(!game.Start())
                {
                    HandleBadAction(game, player, action, "You need at least 3 players to start a game." + FlavorText.Divider + game.Status());
                }
            }
        }

        public void Quit(Player player, Action action, Game game)
        {
            if (game.IsInProgress())
            {
                player.Quit();
                Gmail.MessagePlayer(player, game, "Your character in " + game.Title + ", " + player.Name + " is now dead.");
                gameLog.Info(player + " is dead because they quit " + game);
            }
            else
            {
                game.RemovePlayer(player);
                Gmail.MessagePlayer(player, game, "You have been removed from " + game);
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
                            HandleBadAction(null, player, action, "You didn't specify the name you wished to join the game as. To do this reply to this message with \"Join as <i>name</i>\" where <i>name</i> is your name.");
                        }
                        else
                        {
                            HandleBadAction(game, player, action, "You didn't specify the name you wished to join the game as. To do this reply to this message with \"Join as <i>name</i>\" where <i>name</i> is your name.");
                        }
                        return;
                    }
                    JoinGame(player, action, game);
                    return;
                }
            }
            Player p = new Player("", address);
            Action a = new Action(ActionEnum.JoinAs, "");
            HandleBadAction(null, p, a, "You aren't playing any games, the only action you may take is joining a game or creating a game. To start a new game reply to this message with \"Join as <i>name</i>\" where <i>name</i> is your name.");
        }

        private void JoinGame(Player player, Action action, Game game)
        {
            if (game == null) NewGame(player, action);
            else
            {
                if(game.IsInProgress())
                {
                    HandleBadAction(game, player, action, "You cannot join " + game + " because it is in progress.");
                }
                else
                {
                    game.AddPlayer(player);
                    Gmail.MessagePlayer(player, game, "Successfully added you to the game, <b>" + game.Overlord + "</b> is the Overlord.<br />"
                        + "I will notify you when the game has started."
                        + "<br /><br />"
                        + game.Help(player));
                    Gmail.MessagePlayer(game.Overlord, game, "<b>" + player + "</b> has joined " + game.Title);
                    gameLog.Info("Added player " + player + " to " + game);
                }
            }
        }

        private void NewGame(Player overlord, Action action)
        {
            Game game = GetGameByPlayer(overlord);
            if (game != null)
            {
                HandleBadAction(null, overlord, action, "You are already playing in " + game + " , you may only play one game at a time.");
                return;
            }
            game = new Game(GetNextGameId(), overlord);
            Games.Add(game);
            Gmail.MessagePlayer(overlord, game, game.Title + " has been started, you are the <b>Overlord</b>."
                + "<br /><br />Have your friends join by sending a message with the subject \"" + game.Title + "\" and body \"Join as <i>name</i>\" where <i>name</i> is their name.<br />"
                + game.Help(overlord)
                + "<br /><hr><br />" + Program.License.Replace('\n'.ToString(), "<br />")
                + "<br /><br />Download this program on " + Program.GitHub);
            gameLog.Info("Started new game: " + game);
        }

        public int GetNextGameId()
        {
            int id = Properties.Settings.Default.NumGames;
            Properties.Settings.Default.NumGames++;
            Properties.Settings.Default.Save();
            return id;
        }

        private void HandleBadAction(Game game, Player player, Action action, string error, Exception e)
        {
            log.Warn("Exception thrown when parsing action: " + e.Message);
            HandleBadAction(game, player, action, error);
        }

        private void HandleBadAction(Game game, Player player, Action action, string error)
        {
            log.Warn("Malformed action " + action.Name + " with param " + action.Parameter + " from " + player.Address);
            Gmail.MessagePlayer(player, game, error);
        }
    }
}
