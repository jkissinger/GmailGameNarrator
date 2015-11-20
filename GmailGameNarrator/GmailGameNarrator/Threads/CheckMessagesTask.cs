using GmailGameNarrator.Threads;
using System;
using System.Collections.Generic;

namespace GmailGameNarrator.Threads
{
    class CheckMessagesTask : TimerThread
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public override String InitMessage
        {
            get { return "Initializing CheckMessagesTask.  Checking unread messages every " + Program.CheckMessagesInterval + " second(s)."; }
        }

        /// <summary>
        /// Starts checking email messages
        /// </summary>
        public override void Start()
        {
            if (Program.Backoff) return;

            IList<SimpleMessage> messages = Gmail.GetUnreadMessages();
            if (messages != null && messages.Count > 0)
            {
                foreach (SimpleMessage msg in messages)
                {
                    log.Debug("Message found: Subject: " + msg.Subject + " From: " + msg.From);
                    if (msg.Subject.Equals("What am I?") || msg.Subject.Equals("Re: What am I?"))
                    {
                        String response = "";
                        if (msg.From.IndexOf("alysha", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            response = "You are " + Compliment();
                        }
                        else
                        {
                            response = "You are " + RandomResponse();
                        }
                        Program.EnqueueEmail(msg.From, msg.Subject, response);
                    }
                    Gmail.MarkMessageRead(msg.Message.Id);
                }
            }
        }

        /// <summary>
        /// Returns a random compliment, if the email has my wife's name in the from field
        /// </summary>
        private string Compliment()
        {
            Random r = new Random();

            switch (r.Next(0, 4))
            {
                case 0:
                    return "the most beautiful creature in the world.";
                case 1:
                    return "the most brilliant wife on the planet.";
                case 2:
                    return "going to get better, be happy, and have a clean house soon.";
                case 3:
                    return "far too tolerant of your husband's hyperfocus when programming.";
                default:
                    return RandomResponse();
            }
        }

        /// <summary>
        /// Returns a random response; ignore my sense of humor
        /// </summary>
        private string RandomResponse()
        {
            Random r = new Random();

            switch (r.Next(0, 4))
            {
                case 0:
                    return "wasting your time messaging a computer program.";
                case 1:
                    return "a toaster?  I don't date toasters, swipe left.";
                case 2:
                    return "...I don't know, but what am I? ...wait that's not right.";
                case 3:
                    return "bored.  Most likely.";
                default:
                    return "dumbfounding!";
            }
        }
    }
}
