using GmailGameNarrator.Game;
using GmailGameNarrator.Threads;
using System;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace GmailGameNarrator
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly int CheckMessagesInterval = 5;
        public static readonly int SendMessagesInterval = 1;
        public static readonly int SendMessagesBatchSize = 2;
        public static readonly int MaxSendAttempts = 3;
        public static volatile bool Backoff = false;
        public static volatile bool LastRequestState = true;

        static void Main(string[] args)
        {
            log.Info("Initializing program.");
            Gmail.StartService();
            //Make sure these are instantiated first for thread safety
            NarratorSystem ns = NarratorSystem.Instance;
            MessageParser mp = MessageParser.Instance;

            CheckMessagesTask CheckTask = new CheckMessagesTask();
            TaskState CheckMessagesState = CheckTask.Init();

            SendMessagesTask SendTask = new SendMessagesTask();
            TaskState SendMessagesState = SendTask.Init();

            ///Not implemented because I don't know the syntax for the Gmail API returning
            ///403 rate limited responses
            //GmailRequestBackoff BackoffThread = new GmailRequestBackoff();
            //TaskState BackoffState = BackoffThread.Init();

            Console.Read();

            CheckMessagesState.TimerCanceled = true;
            SendMessagesState.TimerCanceled = true;
            //BackoffState.TimerCanceled = true;
        }
    }
}
