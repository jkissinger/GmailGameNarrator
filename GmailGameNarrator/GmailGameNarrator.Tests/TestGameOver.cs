using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GmailGameNarrator.Game;

namespace GmailGameNarrator.Tests
{
    /// <summary>
    /// Summary description for TestGameOver
    /// </summary>
    [TestClass]
    public class TestGameOver
    {
        private GameSystem gameSystem = GameSystem.Instance;

        [TestMethod]
        public void GameOverTest()
        {
            for (int tests = 0; tests < 1; tests++)
            {
                for (int i = 3; i < 15; i = i + 3)
                {
                    List<Player> players = TestX.GenListOfPlayers(i);
                    Game.Game game = new Game.Game(gameSystem.GetNextGameId(), players[0]);
                    for (int j = 1; j < players.Count; j++)
                    {
                        game.AddPlayer(players[j]);
                    }
                    Assert.IsTrue(game.Start());
                    while (!game.CheckGameEnd())
                    {
                        KillNextPlayer(players);
                    }
                }
            }
        }

        public void KillNextPlayer(List<Player> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].IsAlive)
                {
                    players[i].Kill(null);
                    return;
                }
            }
        }
    }
}
