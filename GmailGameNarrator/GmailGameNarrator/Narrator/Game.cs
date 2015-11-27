using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GmailGameNarrator.Narrator.Roles;
using System.Linq;

namespace GmailGameNarrator.Narrator
{
    public class Game
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public int Id { get; }
        public string Subject
        {
            get
            {
                if(IsInProgress())
                {
                    return FullTitle;
                }
                return Title;
            }
        }
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
        public string FullTitle
        {
            get
            {
                return Title + " - " + CycleTitle;
            }
        }
        private bool AnonymousVoting = false;
        public Summary Summary;

        public Game(int id, Player overlord)
        {
            Id = id;
            Overlord = overlord;
            MyPlayers.Add(overlord);
            isInProgress = false;
            Summary = new Summary(this);
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

        public int GetCountOfPlayersOnTeam(Team team)
        {
            int count = 0;
            foreach (Player p in Players)
            {
                if (p.Role != null && p.Team.Equals(team)) count++;
            }
            return count;
        }

        public int GetCountOfPlayersWithRole(Role role)
        {
            int count = 0;
            foreach (Player p in Players)
            {
                if (p.Role != null && p.Role.Equals(role)) count++;
            }
            return count;
        }

        public List<Player> GetLivingPlayers()
        {
            List<Player> players = new List<Player>();
            foreach (Player p in Players)
            {
                if (p.IsAlive) players.Add(p);
            }
            return players;
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
        /// Generates a list of active teams from the Players in a given game. No duplicates!
        /// </summary>
        /// <returns></returns>
        public List<Team> GetTeamsPlayingUnique()
        {
            List<Team> Teams = new List<Team>();
            foreach (Player p in Players)
            {
                if (p.Role != null && !Teams.Contains(p.Team)) Teams.Add(p.Team);
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
            Summary.EndCycle();
            if (ActiveCycle == Cycle.Day)
            {
                MyCycle = Cycle.Night;
            }
            else
            {
                MyCycle = Cycle.Day;
                RoundCounter++;
            }
            //Reset players
            foreach (Player p in Players)
            {
                p.Vote = null;
                p.ClearNightActions();
                p.IsProtected = false;
            }
            Summary.NewCycle(this);
        }

        public bool Start()
        {
            if (Players.Count < 3) return false;
            GameSystem gameSystem = GameSystem.Instance;
            List<Type> roleTypes = gameSystem.GetRoleTypes();

            //Assign roles
            AssignRoles(roleTypes);

            //Initialize game
            RoundCounter++;
            MyCycle = Cycle.Day;
            isInProgress = true;

            //Iterate again after roles are finalized to setup the players and notify them.
            foreach (Player p in Players)
            {
                Role r = p.Role;
                string body = FlavorText.GetStartGameMessage();
                body += FlavorText.Divider + "You have been assigned the role of <b>" + r.Name + "</b> and are on the <b>" + r.Team.Name + "</b> team.<br />";
                string teammates = ListTeammates(p);
                if (String.IsNullOrEmpty(teammates)) teammates = "You have no teammates.";
                if (r.Team.KnowsTeammates) body += "Teammates:<br />" + teammates;
                body += FlavorText.Divider + CycleTitle + " has begun, use the commands below to take an action.";
                body += FlavorText.Divider + this.Status();
                body += FlavorText.Divider + this.Help(p);
                Gmail.MessagePlayer(p, this, body);
            }

            Summary.NewCycle(this);

            return true;
        }

        private List<Team> GetMajorTeamsAvailable(List<Type> roleTypes)
        {
            List<Team> majors = new List<Team>();
            foreach (Type type in roleTypes)
            {
                Role role = (Role)Activator.CreateInstance(type);
                if (role.Team.IsMajor && !majors.Contains(role.Team)) majors.Add(role.Team);
            }
            return majors;
        }

        private int GetMajorTeamsPlayingCount()
        {
            List<Team> teams = new List<Team>();
            foreach (Team team in GetTeamsPlayingUnique())
            {
                if (team.IsMajor) teams.Add(team);
            }
            return teams.Count;
        }

        public void AssignRoles(List<Type> roleTypes)
        {
            //Randomize the list of players
            foreach (Player p in MathX.RandomizedList(Players))
            {
                if (GetMajorTeamsPlayingCount() < 2)
                {
                    //If we have no teams yet we don't care, set team to null
                    List<Team> teams = GetTeamsPlayingUnique();
                    Team team = null;
                    //We have at least 1 team, set the team to that team, and thisTeam to false to pick a different team
                    if (teams.Count > 0) team = teams[0];
                    //We don't have enough major teams, assign another one
                    AssignRoleMajorTeam(p, team, false, roleTypes);
                }
                else
                {
                    AssignRole(p, GetNextTeam(), true, roleTypes);
                }
            }
        }

        /// <summary>
        /// Determines the next team that needs to have a player assigned to it, or null if any.  Precedence is given to major teams.
        /// </summary>
        /// <returns>The next team that needs a player</returns>
        private Team GetNextTeam()
        {
            //Give precedence to Major teams
            foreach (Team team in GetTeamsPlayingUnique())
            {
                if (team.IsMajor)
                {
                    int min = MathX.Percent(Players.Count, team.MinPercentComposition);
                    if (GetCountOfPlayersOnTeam(team) < min) return team;
                }
            }
            //Check other teams
            foreach (Team team in GetTeamsPlayingUnique())
            {
                int min = MathX.Percent(Players.Count, team.MinPercentComposition);
                if (GetCountOfPlayersOnTeam(team) < min) return team;
            }
            return null;
        }

        private void AssignRoleMajorTeam(Player player, Team team, bool thisTeam, List<Type> roleTypes)
        {
            AssignRole(player, team, thisTeam, roleTypes);
            while (!player.Team.IsMajor)
            {
                AssignRole(player, team, thisTeam, roleTypes);
            }
        }

        /// <summary>
        /// Assigns a random role to a player.
        /// <para />
        /// If team is null, it can be any team.  Otherwise, if thisTeam is true it must be the same team as team.  If thisTeam is false, it must be a different team than team.
        /// </summary>
        /// <param name="player">Player to assign the role to</param>
        /// <param name="team">The team the assigned role must match or not match depending on thisTeam.  If null, any team is allowed.</param>
        /// <param name="thisTeam">Whether or not the assigned role should match or mismatch the team parameter</param>
        /// <param name="roleTypes">List of available roles, by class type</param>
        private void AssignRole(Player player, Team team, bool thisTeam, List<Type> roleTypes)
        {
            player.Role = GetRandomRole(roleTypes, team, thisTeam);
        }

        private bool ValidTeam(Team team, Role role, bool thisTeam)
        {
            //Whether or not any team is allowed
            bool anyTeam = team == null;
            bool teamIsValid = true;
            if (!anyTeam)
            {
                //Any team is not allowed; check that the team is valid
                teamIsValid = role.Team.Equals(team) && thisTeam || !role.Team.Equals(team) && !thisTeam;
            }
            return teamIsValid;
        }

        /// <summary>
        /// Checks status of all roles and playres and randomly picks a role from the list of <see cref="GetAllowedRoles(List{Type}, Team, bool)"/>
        /// </summary>
        /// <param name="roleTypes"></param>
        /// <param name="team"></param>
        /// <param name="thisTeam"></param>
        /// <returns>The legal, randomly chosen role</returns>
        private Role GetRandomRole(List<Type> roleTypes, Team team, bool thisTeam)
        {
            Type type = (Type)GetAllowedRoles(roleTypes, team, thisTeam).PickOne();
            Role role = (Role)Activator.CreateInstance(type);
            return role;
        }

        public List<Type> GetAllowedRoles(List<Type> roleTypes, Team team, bool thisTeam)
        {
            List<Type> allowedRoles = new List<Type>();
            allowedRoles.AddRange(roleTypes);
            for (int i = roleTypes.Count - 1; i >= 0; i--)
            {
                Role role = (Role)Activator.CreateInstance(roleTypes[i]);
                if (MaxPlayersForRoleReached(role))
                {
                    allowedRoles.RemoveAt(i);
                }
                else if (!ValidTeam(team, role, thisTeam))
                {
                    allowedRoles.RemoveAt(i);
                }
            }
            return allowedRoles;
        }

        /// <summary>
        /// Implements the properties <see cref="Role.MaxPercentage"/> and <see cref="Role.MaxPlayers"/>.
        /// </summary>
        /// <param name="role"></param>
        /// <returns>True if either property is exceeded, false otherwise.</returns>
        public bool MaxPlayersForRoleReached(Role role)
        {
            int playerCount = GetCountOfPlayersWithRole(role) + 1;
            int maxPercent = MathX.Percent(Players.Count, role.MaxPercentage);
            //1 player is okay for any role
            if (playerCount == 1) return false;
            if (playerCount >= maxPercent || playerCount >= role.MaxPlayers)
            {
                return true;
            }
            return false;
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

        public void CheckEndOfCycle()
        {
            foreach (Player p in Players)
            {
                if (ActiveCycle == Cycle.Day && p.IsAlive && p.Vote == null) return;
                if (ActiveCycle == Cycle.Night && p.IsAlive && p.Actions.Count == 0) return;
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
                if (p.IsAlive)
                {
                    candidates.Add(p.Vote.Candidate);
                    if (!AnonymousVoting) votingResults.Add(p.Name.b() + " voted for: " + p.Vote.Candidate.Name.i());
                }
            }
            Dictionary<Player, int> candidateCounts = candidates.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            //Get 50% of players rounded up, the minimum for a majority
            int max = MathX.Percent(GetLivingPlayers().Count, 51);
            Player electee = null;
            //Tally votes
            foreach (KeyValuePair<Player, int> c in candidateCounts)
            {
                //We found someone, but keep processing to record all the results.
                if (c.Value >= max) electee = c.Key;
                if (AnonymousVoting) votingResults.Add(c.Key.Name + ": " + c.Value);
            }
            string voteMessage = "";
            //The first 2 if statements can't trigger a game end condition because no one dies.
            if (electee == null)
            {
                voteMessage = "There was no majority vote.";
            }
            else if (electee.Name.ToLowerInvariant().Equals("no one"))
            {
                voteMessage = "The majority vote was for no one, everyone is safe today.  But, at least one of you will probably regret it tonight...";
            }
            else
            {
                //Mark electee as dead then check if the game is over
                electee.Kill(null);
                voteMessage = String.Format(FlavorText.PlayerOutcastMessage, electee.Name.b());
            }
            Summary.AddEventLi("Voting Results: " + voteMessage);
            Summary.AddDetailEvent(votingResults.HtmlBulletList());
            ShowVotes(votingResults, voteMessage);

            if (IsGameOver()) return;
            NextCycle();
            foreach (Player p in Players)
            {
                string message = p.Role.Instructions;
                string teammates = ListTeammates(p);
                if (p.Team.KnowsTeammates && !String.IsNullOrEmpty(teammates)) message += FlavorText.Divider + teammates;
                if (p.IsAlive) Gmail.MessagePlayer(p, this, message);
            }


        }

        private void ShowVotes(List<string> votingResults, string resultMessage)
        {
            string message = CycleTitle + " has come to an end. Votes have been tallied: ";
            message += votingResults.HtmlBulletList();
            message += resultMessage;
            Gmail.MessageAllPlayers(this, message);
        }

        private void EndOfNight()
        {
            List<string> nightSummary = new List<string>();
            //This is very inefficient, but even with a game of 20 players, it will only take 100 iterations, which is trivial
            for (int i = 0; i < 5; i++)
            {
                foreach (Player p in Players)
                {
                    if (p.IsAlive && p.Role.NightActionPriority == i)
                    {
                        string msg = p.DoActions(this);
                        if (!String.IsNullOrEmpty(msg) && !nightSummary.Contains(msg)) nightSummary.Add(msg);
                    }
                }
            }

            string message = "The night has come to a close, ";
            if (nightSummary.Count > 0) message += "this is what happened:<br />" + nightSummary.HtmlBulletList();
            else message = "it was completely uneventful.";
            Gmail.MessageAllPlayers(this, message);

            if (!IsGameOver())
            {
                NextCycle();
            }
        }

        public List<Player> GetLivingPlayersOnMyTeam(Player player)
        {
            return GetLivingPlayersOnTeam(player.Team);
        }

        public List<Player> GetLivingPlayersOnTeam(Team team)
        {
            List<Player> pList = new List<Player>();
            foreach (Player p in Players)
            {
                if (p.IsAlive && p.Team.Equals(team)) pList.Add(p);
            }
            return pList;
        }

        public bool IsGameOver()
        {
            List<Player> winners = new List<Player>();
            foreach (Player p in Players)
            {
                if (p.HaveIWon(this)) winners.Add(p);
            }
            if (winners.Count > 0)
            {
                GameOver(winners);
                return true;
            }
            return false;
        }

        private void GameOver(List<Player> winners)
        {
            List<string> winnersList = new List<string>();
            foreach (Player w in winners)
            {
                string msg = StringX.b(w.Name) + " as " + StringX.b(w.Role.Name) + " for the " + StringX.b(w.Team.Name);
                winnersList.Add(msg);
            }
            List<string> othersList = new List<string>();
            List<Player> others = Players.Except(winners).ToList();
            foreach (Player o in others)
            {
                string msg = StringX.b(o.Name) + " as " + StringX.b(o.Role.Name) + " for the " + StringX.b(o.Team.Name);
                othersList.Add(msg);
            }
            isInProgress = false;
            string message = "The game is over.  Winners:".li();
            message += winnersList.HtmlBulletList();
            message += "Everyone else:".li();
            message += othersList.HtmlBulletList();
            Summary.EndCycle();
            Summary.AddEvent(message);
            Gmail.MessageAllPlayers(this, message.tag("ul"));
            Summary.GameOver(this);
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
