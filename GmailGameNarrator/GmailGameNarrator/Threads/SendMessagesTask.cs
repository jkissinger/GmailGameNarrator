using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Threads
{
    class SendMessagesTask
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes the timer
        /// </summary>
        public TaskState Init()
        {
            int interval = Program.SendMessagesInterval * 1000;
            log.Info("Initializing SendMessagesTask.  Sending " + Program.SendMessagesBatchSize + " messages every " + Program.SendMessagesInterval + " second(s).");
            TaskState state = new TaskState();
            state.TimerCanceled = false;
            System.Threading.TimerCallback TimerDelegate =
                new System.Threading.TimerCallback(TimerTask);

            System.Threading.Timer TimerItem =
                new System.Threading.Timer(TimerDelegate, state, interval, interval);

            state.TimerReference = TimerItem;
            return state;
        }

        /// <summary>
        /// Initializes the timer task.
        /// The task sends queued messages every time it's triggered by the timer.
        /// </summary>
        public static void TimerTask(object StateObj)
        {
            TaskState state = (TaskState)StateObj;
            if (state.TimerCanceled) state.TimerReference.Dispose();

            Start();
        }

        /// <summary>
        /// Starts sending email messages
        /// </summary>
        private static void Start()
        {
            //If queue is empty skip this tick
            if (Program.outgoingQueue.Count == 0) return;

            for (int i = 0; i < Program.SendMessagesBatchSize; i++)
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
            }
        }
    }
}
