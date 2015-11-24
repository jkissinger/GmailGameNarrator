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
        //FEATURE Implement summary functionality
        private Summary summary = new Summary();

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

        public bool RemovePlayer(Player player)
        {
            return MyPlayers.Remove(player);
        }

        public float GetTeamCount(Team team)
        {
            int count = 0;
            foreach(Player p in Players)
            {
                if (p.Team.Equals(team)) count++;
            }
            return count;
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

        public bool IsOverlord(Player player)
        {
            if (Overlord.Address.Equals(player.Address)) return true;
            return false;
        }

        /// <summary>
        /// Generates a list of active teams from the Players in a given game.
        /// </summary>
        /// <returns></returns>
        public List<Team> GetPlayingTeams()
        {
            List<Team> Teams = new List<Team>();
            foreach (Player p in Players)
            {
                if (!Teams.Contains(p.Team)) Teams.Add(p.Team);
            }
            return Teams;
        }

        /// <summary>
        /// Generates a list of active <see cref="Role"/>s from the Players in a given game.
        /// </summary>
        /// <returns></returns>
        public List<Role> GetPlayingRoles()
        {
            List<Role> Roles = new List<Role>();
            foreach (Player p in Players)
            {
                if (!Roles.Contains(p.Role)) Roles.Add(p.Role);
            }
            return Roles;
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
            foreach(Player p in Players)
            {
                p.Vote = null;
                p.ClearNightActions();
            }
        }

        public bool Start()
        {
            if (Players.Count < 3) return false;
            GameSystem gameSystem = GameSystem.Instance;
            List<Type> roleTypes = gameSystem.GetRoleTypes();

            //Assign roles and validate minimum team compositions have been met
            AssignRoles(roleTypes);
            validateTeamComposition();
            int counter = 0;
            while (!ValidateRoles())
            {
                AssignRoles(roleTypes);
                if (counter > 50) throw new Exception("It's taken more than 50 attempts to randomly choose a valid team composition, either make the algorithm smarter, or the composition easier to achieve."); 
                counter++;
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
                body += FlavorText.Divider + this.Status();
                body += FlavorText.Divider + this.Help(p);
                Gmail.EnqueueMessage(p.Address, Subject, body);
            }

            return true;
        }

        /// <summary>
        /// Ensures the minimum team composition doesn't exceed 100%.
        /// </summary>
        private void validateTeamComposition()
        {
            int totalPercent = 0;
            foreach (Team t in GetPlayingTeams())
            {
                totalPercent += t.MinPercentComposition;
            }
            if (totalPercent > 100)
            {
                throw new Exception("The minimum team composition required is greater than 100%, this is impossible to achieve.");
            }
        }

        private void AssignRoles(List<Type> roleTypes)
        {
            foreach (Player p in Players)
            {
                Type type = (Type)roleTypes.PickOne();
                Role role = (Role)Activator.CreateInstance(type);
                p.Role = role;
            }
        }
        /// <summary>
        /// Validates the following:
        /// <para />For each team: Percentage of players in this game is greater than <see cref="Team.MinPercentComposition"/>
        /// <para />At least 2 teams are playing.
        /// <para /><see cref="Role.MaxPlayers"/> is not violated.
        /// </summary>
        /// <returns></returns>
        private bool ValidateRoles()
        {
            if (GetPlayingTeams().Count < 2) return false;
            foreach (Team t in GetPlayingTeams())
            {
                if (!((GetTeamCount(t) / Players.Count) * 100 > t.MinPercentComposition)) return false;
            }
            foreach (Role r in GetPlayingRoles())
            {
                int count = 0;
                foreach (Player p in Players)
                {
                    if (p.Role.Equals(r)) count++;
                }
                if (count > r.MaxPlayers) return false;
            }
            //FEATURE Implement max percentage of role per team
            return true;
        }

        private string ListTeammates(Player player)
        {
            string message = "";
            List<string> teammates = new List<string>();
            foreach (Player p in Players)
            {
                if (!player.Equals(p) && player.Team.Equals(p.Team)) teammates.Add("<b>" + p.Name + "</b>");
            }
            message = teammates.HtmlBulletList();
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
            foreach (Player p in Players)
            {
                if (ActiveCycle == Cycle.Day && p.IsAlive && p.Vote == null) return;
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
                candidates.Add(p.Vote.Candidate);
                if (!AnonymousVoting) votingResults.Add(p.Name.b() + " voted for: " + p.Vote.Candidate.Name.i());
            }
            Dictionary<Player, int> candidateCounts = candidates.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            //Get 50% of players rounded up, the minimum for a majority
            int max = MathX.Percent(Players.Count, 50);
            Player electee = null;
            //Tally votes
            foreach (KeyValuePair<Player, int> c in candidateCounts)
            {
                //We found someone, but keep processing to record all the results.
                if (c.Value >= max) electee = c.Key;
                if (AnonymousVoting) votingResults.Add(c.Key.Name + ": " + c.Value);
            }
            //The first 2 if statements can't trigger a game end condition because no one dies.
            if (electee == null)
            {
                ShowVotes(votingResults, "There was no majority vote.");
            }
            else if (electee.Name.ToLowerInvariant().Equals("no one"))
            {
                ShowVotes(votingResults, "The majority vote was for no one, everyone is safe today.  But, at least one of you will probably regret it tonight...");
            }
            else {
                //Mark electee as dead then check if the game is over
                electee.IsAlive = false;
                if (CheckGameEnd("Results of the last vote:<br />" + votingResults.HtmlBulletList())) return;
                ShowVotes(votingResults, String.Format(FlavorText.PlayerOutcastMessage, electee.Name.b()));
            }
            foreach (Player p in Players)
            {
                string message = p.Role.Instructions;
                string teammates = ListTeammates(p);
                if(p.Team.KnowsTeammates && !String.IsNullOrEmpty(teammates)) message += FlavorText.Divider + teammates);
                if (p.IsAlive) Gmail.EnqueueMessage(p.Address, Subject, message);
            }
        }

        private void ShowVotes(List<string> votingResults, string resultMessage)
        {
            string message = CycleTitle + " has come to an end. Votes have been tallied: ";
            message += votingResults.HtmlBulletList();
            NextCycle();
            message += resultMessage;
            message += " " + CycleTitle + " will now begin.";
            MessageAllPlayers(message);
        }

        private void EndOfNight()
        {
            List<string> nightSummary = new List<string>();
            foreach(Player p in Players)
            {
                //FEATURE At some point we need to process night actions in order
                string msg = p.DoNightActions(this);
                if (!String.IsNullOrEmpty(msg)  && !nightSummary.Contains(msg)) nightSummary.Add(msg);
            }

            string message = "The night has come to a close, ";
            if (nightSummary.Count > 0) message += "this is what happened:<br />" + nightSummary.HtmlBulletList();
            else message = "it was completely uneventful.";
            MessageAllPlayers(message);
                            
            CheckGameEnd("");
        }

        public List<Player> GetLivingPlayersOnMyTeam(Player player)
        {
            return GetLivingPlayersOnTeam(player.Team);
        }

        public List<Player> GetLivingPlayersOnTeam(Team team)
        {
            List<Player> pList = new List<Player>();
            foreach(Player p in Players)
            {
                if (p.IsAlive && p.Team.Equals(team)) pList.Add(p);
            }
            return pList;
        }

        private bool CheckGameEnd(string finalMessage)
        {
            List<Player> winners = new List<Player>();
            foreach(Player p in Players)
            {
                if(p.HaveIWon(this)) winners.Add(p);
            }
            if (winners.Count > 0)
            {
                GameOver(winners, finalMessage);
                return true;
            }
            return false;
        }

        private void GameOver(List<Player> winners, string finalMessage)
        {
            List<string> winnersList = new List<string>();
            foreach (Player w in winners)
            {
                string msg = StringX.b(w.Name) + " as " + StringX.b(w.Role.Name) + " for the " + StringX.b(w.Team.Name);
                winnersList.Add(msg);
            }
            List<string> othersList = new List<string>();
            //BUG The list below seems to be empty
            List<Player> others = Players.Except(winners).ToList();
            foreach (Player o in others)
            {
                string msg = StringX.b(o.Name) + " as " + StringX.b(o.Role.Name) + " for the " + StringX.b(o.Team.Name);
            }
            string message = "The game is over.  Winners:<br />";
            message += winnersList.HtmlBulletList();
            message += "Everyone else:<br />";
            message += othersList.HtmlBulletList();
            message += finalMessage;
            MessageAllPlayers(message);
            GameSystem.Instance.RemoveGame(this);
        }

        //==========================
        //= Object overrides below =
        //==========================
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
