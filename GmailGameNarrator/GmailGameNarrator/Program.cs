using System;

namespace GmailGameNarrator
{
    class Program
    {
        static void Main(string[] args)
        {
            GmailAPI.StartService();

            TaskState state = new TaskState();
            state.TimerCanceled = false;
            System.Threading.TimerCallback TimerDelegate =
                new System.Threading.TimerCallback(CheckMessagesTask.TimerTask);

            System.Threading.Timer TimerItem =
                new System.Threading.Timer(TimerDelegate, state, 5000, 5000);

            state.TimerReference = TimerItem;

            Console.Read();

            TimerItem.Dispose();
        }
    }
}
