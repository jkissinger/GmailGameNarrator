using GmailGameNarrator.Narrator;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GmailGameNarrator.Threads
{
    public class CheckMessagesTask : TimerThread
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object taskLock = new object();
        protected override string InitMessage
        {
            get { return "Initializing CheckMessagesTask.  Checking unread messages every " + Program.CheckMessagesInterval + " second(s)."; }
        }

        /// <summary>
        /// Starts checking messages
        /// </summary>
        public override void Start()
        {
            if (!Monitor.TryEnter(taskLock))
            {
                log.Warn("CheckMessagesTask.Start() is in use, skipping execution.");
                return;
            }

            if (GmailRequestBackoff.Backoff) return;

            GetUnreadMessages();

            Monitor.Exit(taskLock);
        }

        private void GetUnreadMessages()
        {
            IList<SimpleMessage> messages = Gmail.GetUnreadMessages();
            if (messages != null && messages.Count > 0)
            {
                foreach (SimpleMessage msg in messages)
                {
                    log.Debug("Message found: Subject: " + msg.Subject + " From: " + msg.From);
                    try {
                        MessageParser.Instance.ParseMessage(msg);
                    } catch (Exception e)
                    {
                        log.Error("Error parsing email.", e);
                        Gmail.MessagePerson(msg.From, "Error", "There was an error processing your last email, restart the game.");
                    }
                    Gmail.MarkMessageRead(msg.Message.Id);
                    log.Info("Message processed: Subject: " + msg.Subject + " From: " + msg.From);
                }
            }
        }
    }
}
