using GmailGameNarrator.Game;
using GmailGameNarrator.Threads;
using System;
using System.IO;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace GmailGameNarrator
{
    public class Program
    {
#if DEBUG
        public static readonly string BaseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\"));
#else
        public static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
#endif
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static bool running = true;
        public static readonly int CheckMessagesInterval = 5;
        public static readonly int SendMessagesInterval = 1;
        public static readonly int SendMessagesBatchSize = 2;
        public static readonly int MaxSendAttempts = 3;
        public static readonly string License = "GmailGameNarrator  Copyright (C) 2015  Jonathan Kissinger\n\n" +
            "This program is free software: you can redistribute it and/or modify\n" +
            "it under the terms of the GNU General Public License as published by\n" +
            "the Free Software Foundation, either version 3 of the License, or\n" +
            "(at your option) any later version.\n\n" +
            "This program is distributed in the hope that it will be useful,\n" +
            "but WITHOUT ANY WARRANTY; without even the implied warranty of\n" +
            "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the\n" +
            "<a href=http://www.gnu.org/licenses/>GNU General Public License</a> for more details.\n";
        public static readonly string GitHub = "<a href=http://github.com/jkissinger/GmailGameNarrator>GitHub</a>";

        public static string UnitTestAddress = "@unit.test";

        /// <summary>
        /// Begins the Gmail Game Narrator program.
        /// </summary>
        /// <param name="args">Unused</param>
        /// <remarks>
        /// The term email is too ambiguous and should not be used, instead use message and address respectively.
        /// </remarks>
        static void Main(string[] args)
        {
            log.Info("License info:\n" + License);
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
