﻿namespace GmailGameNarrator
{
    class TaskState
    {
        /// <summary>
        /// A reference to the timer object
        /// </summary>
        public System.Threading.Timer TimerReference;
        /// <summary>
        /// True if the timer has been canceled; false otherwise
        /// </summary>
        public bool TimerCanceled;
    }
}
