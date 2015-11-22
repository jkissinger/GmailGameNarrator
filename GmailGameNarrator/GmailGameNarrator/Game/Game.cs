using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GmailGameNarrator.Game.Roles;
using System.Linq;

namespace GmailGameNarrator.Game
{
    class Game
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public int Id { get; }
        public string Subject { get { return "Re: " + Title; } }
        public string Title { get { return "Game " + Id; } }
        public Player Overlord { get; }
        private bool isInProgress { get; set; }
        public ReadOnlyCollection<Player> Players
        {
            get
            {
                return new ReadOnlyCollection<Player>(MyPlayers);
            }
        }
        private IList<Player> MyPlayers = new List<Player>();
        private int RoundCounter = 0;
        public Cycle ActiveCycle { get { return MyCycle; } }
        private Cycle MyCycle;
        public enum Cycle
        {
            Day,
            Night
        }
        public string CycleTitle
        {
            get
            {
                return Enum.GetName(typeof(Cycle), ActiveCycle) + " " + RoundCounter;
            }
        }
        private bool AnonymousVoting = false;

        public Game(int id, Player overlord)
        {
            Id = id;
            Overlord = overlord;
            MyPlayers.Add(overlord);
            isInProgress = false;
        }

        public bool IsInProgress()
        {
            return isInProgress;
        }

        public void AddPlayer(Player player)
        {
            MyPlayers.Add(player);
        }

        internal bool RemovePlayer(Player player)
        {
            return MyPlayers.Remove(player);
        }

        public Player GetPlayerByName(string name)
        {
            foreach (Player p in Players)
            {
                if (p.Name.ToLowerInvariant().Equals(name)) return p;
            }

            return null;
        }

        public bool IsPlaying(Player player)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Address.Equals(player.Address)) return true;
            }
            return false;
        }

        internal bool IsOverlord(Player player)
        {
            if (Overlord.Address.Equals(player.Address)) return true;
            return false;
        }

        internal string Status()
        {
            string status = "Status of " + Title + ":<br /><ul>"
                + "<li>In progress: " + (isInProgress ? "Yes</li>" : "No</li>")
                + (isInProgress ? "<li>Cycle: " + CycleTitle + "</li>" : "")
                + "<li>Players:</li>" + ListPlayers() + "</ul>";
            return status;
        }

        private void NextCycle()
        {
            if (ActiveCycle == Cycle.Day)
            {
                MyCycle = Cycle.Night;
            }
            else
            {
                MyCycle = Cycle.Day;
                RoundCounter++;
            }
        }

        private string ListPlayers()
        {
            string players = "<ul>";
            foreach (Player player in Players)
            {
                string livingState = "";
                string cycleStatus = "";
                if (isInProgress)
                {
                    if (player.IsAlive)
                    {
                        livingState = " - <b>Alive</b>";
                        if (ActiveCycle == Cycle.Day && player.DayAction == null)
                        {
                            cycleStatus = " - <b><i>Waiting On Action</i></b>";
                        }
                        else if (ActiveCycle == Cycle.Night && player.NightActions.Count == 0)
                        {
                            cycleStatus = " - <b><i>Waiting On Action</i></b>";
                        }
                        else
                        {
                            cycleStatus = " - Action Submitted";
                        }
                    }
                    else livingState = " - Dead";
                    
                }
                players = players + "<li>" + player + livingState + cycleStatus + "</li>";
            }
            players = players + "</ul>";
            return players;
        }

        /// <summary>
        /// Sends a message to the given player listing all their available commands for the given game.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public string Help(Player player)
        {
            string commands = "<ul>";
            if (player.IsAlive && IsInProgress() && ActiveCycle == Cycle.Day) commands += "<li>To vote for someone to be cast out: <b>Vote</b> <i>name</i></li>";
            if (IsOverlord(player) && !IsInProgress()) commands += "<li>To start the game: <b>Start</b>.</li>";
            if (IsOverlord(player)) commands += "<li>To cancel the game: <b>Cancel</b></li>";
            if (IsOverlord(player)) commands += "<li>To kick a player from the game: <b>Kick</b> <i>name</i></li>";
            if (IsOverlord(player)) commands += "<li>To ban a player from the game: <b>Ban</b> <i>name</i></li>";
            if (player.IsAlive && !IsOverlord(player)) commands += "<li>To quit the game: <b>Quit</b></li>";
            commands += "<li>To see the game status: <b>Status</b></li>";
            commands += "</ul>";
            commands += "<br />To use a command, reply to this email as indicated above.";
            return commands;
        }

        internal bool Start()
        {
            if (Players.Count < 3) return false;
            GameSystem gameSystem = GameSystem.Instance;
            List<Role> Roles = gameSystem.GetRoles();
            List<Team> Teams = gameSystem.GetTeams();
            foreach (Player p in Players)
            {
                Role role = (Role)MathX.PickOne(Roles);
                //TODO Validate role chosen with percent composition
                p.Role = role;
            }

            //Initialize game
            RoundCounter++;
            MyCycle = Cycle.Day;
            isInProgress = true;

            //Iterate again after roles are finalized to setup the players and notify them.
            foreach (Player p in Players)
            {
                p.IsAlive = true;
                Role r = p.Role;
                string body = FlavorText.GetStartGameMessage();
                body += FlavorText.Divider + "You have been assigned the role of <b>" + r.Name + "</b> and are on the <b>" + r.Team.Name + "</b> team.<br />";
                string teammates = ListTeammates(p);
                if (String.IsNullOrEmpty(teammates)) teammates = "You have no teammates.";
                if (r.Team.KnowsTeammates) body += "Teammates:<br />" + teammates;
                body += FlavorText.Divider + CycleTitle + " has begun, use the commands below to take an action.";
                body += FlavorText.Divider + Status();
                body += FlavorText.Divider + Help(p);
                Gmail.EnqueueMessage(p.Address, Subject, body);
            }

            return true;
        }

        private string ListTeammates(Player player)
        {
            string message = "";
            List<string> teammates = new List<string>();
            foreach (Player p in Players)
            {
                if (!player.Equals(p) && player.Role.Team.Equals(p.Role.Team)) teammates.Add("<b>" + p.Name + "</b>");
            }
            message = FlavorText.HtmlBulletList(teammates);
            return message;
        }

        public void MessageAllPlayers(string body)
        {
            foreach (Player p in Players)
            {
                Gmail.EnqueueMessage(p.Address, Subject, body);
            }
        }


        public void CheckEndOfCycle()
        {
            //TODO Finish check end of cycle
            foreach (Player p in Players)
            {
                if (ActiveCycle == Cycle.Day && p.IsAlive && p.DayAction == null) return;
                if (ActiveCycle == Cycle.Night && p.IsAlive && p.NightActions.Count == 0) return;
            }
            if (ActiveCycle == Cycle.Day) EndOfDay();
            else EndOfNight();
        }

        private void EndOfDay()
        {
            List<string> votingResults = new List<string>();
            List<Player> candidates = new List<Player>();
            foreach (Player p in Players)
            {
                candidates.Add(p.DayAction.Candidate);
                if (!AnonymousVoting) votingResults.Add("<b>" + p.Name + "</b> voted for: <i>" + p.DayAction.Candidate.Name + "</i>");
            }
            Dictionary<Player, int> candidateCounts = candidates.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            //TODO There's a bug here
            int max = MathX.Percent(Players.Count, 50);
            Player winner = null;

            foreach (KeyValuePair<Player, int> c in candidateCounts)
            {
                if (c.Value >= max) winner = c.Key;
                if (AnonymousVoting) votingResults.Add(c.Key.Name + ": " + c.Value);
            }
            if (winner == null)
            {
                ShowVotes(votingResults, "There was no majority vote.");
            }
            else
            {
                winner.IsAlive = false;
                ShowVotes(votingResults, "<b>" + winner.Name + "</b> was chosen to be outcast and is no longer with the living.");
            }
            CheckGameEnd();
        }

        private void ShowVotes(List<string> votingResults, string resultMessage)
        {
            string message = CycleTitle + " has come to an end. Votes have been tallied: ";
            message += FlavorText.HtmlBulletList(votingResults);
            NextCycle();
            message += resultMessage;
            message += " " + CycleTitle + " will now begin.";
            MessageAllPlayers(message);
        }

        private void EndOfNight()
        {
            //TODO End of Night
        }

        private void CheckGameEnd()
        {
            //TODO Check game end
        }

        //Override methods below here:

        public override string ToString()
        {
            return Title + " Overlord: " + Overlord;
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
