using System;
using System.Collections.Generic;
using System.IO;

namespace GmailGameNarrator.Narrator
{
    public class Summary
    {
        /// <summary>
        /// As game events happen a string is added to this list to record it.
        /// </summary>
        private List<string> events = new List<string>();
        private List<string> detailEvents = new List<string>();
        private List<string> emails = new List<string>();

        public Summary(Game game)
        {
            string line = game.Title.b() + " created with " + game.Overlord.b() + " as the Overlord.";
            line = line.tag("h1");
            AddEvent(line);
        }

        public void AddEmail(string to, string subject, string body)
        {
            emails.Add("<h1>" + subject + " sent to " + to + ":</h1>" + body.tag("p"));
        }

        public void AddEventLi(string line)
        {
            AddEvent(line.li());
        }

        public void AddEvent(string line)
        {
            detailEvents.Add(line);
            events.Add(line);
        }

        public void AddDetailEvent(string line)
        {
            detailEvents.Add(line);
        }

        public void AddUniqueEvent(string line)
        {
            if (!events.Contains(line)) AddEvent(line);
        }

        public void NewCycle(Game game)
        {
            string line = game.FullTitle + " has begun.";
            AddEvent(line.li());
            line = "<ul>";
            AddEvent(line);
            line = "Players:".li();
            AddDetailEvent(line);
            line = game.ListPlayersForSummary();
            AddDetailEvent(line);
        }

        public void EndCycle()
        {
            string line = "</ul>";
            AddEvent(line);
        }

        public void GameOver(Game game)
        {
            string line = "Final game status:".li();
            AddDetailEvent(line);
            line = game.ListPlayersForSummary();
            AddDetailEvent(line);
            WriteFiles(Path.Combine(Program.BaseDir, DateTime.Now.ToString("yyyy-MM-dd") + " - " + game.Title));
        }

        private void WriteFiles(string fileName)
        {
            File.WriteAllLines(fileName + ".html", events);
            File.WriteAllLines(fileName + " - Details.html", detailEvents);
            File.WriteAllLines(fileName + " - Emails.html", emails);
        }
    }
}
