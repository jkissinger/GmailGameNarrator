﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GmailGameNarrator.Game;
using GmailGameNarrator.Game.Roles;
using System;

namespace GmailGameNarrator.Tests
{
    [TestClass]
    public class TestRoleGeneration
    {
        private static GameSystem gameSystem = GameSystem.Instance;

        [TestMethod]
        public void TestGameGeneration()
        {
            for (int tests = 0; tests < 100; tests++)
            {
                for (int i = 3; i < 25; i = i + 3)
                {
                    List<Player> players = GenListOfPlayers(i);
                    Game.Game game = new Game.Game(gameSystem.GetNextGameId(), players[0]);
                    Assert.IsTrue(validateTeamComposition(game));
                    for (int j = 1; j < players.Count; j++)
                    {
                        game.AddPlayer(players[j]);
                    }
                    List<Type> roleTypes = gameSystem.GetRoleTypes();
                    game.AssignRoles(roleTypes);
                    Assert.IsTrue(ValidateRoleMaxCount(game));
                    Assert.IsTrue(ValidateRoleMaxPercentage(game));
                    Assert.IsTrue(ValidateTeamMinPercent(game));
                }
            }
        }
        private List<Player> GenListOfPlayers(int size)
        {
            List<Player> players = new List<Player>();
            for (int i = 0; i < size; i++)
            {
                string name = "Player" + i;
                string address = "Player" + i + Program.UnitTestAddress;
                Player player = new Player(name, address);
                players.Add(player);
            }
            return players;
        }

        /// <summary>
        /// Ensures the minimum team composition doesn't exceed 100%.
        /// </summary>
        private bool validateTeamComposition(Game.Game game)
        {
            int totalPercent = 0;
            foreach (Team t in game.GetTeamsPlayingUnique())
            {
                totalPercent += t.MinPercentComposition;
            }
            if (totalPercent > 100)
            {
                throw new System.Exception("Minimum team composition required can be greater than 100%");
            }
            return true;
        }

        /// <summary>
        /// Validates the following:
        /// <para />For each team: Percentage of players in this game is greater than <see cref="Team.MinPercentComposition"/>
        /// <para />At least 2 teams are playing.
        /// <para /><see cref="Role.MaxPlayers"/> is not violated.
        /// </summary>
        /// <returns></returns>
        private bool ValidateTeamMinPercent(Game.Game game)
        {
            if (game.GetTeamsPlayingUnique().Count < 2) return false;
            foreach (Team t in game.GetTeamsPlayingUnique())
            {
                int teamMembersCount = game.GetCountOfPlayersOnTeam(t);
                int minMembers = MathX.Percent(game.Players.Count, t.MinPercentComposition);
                if (teamMembersCount < minMembers) return false;
            }
            return true;
        }

        private bool ValidateRoleMaxCount(Game.Game game)
        {
            foreach (Role r in game.GetPlayingRoles())
            {
                int count = 0;
                foreach (Player p in game.Players)
                {
                    if (p.Role.Equals(r)) count++;
                }
                if (count > r.MaxPlayers) return false;
            }
            return true;
        }

        private bool ValidateRoleMaxPercentage(Game.Game game)
        {
            foreach (Role r in game.GetPlayingRoles())
            {
                int count = 0;
                foreach (Player p in game.Players)
                {
                    if (p.Role.Equals(r)) count++;
                }
                int minPercent = MathX.Percent(game.Players.Count, r.MaxPercentage);
                if (count > minPercent) return false;
            }
            return true;
        }

        [TestMethod]
        public void TestGameOver()
        {
            for (int tests = 0; tests < 1; tests++)
            {
                for (int i = 3; i < 4; i = i + 3)
                {
                    List<Player> players = GenListOfPlayers(i);
                    Game.Game game = new Game.Game(gameSystem.GetNextGameId(), players[0]);
                    for (int j = 1; j < players.Count; j++)
                    {
                        game.AddPlayer(players[j]);
                    }
                    List<Type> roleTypes = gameSystem.GetRoleTypes();
                    game.AssignRoles(roleTypes);
                    KillPlayers(players);
                    Assert.IsTrue(game.CheckGameEnd("Test"));
                }
            }
        }

        public void KillPlayers(List<Player> players)
        {
            for(int i = 1; i < players.Count; i++)
            {
                players[i].Kill(null);
            }
        }

        
    }
}