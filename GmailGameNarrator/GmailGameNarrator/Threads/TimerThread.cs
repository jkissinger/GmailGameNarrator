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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public TaskState State { get; set; }
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
            TimerCallback TimerDelegate = new TimerCallback(TimerTask);

            Timer TimerItem = new Timer(TimerDelegate, state, interval, interval);

            state.TimerReference = TimerItem;
            return state;
        }

        /// <summary>
        /// Initializes the timer task.
        /// </summary>
        public void TimerTask(object StateObj)
        {
            State = (TaskState)StateObj;
            if (State.TimerCanceled) State.TimerReference.Dispose();

            try
            {
                Start();
            }
            catch (Exception e)
            {
                log.Error("Task had an error.", e);
            }
        }

        public abstract void Start();
    }
}
