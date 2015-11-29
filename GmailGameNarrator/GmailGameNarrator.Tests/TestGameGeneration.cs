using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GmailGameNarrator.Narrator;
using GmailGameNarrator.Narrator.Roles;
using System;

namespace GmailGameNarrator.Tests
{
    [TestClass]
    public class TestGameGeneration
    {
        private static GameSystem gameSystem = GameSystem.Instance;

        [TestMethod]
        public void GameGenerationTest()
        {
            for (int tests = 0; tests < 100; tests++)
            {
                for (int i = 3; i < 25; i = i + 3)
                {
                    List<Player> players = TestX.GenListOfPlayers(i);
                    Game game = new Game(gameSystem.GetNextGameId(), players[0]);
                    Assert.IsTrue(validateTeamComposition(game));
                    for (int j = 1; j < players.Count; j++)
                    {
                        game.AddPlayer(players[j]);
                    }
                    List<Type> roleTypes = gameSystem.GetRoleTypes();
                    game.Start();
                    Assert.IsTrue(ValidateRoleMaxPercentage(game));
                    Assert.IsTrue(ValidateTeamMinPercent(game));
                }
            }
        }

        /// <summary>
        /// Ensures the minimum team composition doesn't exceed 100%.
        /// </summary>
        private bool validateTeamComposition(Game game)
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
        private bool ValidateTeamMinPercent(Game game)
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

        /// <summary>
        /// Validates no more than the maximum percentage is made of the given role.  Even if the maximum is less than or = 1, 1 player of any role is allowed.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool ValidateRoleMaxPercentage(Game game)
        {
            foreach (Role r in game.GetPlayingRoles())
            {
                int count = 0;
                foreach (Player p in game.Players)
                {
                    if (p.Role.Equals(r)) count++;
                }
                int maxPercent = MathX.Percent(game.Players.Count, r.MaxPercentage);
                if (count > maxPercent && count > 1)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
