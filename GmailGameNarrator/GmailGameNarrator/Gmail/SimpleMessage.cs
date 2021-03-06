﻿using Google.Apis.Gmail.v1.Data;
using System;
using System.Collections.Generic;

namespace GmailGameNarrator
{
    public class SimpleMessage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        public string To { get; set; }
        /// <summary>
        /// Number of times the message has been attempted to be sent
        /// </summary>
        public int SendAttempts { get; set; }

        public List<string> BodyAsLines()
        {
            string body = Body.Replace('\r'.ToString(), string.Empty);
            List<string> lines = new List<string>();
            foreach(string l in body.Split('\n'))
            {
                string line = l.Trim().ToLowerInvariant();
                //Ignore everything after a blank line; we don't want to parse our own messages
                if(lines.Count > 0 && (String.IsNullOrEmpty(line) || line.StartsWith(">"))) break;
                lines.Add(line);
            }
            return lines;
        }
    }
}
