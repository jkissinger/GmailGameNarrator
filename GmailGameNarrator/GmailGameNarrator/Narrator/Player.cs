using GmailGameNarrator.Narrator.Roles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GmailGameNarrator.Narrator
{
    public class Player
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Name { get; }
        public string Address { get; }
        public bool IsAlive { get; private set; }
        public bool IsProtected { get; set; }
        public Role Role { get; set; }
        public int BittenRound = -1;
        public Team Team
        {
            get
            {
                return Role.Team;
            }
        }
        public Vote Vote { get; set; }
        //TODO Have vote, day actions, stored here as well
        public ReadOnlyCollection<Action> Actions
        {
            get
            {
                return new ReadOnlyCollection<Action>(MyActions);
            }
        }
        private IList<Action> MyActions = new List<Action>();

        public Player(string name, string address)
        {
            Name = name;
            Address = address.Trim().ToLowerInvariant();
            IsAlive = true;
        }

        /// <summary>
        /// Returns true if the attack succeeds, false otherwise.
        /// </summary>
        /// <param name="attackerRole">The role of the player making the attack</param>
        /// <param name="isDeadly">Whether or not the attack will kill the player</param>
        /// <returns>True if the attack succeeds, false otherwise.</returns>
        public bool Attack(Role attackerRole, bool isDeadly)
        {
            if (!IsProtected)
            {
                if (isDeadly)
                {
                    Role.KilledBy = attackerRole;
                    IsAlive = false;
                }
                return true;
            }
            return false;
        }

        public void Quit()
        {
            IsAlive = false;
        }

        public bool HaveIWon(Game game)
        {
            return Role.HaveIWon(this, game);
        }

        public string AddNightAction(Action action, Game game)
        {
            string result = Role.ValidateAction(this, action, game);
            if (String.IsNullOrEmpty(result))
            {
                MyActions.Add(action);
            }
            return result;
        }

        public string DoActions(Game game)
        {
            string result = "";
            if (game.ActiveCycle == Game.Cycle.Night)
            {
                if (IsAlive) result += Role.DoNightActions(this, game);
            }
            else Role.DoDayActions(this, game);
            return result;
        }

        public string CheckForZombieBite(Game game)
        {
            if (Role.IsInfectionImmune) return "";
            string message = "";
            string result = "";
            if (game.RoundCounter == BittenRound + 1)
            {
                if (MathX.PercentChance(ZombieMaster.ChanceToTurn))
                {
                    IsAlive = false;
                    message += "You succumbed to the zombie bite last night, and have turned into a zombie! You are now dead.";
                    result += "Just as the sun set completely, " + Name.b() + " turned into a zombie!  You quickly banded together and put them out of their misery.  But one of you may have been bitten in the process...";
                    if (MathX.PercentChance(ZombieMaster.ChanceToTurn))
                    {
                        //this bypasses protection because it technically happens before night starts
                        Player bitten = (Player)game.GetLivingPlayers().PickOne();
                        message += " After turning into a zombie, you bit " + bitten.Name.b() + ".";
                        bitten.BittenRound = game.RoundCounter;
                        Gmail.MessagePlayer(bitten, game, "While putting " + Name.b() + " down, you were bitten! Hopefully you won't turn tomorrow night...");
                    }
                }
                else
                {
                    message += "You shrugged off the effects of the zombie bite and are ready to perform your normal night actions.";
                }
                game.Summary.AddEventLi(game.CycleTitle + " - " + Name.b() + ": " + message);
                Gmail.MessagePlayer(this, game, message);
            }
            return result;
        }

        public void Reset()
        {
            Vote = null;
            MyActions.Clear();
            IsProtected = false;
        }

        //==========================
        //= Object overrides below =
        //==========================
        public override string ToString()
        {
            return Address + " playing as " + Name;
        }

        public override bool Equals(object obj)
        {
            try
            {
                Player p = (Player)obj;
                if (p.Address.Equals(Address)) return true;
            }
            catch (InvalidCastException e)
            {
                log.Warn("Attempted to compare non player object to a player. Error: " + e.Message);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
    }
}
