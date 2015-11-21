using System.Threading;

namespace GmailGameNarrator.Threads
{
    class SendMessagesTask : TimerThread
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object taskLock = new object();
        protected override string InitMessage
        {
            get { return "Initializing SendMessagesTask.  Sending " + Program.SendMessagesBatchSize + " messages every " + Program.SendMessagesInterval + " second(s)."; }
        }

        /// <summary>
        /// Starts sending messages
        /// </summary>
        public override void Start()
        {
            if (!Monitor.TryEnter(taskLock))
            {
                log.Warn("SendMessagesTask.Start() is in use, skipping execution.");
                return;
            }

            if (GmailRequestBackoff.Backoff) return;

            SendMessages();

            Monitor.Exit(taskLock);
        }

        private void SendMessages()
        {
            int i = 0;

            while (i < Program.SendMessagesBatchSize && Gmail.outgoingQueue.Count > 0)
            {
                SimpleMessage outgoing = new SimpleMessage();
                if (Gmail.outgoingQueue.TryDequeue(out outgoing))
                {
                    log.Info("Sending message " + outgoing.Subject + " to " + outgoing.To);
                    log.Debug("Body: " + outgoing.Body);
                    if (Gmail.SendMessage(outgoing.To, outgoing.Subject, outgoing.Body) == null)
                    {
                        outgoing.SendAttempts++;
                        if (outgoing.SendAttempts <= Program.MaxSendAttempts)
                        {
                            log.Warn("Failed to send message " + outgoing.Subject + " to " + outgoing.To + " retrying. Attempt " + outgoing.SendAttempts + ".");
                            log.Debug("Body: " + outgoing.Body);
                            Gmail.outgoingQueue.Enqueue(outgoing);
                        }
                        else
                        {
                            log.Error("Failed to send message " + outgoing.Subject + " to " + outgoing.To + " body " + outgoing.Body + "too many times.  WILL NOT RETRY.");
                        }
                    }
                }
                else
                {
                    log.Warn("Attempt to dequeue message failed.");
                }

                i++;
            }
        }
    }
}
