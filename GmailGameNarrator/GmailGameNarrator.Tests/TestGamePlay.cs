using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GmailGameNarrator.Narrator;
using System;

namespace GmailGameNarrator.Tests
{
    /// <summary>
    /// Summary description for TestGamePlay
    /// </summary>
    [TestClass]
    public class TestGamePlay
    {
        private GameSystem gameSystem = GameSystem.Instance;
        /// <summary>
        /// Number of tests to run
        /// </summary>
        private int numTests = 1;
        /// <summary>
        /// Minimum player count to test
        /// </summary>
        private int minPlayers = 15;
        /// <summary>
        /// Maximum player count to test
        /// </summary>
        private int maxPlayers = 15;
        /// <summary>
        /// Number to increase player count by.
        /// </summary>
        private int iterator = 1;

        [TestMethod]
        public void GamePlayTest()
        {
            for (int tests = 0; tests < numTests; tests++)
            {
                for (int i = minPlayers; i < maxPlayers + 1; i = i + iterator)
                {
                    List<Player> players = TestX.GenListOfPlayers(i);
                    CreateTheGame(players[0]);
                    Game game = gameSystem.GetGameByPlayer(players[0]);
                    JoinPlayers(players, game);
                    Assert.IsTrue(game.Start());
                    Assert.IsTrue(game.GetLivingPlayers().Count == i);
                    while (game.IsInProgress)
                    {
                        if (game.ActiveCycle == Game.Cycle.Day) DoDayVotes(game);
                        else DoNightActions(game);
                    }
                }
            }
        }

        private void JoinPlayers(List<Player> players, Game game)
        {
            for(int i = 1; i < players.Count; i++)
            {
                SendAction(game, players[i], players[i], "join as");
            }
        }

        private void CreateTheGame(Player player)
        {
            SendAction(null, player, player, "join as");
        }

        private void DoDayVotes(Game game)
        {
            Player candidate = (Player)game.GetLivingPlayers().PickOne();
            foreach (Player p in game.GetLivingPlayers())
            {
                SendAction(game, p, candidate, "vote");
                while (p.Vote == null && game.IsInProgress && game.ActiveCycle == Game.Cycle.Day)
                {
                    Player newCandidate = (Player)game.GetLivingPlayers().PickOne();
                    SendAction(game, p, newCandidate, "vote");
                }
            }
        }

        private void DoNightActions(Game game)
        {
            Player sheepleCandidate = (Player)LivingSheeple(game).PickOne();
            foreach (Player p in game.GetLivingPlayers())
            {
                Player candidate = (Player)game.GetLivingPlayers().PickOne();
                if (p.Team.Equals("Illuminati")) SendAction(game, p, sheepleCandidate, p.Role.ActionText);
                else SendAction(game, p, candidate, p.Role.ActionText);
                while (p.Actions.Count == 0 && game.IsInProgress && game.ActiveCycle == Game.Cycle.Night)
                {
                    if (p.Team.Equals("Illuminati"))
                    {
                        sheepleCandidate = (Player)LivingSheeple(game).PickOne();
                        SendAction(game, p, sheepleCandidate, p.Role.ActionText);
                    }
                    else
                    {
                        candidate = (Player)game.GetLivingPlayers().PickOne();
                        SendAction(game, p, candidate, p.Role.ActionText);
                    }                 
                }
            }
        }

        private void SendAction(Game game, Player p, Player candidate, string actionText)
        {
            SimpleMessage msg = new SimpleMessage();
            msg.From = p.Address;
            msg.Subject = "New Game";
            if(game != null) msg.Subject = game.FullTitle;
            msg.Body = actionText + " " + candidate.Name.ToLowerInvariant();
            MessageParser.Instance.ParseMessage(msg);
        }

        private List<Player> LivingSheeple(Game game)
        {
            List<Player> sheeple = new List<Player>();
            foreach (Player p in game.GetLivingPlayers())
            {
                if (!p.Team.Equals("Illuminati")) sheeple.Add(p);
            }
            return sheeple;
        }

        public void KillPlayers(List<Player> players)
        {
            for (int i = 1; i < players.Count; i++)
            {
                players[i].Attack(null, true);
            }
        }
    }
}
