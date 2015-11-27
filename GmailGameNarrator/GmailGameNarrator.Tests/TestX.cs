using GmailGameNarrator.Narrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Tests
{
    class TestX
    {
        public static List<Player> GenListOfPlayers(int size)
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
    }
}
