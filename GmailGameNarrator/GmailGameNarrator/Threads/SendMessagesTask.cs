using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Threads
{
    class SendMessagesTask : TimerThread
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public override String InitMessage
        {
            get { return "Initializing SendMessagesTask.  Sending " + Program.SendMessagesBatchSize + " messages every " + Program.SendMessagesInterval + " second(s)."; }
        }

        /// <summary>
        /// Starts sending email messages
        /// </summary>
        public override void Start()
        {
            //If queue is empty skip this tick
            if (Program.Backoff) return;

            int i = 0;

            while (i < Program.SendMessagesBatchSize && Program.outgoingQueue.Count > 0)
            {
                SimpleMessage outgoing = new SimpleMessage();
                if (Program.outgoingQueue.TryDequeue(out outgoing))
                {
                    log.Info("Sending email " + outgoing.Subject + " to " + outgoing.To);
                    log.Debug("Body: " + outgoing.Body);
                    if (Gmail.SendMessage(outgoing.To, outgoing.Subject, outgoing.Body) == null)
                    {
                        outgoing.SendAttempts++;
                        if (outgoing.SendAttempts <= Program.MaxSendAttempts)
                        {
                            log.Warn("Failed to send email " + outgoing.Subject + " to " + outgoing.To + " retrying. Attempt " + outgoing.SendAttempts + ".");
                            log.Debug("Body: " + outgoing.Body);
                            Program.outgoingQueue.Enqueue(outgoing);
                        }
                        else
                        {
                            log.Error("Failed to send email " + outgoing.Subject + " to " + outgoing.To + " body " + outgoing.Body + "too many times.  WILL NOT RETRY.");
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
