using GmailGameNarrator.Game;
using GmailGameNarrator.Threads;
using System;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace GmailGameNarrator
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static bool running = true;
        public static readonly int CheckMessagesInterval = 5;
        public static readonly int SendMessagesInterval = 1;
        public static readonly int SendMessagesBatchSize = 2;
        public static readonly int MaxSendAttempts = 3;

        /// <summary>
        /// Begins the Gmail Game Narrator program.
        /// </summary>
        /// <param name="args">Unused</param>
        /// <remarks>
        /// The term email is too ambiguous and should not be used, instead use message and address respectively.
        /// </remarks>
        static void Main(string[] args)
        {
            log.Info("Initializing program.");
            Gmail.StartService();
            //Make sure these are instantiated first for thread safety
            GameSystem ns = GameSystem.Instance;
            MessageParser mp = MessageParser.Instance;

            CheckMessagesTask CheckTask = new CheckMessagesTask();
            TaskState CheckMessagesState = CheckTask.Init();

            SendMessagesTask SendTask = new SendMessagesTask();
            TaskState SendMessagesState = SendTask.Init();

            ///Not implemented because I don't know the syntax for the Gmail API returning
            ///403 rate limited responses
            //GmailRequestBackoff BackoffThread = new GmailRequestBackoff();
            //TaskState BackoffState = BackoffThread.Init();

            while (running) { };

            CheckMessagesState.TimerCanceled = true;
            SendMessagesState.TimerCanceled = true;
            //BackoffState.TimerCanceled = true;
        }
    }
}
