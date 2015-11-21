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
        /// The address of the sender
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// The date time the message was sent
        /// </summary>
        public DateTime SentTime { get; set; }
        /// <summary>
        /// The message destination address
        /// </summary>
        public string To { get; internal set; }
        /// <summary>
        /// Number of times the message has been attempted to be sent
        /// </summary>
        public int SendAttempts { get; internal set; }

        /// <summary>
        /// This may be unncessary and could add complications, consider removing and just using From.
        /// </summary>
        /// <returns></returns>
        public string FromAddress()
        {
            string address = From;
            try
            {
                address = StringX.GetTextAfter(From, "<");
                address = address.Remove(address.Length - 1);
            }
            catch (Exception e)
            {
                log.Error("Malformed address: " + From, e);
            }
            return address.ToLowerInvariant();
        }

        public List<string> BodyAsLines()
        {
            string body = Body.Replace('\r'.ToString(), string.Empty);
            List<string> lines = new List<string>();
            foreach(string l in body.Split('\n'))
            {
                string line = l.Trim().ToLowerInvariant();
                if(!String.IsNullOrEmpty(line)  && !line.StartsWith(">")) lines.Add(line);
            }
            return lines;
        }

        public string NiceSubject()
        {
            return Subject.Trim().ToLowerInvariant();
        }
    }
}
