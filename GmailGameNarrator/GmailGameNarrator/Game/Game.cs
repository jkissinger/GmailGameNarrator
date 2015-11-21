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
        public Player overlord { get; }
        private IList<Player> players = new List<Player>();

        public Game(int id, Player overlord)
        {
            this.Id = id;
            this.overlord = overlord;
            this.players.Add(overlord);
        }

        public bool AddPlayer(Player player)
        {
            if (!IsPlaying(player))
            {
                this.players.Add(player);
                return true;
            }
            return false;
        }

        public bool IsPlaying(Player player)
        {
            for (int i=0;i<this.players.Count;i++)
            {
                if (this.players[i].address.Equals(player.address)) return true;
            }
            return false;
        }
    }
}
