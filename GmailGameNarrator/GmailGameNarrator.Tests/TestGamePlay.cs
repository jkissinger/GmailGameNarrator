using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GmailGameNarrator.Narrator;

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
        private int numTests = 3;
        /// <summary>
        /// Minimum player count to test
        /// </summary>
        private int minPlayers = 10;
        /// <summary>
        /// Maximum player count to test
        /// </summary>
        private int maxPlayers = 10;
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
                    Game game = new Game(gameSystem.GetNextGameId(), players[0]);
                    for (int j = 1; j < players.Count; j++)
                    {
                        game.AddPlayer(players[j]);
                    }
                    Assert.IsTrue(game.Start());
                    while (game.IsInProgress())
                    {
                        if (game.ActiveCycle == Game.Cycle.Day) DoDayVotes(game);
                        else DoNightActions(game);
                    }
                }
            }
        }

        private void DoDayVotes(Game game)
        {
            Player candidate = (Player)game.GetLivingPlayers().PickOne();
            foreach (Player p in game.GetLivingPlayers())
            {
                Narrator.Action action = new Narrator.Action(GameSystem.ActionEnum.Vote, candidate.Name.ToLowerInvariant());
                gameSystem.DoAction(game, p, action);
                while (p.Vote == null && game.IsInProgress() && game.ActiveCycle == Game.Cycle.Day)
                {
                    Player newCandidate = (Player)game.GetLivingPlayers().PickOne();
                    action = new Narrator.Action(GameSystem.ActionEnum.Vote, newCandidate.Name.ToLowerInvariant());
                    gameSystem.DoAction(game, p, action);
                }
            }
        }

        private void DoNightActions(Game game)
        {
            Player sheepleCandidate = (Player)LivingSheeple(game).PickOne();
            foreach (Player p in game.GetLivingPlayers())
            {
                Player candidate = (Player)game.GetLivingPlayers().PickOne();
                Narrator.Action action = null;
                if (p.Team.Equals("Illuminati")) action = GenerateAction(game, p, sheepleCandidate);
                else action = GenerateAction(game, p, candidate);
                gameSystem.DoAction(game, p, action);
                while (p.Actions.Count == 0)
                {
                    if (p.Team.Equals("Illuminati")) sheepleCandidate = (Player)LivingSheeple(game).PickOne();
                    else candidate = (Player)game.GetLivingPlayers().PickOne();
                    action = GenerateAction(game, p, candidate);
                    gameSystem.DoAction(game, p, action);                    
                }
            }
        }

        private Narrator.Action GenerateAction(Game game, Player p, Player candidate)
        {
            string param = p.Role.ActionText;
            if (param.Length > 0)
            {
                param += " " + candidate.Name.ToLowerInvariant();
            }
            Narrator.Action action = new Narrator.Action(GameSystem.ActionEnum.Role, param);
            return action;
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
                players[i].Kill(null);
            }
        }
    }
}
