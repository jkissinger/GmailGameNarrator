using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GmailGameNarrator.Threads
{
    abstract class TimerThread
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public TaskState state;
        abstract protected string InitMessage { get; }
        protected bool acquiredLock = false;

        /// <summary>
        /// Initializes the timer
        /// </summary>
        public TaskState Init()
        {
            int interval = Program.CheckMessagesInterval * 1000;
            log.Info(InitMessage);
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
        /// </summary>
        public void TimerTask(object StateObj)
        {
            this.state = (TaskState)StateObj;
            if (state.TimerCanceled) state.TimerReference.Dispose();

            Start();
        }

        public abstract void Start();
    }
}
