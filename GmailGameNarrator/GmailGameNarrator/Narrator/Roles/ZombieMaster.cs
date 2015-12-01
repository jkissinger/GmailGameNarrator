using GmailGameNarrator.Narrator.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Narrator.Roles
{
    class ZombieMaster : Role
    {
        public static int ChanceToTurn = 75;
        private List<Player> TheBitten = new List<Player>();

        public ZombieMaster()
        {
            Name = "Zombie Master";
            Team = new SoloTeam();
            ActionText = "Bite";
            Description = "You are immmune to the zombie infection and have dubbed yourself the " + Name.b() + ".  Each night you may bite someone, there is a " + ChanceToTurn + "% chance they will turn into a zombie the next night, and a " + ChanceToTurn + "% chance they will bite someone else.";
            Instructions = "At night, send a message with \"" + ActionText.b() + " name".i() + "\" where " + "name".i() + " is the player you want to " + ActionText.b() + ".  There is a " + ChanceToTurn + "% chance they will turn into a zombie at the beginning of the next night.";
            NightActionPriority = 3;
            MaxPercentage = 20;
            Prevalence = 10;
            IsAttacker = true;
            IsInfectionImmune = true;
        }

        public override void PerformNightActions(Player player, Game game)
        {
            if (NightActionPriority == 0)
            {
                CheckForZombieBites(game);
            }
            else if (player.IsAlive)
            {
                Player nominee = player.MyAction.Target;
                string message = "";
                string result = "";
                if (nominee.Attack(this, false, false))
                {
                    TheBitten.Add(nominee);
                    message += "You bit " + nominee.Name.b() + "! There is a " + ChanceToTurn + "% chance they will turn into a zombie tomorrow night!";
                    result = " bit " + nominee.Name.b() + ".";
                }
                else
                {
                    message += "You were unable to bite " + nominee.Name.b() + " because someone was protecting them!";
                    result = " tried to bite " + nominee.Name.b() + " But they were protected.";
                }
                game.Summary.AddEventLi(game.CycleTitle + " - " + player.Name.b() + result);
                Gmail.MessagePlayer(player, game, message);
                NightActionPriority = 0;
            }
        }

        public void CheckForZombieBites(Game game)
        {
            List<Player> victims = new List<Player>();
            victims.AddRange(TheBitten);
            TheBitten.Clear();
            foreach (Player p in victims)
            {
                Player bitten = null;
                if (p.Role.IsInfectionImmune || !p.IsAlive) return;
                string message = "";
                if (MathX.PercentChance(ZombieMaster.ChanceToTurn))
                {
                    p.Attack(this, true, true);
                    message += "You succumbed to the zombie bite last night, and have turned into a zombie! You are now dead.";
                    string msg = "Just as the sun set completely, " + Name.b() + " turned into a zombie!  You quickly banded together and put them out of their misery.  But one of you may have been bitten in the process...";
                    game.NightEvents.Add(msg);
                    if (MathX.PercentChance(ZombieMaster.ChanceToTurn))
                    {
                        //this bypasses protection because it technically happens before night starts
                        bitten = (Player)game.GetLivingPlayers().PickOne();
                        message += " After turning into a zombie, you bit " + bitten.Name.b() + ".";
                        Gmail.MessagePlayer(bitten, game, "While putting " + Name.b() + " down, you were bitten! Hopefully you won't turn tomorrow night...");
                    }
                }
                else
                {
                    message += "You shrugged off the effects of the zombie bite and are ready to perform your normal night actions.";
                }
                game.Summary.AddEventLi(game.CycleTitle + " - " + Name.b() + ": " + message);
                Gmail.MessagePlayer(p, game, message);
                if (bitten != null) TheBitten.Add(bitten);
            }
            NightActionPriority = 3;
        }
    }
}
