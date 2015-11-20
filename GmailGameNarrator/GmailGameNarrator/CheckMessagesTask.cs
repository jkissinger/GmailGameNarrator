using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator
{
    class CheckMessagesTask
    {
        /// <summary>
        /// Initializes the timer task.
        /// The task checks for email messages every time it's triggered by the timer.
        /// </summary>
        public static void TimerTask(object StateObj)
        {
            TaskState state = (TaskState)StateObj;
            if (state.TimerCanceled) state.TimerReference.Dispose();

            Start();
        }

        /// <summary>
        /// Starts checking email messages
        /// </summary>
        private static void Start()
        {
            IList<SimpleMessage> messages = GmailAPI.GetUnreadMessages();
            if (messages != null && messages.Count > 0)
            {
                foreach (SimpleMessage msg in messages)
                {
                    Console.WriteLine("Subject: " + msg.Subject + " From: " + msg.From);
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
                        GmailAPI.SendMessage(msg.From, msg.Subject, response);
                    }
                    GmailAPI.MarkMessageRead(msg.Message.Id);
                }
            }
        }

        /// <summary>
        /// Returns a random compliment, if the email has my wife's name in the from field
        /// </summary>
        private static string Compliment()
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
        private static string RandomResponse()
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
