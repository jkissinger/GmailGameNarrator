using GmailGameNarrator.Threads;
using System;
using System.Collections.Concurrent;

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
        public static ConcurrentQueue<SimpleMessage> outgoingQueue = new ConcurrentQueue<SimpleMessage>();

        static void Main(string[] args)
        {
            log.Info("Initializing program.");
            Gmail.StartService();

            CheckMessagesTask CheckTask = new CheckMessagesTask();
            TaskState CheckMessagesState = CheckTask.Init();

            SendMessagesTask SendTask = new SendMessagesTask();
            TaskState SendMessagesState = SendTask.Init();

            Console.Read();

            CheckMessagesState.TimerReference.Dispose();
            SendMessagesState.TimerReference.Dispose();
        }

        public static void EnqueueEmail(String to, String subject, String body)
        {
            SimpleMessage outgoing = new SimpleMessage();
            outgoing.To = to;
            outgoing.Subject = subject;
            outgoing.Body = body;
            outgoing.SendAttempts = 0;
            Program.outgoingQueue.Enqueue(outgoing);
        }
    }
}
