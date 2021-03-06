﻿using GmailGameNarrator.Narrator.Roles;
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
                if (t.Namespace == "GmailGameNarrator.Narrator.Roles")
                {
                    if (!t.IsAbstract)
                    {
                        Role role = (Role)Activator.CreateInstance(t);
                        //Implements the Prevalence property
                        for (int i = 0; i < role.Prevalence; i++) RoleTypes.Add(t);
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
            foreach (Game g in Games)
            {
                if (g.Players.Contains(player)) game = g;
            }
            return game;
        }

        public void PerformAction(Game game, Player player, Action action)
        {
            if (action.Type == ActionEnum.JoinAs)
            {
                //If JoinAs gets hit here, it's only because the player is already playing in a different game
                Game g = GetGameByPlayer(player);
                if (g != null) HandleBadAction(g, player, action, "Failed to join " + game.Title + ". You are already playing in " + g.Title + ".");
            }
            else if (action.Type == ActionEnum.Status) Status(player, action, game);
            else if (action.Type == ActionEnum.Cancel) Cancel(player, action, game);
            else if (action.Type == ActionEnum.Start) Start(player, action, game);
            else if (action.Type == ActionEnum.Quit) Quit(player, action, game);
            else if (action.Type == ActionEnum.Vote && game.ActiveCycle == Game.Cycle.Day) Vote(player, action, game);
            else if (action.Type == ActionEnum.Help) Help(player, action, game);
            else if (action.Type == ActionEnum.Role) RoleAction(player, action, game);
            //TODO Implement Kick & Ban
            //if (action.Name == ActionEnum.Kick) Kick(player, action, game);
            //if (action.Name == ActionEnum.Ban) Ban(player, action, game);
        }

        private void RoleAction(Player player, Action action, Game game)
        {
            //As of now role actions only happen at night
            if (game.ActiveCycle == Game.Cycle.Night)
            {
                string result = player.Role.AddAction(player, action, game);
                if (!String.IsNullOrEmpty(result))
                {
                    HandleBadAction(game, player, action, result);
                    return;
                }
                game.CheckEndOfCycle();
            }
        }

        private void Vote(Player player, Action action, Game game)
        {
            Player nominee = action.Target;
            if (nominee == null)
            {
                if (action.Text.Equals("no one") || action.Text.Equals("nobody") || action.Text.Equals("noone"))
                {
                    Player NoOne = new Player("No one", "nobody");
                    nominee = NoOne;
                }
                else
                {
                    HandleBadAction(game, player, action, "You voted for an invalid player <b>" + action.Target + "</b> see the player's names below." + FlavorText.Divider + game.Status());
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
                Action vote = new Action(ActionEnum.Vote);
                vote.Target = nominee;
                player.AddAction(vote);
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
                if (!game.Start())
                {
                    HandleBadAction(game, player, action, "You need at least 3 players to start a game." + FlavorText.Divider + game.Status());
                }
            }
        }

        public void Quit(Player player, Action action, Game game)
        {
            if (game.IsInProgress)
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

        /// <summary>
        /// The given player isn't playing in any games, but they did reference a valid game id.  See if they want to join it.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="actions"></param>
        /// <param name="game"></param>
        public void ValidateJoinAction(string address, List<Action> actions, Game game)
        {
            foreach (Action action in actions)
            {
                if (action.Type == ActionEnum.JoinAs)
                {
                    Player player = new Player(action.Text.ToTitleCase(), address);
                    if (String.IsNullOrEmpty(player.Name))
                    {
                        HandleBadAction(game, player, action, "You didn't specify the name you wished to join the game as. To do this reply to this message with \"Join as <i>name</i>\" where <i>name</i> is your name.");
                        return;
                    }
                    else if (game.IsInProgress)
                    {
                        HandleBadAction(game, player, action, "You cannot join " + game + " because it is in progress.");
                    }
                    else if (game.GetPlayer(player.Name, "") != null)
                    {
                        HandleBadAction(game, player, action, "Someone else is already using " + player.Name.b() + " as their name, please choose a different name.");
                    }
                    else
                    {
                        ProcessJoinAction(player, action, game);
                    }
                    return;
                }
            }
            //None of the actions were join actions, and they aren't playing in any games so...
            string msg = "You aren't playing any games, the only action you may take is joining a game or creating a game. To start a new game reply to this message with \"Join as <i>name</i>\" where <i>name</i> is your name.";
            log.Warn("Unknown malformed action from " + address);
            Gmail.MessagePerson(address, game.Subject, msg);

        }

        /// <summary>
        /// Join game request is valid, process it.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="action"></param>
        /// <param name="game"></param>
        private void ProcessJoinAction(Player player, Action action, Game game)
        {
            game.AddPlayer(player);
            Gmail.MessagePlayer(player, game, "Successfully added you to the game, <b>" + game.Overlord + "</b> is the Overlord.<br />"
                + "I will notify you when the game has started."
                + "<br /><br />"
                + game.Help(player));
            Gmail.MessagePlayer(game.Overlord, game, "<b>" + player + "</b> has joined " + game.Title);
            gameLog.Info("Added player " + player + " to " + game);
        }

        public void NewGame(SimpleMessage message, List<Action> actions)
        {
            string address = message.From;
            string name = "";
            Action joinAction = null;
            foreach (Action action in actions)
            {
                if (action.Type == ActionEnum.JoinAs) joinAction = action;
            }
            name = joinAction.Text;
            Player player = new Player(name, address);
            if (String.IsNullOrEmpty(name))
            {
                HandleBadAction(null, player, joinAction, "You didn't specify the name you wished to join the game as. To do this send a new email with \"Join as <i>name</i>\" where <i>name</i> is your name.");
                return;
            }
            Game game = GetGameByPlayer(player);
            if (game != null)
            {
                HandleBadAction(null, player, joinAction, "You are already playing in " + game + " , you may only play one game at a time.");
                return;
            }
            game = new Game(GetNextGameId(), player);
            Games.Add(game);
            Gmail.MessagePlayer(player, game, game.Title + " has been started, you are the <b>Overlord</b>."
                + "<br /><br />Have your friends join by sending a message with the subject \"" + game.Title + "\" and body \"Join as <i>name</i>\" where <i>name</i> is their name.<br />"
                + game.Help(player)
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
            log.Warn("Malformed action " + action.Type + " with target " + action.Target + " and text " + action.Text + " from " + player.Address);
            Gmail.MessagePlayer(player, game, error);
        }
    }
}
