using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Game
{
    class Game
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public int Id { get; }
        public string Title { get { return "Game " + Id; } }
        public Player Overlord { get; }
        private bool isInProgress { get; set; }
        internal IList<Player> Players = new List<Player>();

        public Game(int id, Player overlord)
        {
            Id = id;
            Overlord = overlord;
            Players.Add(overlord);
            isInProgress = false;
        }

        public bool IsInProgress()
        {
            return isInProgress;
        }

        public bool AddPlayer(Player player)
        {
            if (!IsPlaying(player))
            {
                Players.Add(player);
                return true;
            }
            return false;
        }

        internal bool RemovePlayer(Player player)
        {
            return Players.Remove(player);
        }

        public bool IsPlaying(Player player)
        {
            for (int i = 0; i < this.Players.Count; i++)
            {
                if (this.Players[i].Address.Equals(player.Address)) return true;
            }
            return false;
        }

        internal bool IsOverlord(Player player)
        {
            if (Overlord.Address.Equals(player.Address)) return true;
            return false;
        }

        internal string Status(Player player)
        {
            string status = "Status of " + Title + ":<br /><ul>"
                + "<li>In progress: " + (isInProgress ? "Yes</li>" : "No</li>")
                + "<li>Players:</li>" + ListPlayers() + "</ul>"
                + "<hr>"
                + GameSystem.Instance.Commands(this, player);
            return status;
        }

        private string ListPlayers()
        {
            //when implemented show like
            //Fred: Has Voted
            //Fred: Has Chosen
            string players = "<ul>";
            foreach (Player player in Players)
            {
                string state = "";
                if(isInProgress)
                {
                    if (player.IsAlive) state = " <b>Alive</b>";
                    else state = " Dead";
                }
                players = players + "<li>" + player + state + "</li>";
            }
            players = players + "</ul>";
            return players;
        }

        public override string ToString()
        {
            return Title + " Overlord: " + Overlord;
        }

        internal void Start()
        {
            //Check if min players is met
            isInProgress = true;
            //Assign roles, set players to alive and start game
        }

        public override bool Equals(object obj)
        {
            try
            {
                Game g = (Game)obj;
                if (g.Id == Id) return true;
            }
            catch (InvalidCastException e)
            {
                log.Warn("Attempted to compare non game object to a game. Error: " + e.Message);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
