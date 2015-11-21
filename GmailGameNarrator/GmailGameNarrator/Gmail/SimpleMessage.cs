using Google.Apis.Gmail.v1.Data;
using System;
using System.Collections.Generic;

namespace GmailGameNarrator
{
    class SimpleMessage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The original Gmail API Message
        /// </summary>
        public Message Message { get; set; }
        /// <summary>
        /// The message subject
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// The message body
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// The email address of the sender
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// The date time the message was sent
        /// </summary>
        public DateTime SentTime { get; set; }
        /// <summary>
        /// The message destination email address
        /// </summary>
        public string To { get; internal set; }
        /// <summary>
        /// Number of times the message has been attempted to be sent
        /// </summary>
        public int SendAttempts { get; internal set; }

        public string FromEmailAddress()
        {
            string email = From;
            try
            {
                int idx = From.IndexOf('<');
                email = From.Substring(idx + 1, From.Length - 1 - idx);
            }
            catch (Exception e)
            {
                log.Error("Malformed email address: " + From, e);
            }
            return email;
        }

        public List<string> BodyAsLines()
        {
            string body = Body.Replace('\r'.ToString(), string.Empty);
            return new List<string>(body.Split('\n'));
        }
    }
}
