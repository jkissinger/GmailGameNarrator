﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GmailGameNarrator.Game;

namespace GmailGameNarrator.Tests
{
    /// <summary>
    /// Summary description for TestGamePlay
    /// </summary>
    [TestClass]
    public class TestGamePlay
    {
        private GameSystem gameSystem = GameSystem.Instance;

        [TestMethod]
        public void GamePlayTest()
        {
            for (int tests = 0; tests < 1; tests++)
            {
                for (int i = 3; i < 7; i = i + 3)
                {
                    List<Player> players = TestX.GenListOfPlayers(i);
                    Game.Game game = new Game.Game(gameSystem.GetNextGameId(), players[0]);
                    for (int j = 1; j < players.Count; j++)
                    {
                        game.AddPlayer(players[j]);
                    }
                    Assert.IsTrue(game.Start());
                    while (game.IsInProgress())
                    {
                        if(game.ActiveCycle == Game.Game.Cycle.Day) DoDayVotes(game);
                        else DoNightActions(game);
                    }
                }
            }
        }

        private void DoDayVotes(Game.Game game)
        {
            Player candidate = (Player)LivingPlayers(game).PickOne();
            foreach (Player p in game.Players)
            {
                Game.Action action = new Game.Action(GameSystem.ActionEnum.Vote, candidate.Name.ToLowerInvariant());
                gameSystem.DoAction(game, p, action);
                while (p.Vote == null)
                {
                    Player newCandidate = (Player)game.Players.PickOne();
                    action = new Game.Action(GameSystem.ActionEnum.Vote, newCandidate.Name.ToLowerInvariant());
                    gameSystem.DoAction(game, p, action);
                }
            }
        }

        private void DoNightActions(Game.Game game)
        {
            foreach (Player p in game.Players)
            {
                if (!p.IsAlive) continue;
                while (p.Actions.Count == 0)
                {
                    Game.Action action = GenerateAction(game, p);
                    gameSystem.DoAction(game, p, action);
                }
            }
        }

        private Game.Action GenerateAction(Game.Game game, Player p)
        {
            Player candidate = (Player)LivingPlayers(game).PickOne();
            if (p.Team.Equals("Illuminati")) candidate = (Player)LivingSheeple(game).PickOne();
            string param = p.Role.ActionText;
            if (param.Length > 0)
            {
                param += " " + candidate.Name.ToLowerInvariant();
            }
            Game.Action action = new Game.Action(GameSystem.ActionEnum.Role, param);
            return action;
        }

        private List<Player> LivingPlayers(Game.Game game)
        {
            List<Player> living = new List<Player>();
            foreach(Player p in game.Players)
            {
                if (p.IsAlive) living.Add(p);
            }
            return living;
        }

        private List<Player> LivingSheeple(Game.Game game)
        {
            List<Player> sheeple = new List<Player>();
            foreach (Player p in LivingPlayers(game))
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