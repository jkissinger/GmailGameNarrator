using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game
{
    class Game
    {
        public int Id { get; }
        public Player Overlord { get; }
        public bool IsInProgress;
        private IList<Player> Players = new List<Player>();

        public Game(int id, Player overlord)
        {
            Id = id;
            Overlord = overlord;
            Players.Add(overlord);
            IsInProgress = false;
        }

        public bool AddPlayer(Player player)
        {
            if (!IsPlaying(player))
            {
                this.Players.Add(player);
                return true;
            }
            return false;
        }

        public bool IsPlaying(Player player)
        {
            for (int i=0;i<this.Players.Count;i++)
            {
                if (this.Players[i].Address.Equals(player.Address)) return true;
            }
            return false;
        }

        internal string Status()
        {
            string status = "Status of Game " + Id + ":\n"
                + "In progress: " + (IsInProgress ? "Yes\n" : "No\n")
                + "Players:\n" + ListPlayers();
            return status;
        }

        private string ListPlayers()
        {
            //when implemented show like
            //Fred: Has Voted
            //Fred: Has Chosen
            string players = "";
            foreach(Player player in Players)
            {
                players = players + player + "\n";
            }
            return players;
        }

        public override string ToString()
        {
            return "Id: " + Id + " Overlord: " + Overlord;
        }
    }
}
