using GmailGameNarrator.Threads;
using System;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace GmailGameNarrator
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log.Warn("Initializing program.");
            Gmail.StartService();

            CheckMessagesTask CheckTask = new CheckMessagesTask();
            TaskState CheckMessagesState = CheckTask.Init();

            Console.Read();

            CheckMessagesState.TimerReference.Dispose();
        }
    }
}
