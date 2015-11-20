using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailGameNarrator.Threads
{
    class SendMessagesTask
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
            
        }
    }
}
