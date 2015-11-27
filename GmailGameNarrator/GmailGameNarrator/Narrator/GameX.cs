using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Narrator
{
    /// <summary>
    /// Static extension class of <see cref="Game"/>.  No logic, just methods returning formatted strings.
    /// </summary>
    static class GameX
    {
        public static string Status(this Game game)
        {
            string status = "Status of " + game.Title + ":<br /><ul>"
                + "<li>In progress: " + (game.IsInProgress() ? "Yes</li>" : "No</li>")
                + (game.IsInProgress() ? "<li>Cycle: " + game.CycleTitle + "</li>" : "")
                + "<li>Players:</li>" + game.ListPlayers() + "</ul>";
            return status;
        }

        public static string ListPlayers(this Game game)
        {
            string players = "<ul>";
            foreach (Player player in game.Players)
            {
                string livingState = "";
                string cycleStatus = "";
                if (game.IsInProgress())
                {
                    if (player.IsAlive)
                    {
                        livingState = " - <b>Alive</b>";
                        if (game.ActiveCycle == Game.Cycle.Day && player.Vote == null)
                        {
                            cycleStatus = " - <b><i>Waiting On Action</i></b>";
                        }
                        else if (game.ActiveCycle == Game.Cycle.Night && player.Actions.Count == 0)
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

        public static string ListPlayersForSummary(this Game game)
        {
            string players = "<ul>";
            foreach (Player player in game.Players)
            {
                string livingState = " - Dead";
                if (player.IsAlive)
                {
                    livingState = " - <b>Alive</b>";
                }
                players += (player + " as " + player.Role + " for " + player.Team + livingState).li();
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
        public static string Help(this Game game, Player player)
        {
            string commands = "<ul>";
            if (player.IsAlive && game.IsInProgress() && game.ActiveCycle == Game.Cycle.Day) commands += "<li>To vote for someone to be cast out: <b>Vote</b> <i>name</i></li>";
            if (game.IsOverlord(player) && !game.IsInProgress()) commands += "<li>To start the game: <b>Start</b>.</li>";
            if (game.IsOverlord(player)) commands += "<li>To cancel the game: <b>Cancel</b></li>";
            if (game.IsOverlord(player)) commands += "<li>To kick a player from the game: <b>Kick</b> <i>name</i></li>";
            if (game.IsOverlord(player)) commands += "<li>To ban a player from the game: <b>Ban</b> <i>name</i></li>";
            if (player.IsAlive && !game.IsOverlord(player)) commands += "<li>To quit the game: <b>Quit</b></li>";
            commands += "<li>To see the game status: <b>Status</b></li>";
            commands += "</ul>";
            commands += "<br />To use a command, reply to this email as indicated above.";
            return commands;
        }
    }
}
