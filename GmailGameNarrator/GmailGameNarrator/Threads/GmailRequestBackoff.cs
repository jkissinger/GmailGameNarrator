using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GmailGameNarrator.Threads
{
    /// <summary>
    /// Implements the exponential backoff as recommended by the Gmail API
    /// </summary>
    class GmailRequestBackoff : TimerThread
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object taskLock = new object();
        private static int Failures = 0;
        public static volatile bool Backoff = false;
        public static volatile bool LastRequestState = true;
        protected override string InitMessage
        {
            get { return "Initializing GmailRequestBackoff thread."; }
        }

        /// <summary>
        /// Delays messages to the Gmail API as necessary
        /// </summary>
        public override void Start()
        {
            if (!Monitor.TryEnter(taskLock))
            {
                log.Warn("GmailRequestBackoff.Start() is in use, skipping execution.");
                return;
            }

            CheckBackoff();

            Monitor.Exit(taskLock);
        }

        private void CheckBackoff()
        {
            if (Backoff)
            {
                if (LastRequestState)
                {
                    //Had a failure, but also a success since the last failure, reset failures counter
                    Failures = 0;
                }
                else
                {
                    //Had successive failures, increment failures
                    Failures++;
                }
                //Do backoff
                Wait();
                //We backed off, reset it
                Backoff = false;
                //Last time was a failure, set the state; note this is the only way LastRequestState can be set to false
                LastRequestState = false;

            }
            //If backoff is false we don't care, either we're waiting for a request to be made and find out the result
            //or it's working (if LastRequestState is true).
        }

        private void Wait()
        {
            //Exponential delay in seconds, minimum of 1 second
            int delay = (int)Math.Pow(2, Failures);
            Thread.Sleep(delay * 1000);
            Backoff = false;
            LastRequestState = false;
        }
    }
}
