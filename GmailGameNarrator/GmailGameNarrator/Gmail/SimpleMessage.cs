using Google.Apis.Gmail.v1.Data;
using System;

namespace GmailGameNarrator
{
    class SimpleMessage
    {
        /// <summary>
        /// The original Gmail API Message
        /// </summary>
        public Message Message { get; set; }
        /// <summary>
        /// The message subject
        /// </summary>
        public String Subject { get; set; }
        /// <summary>
        /// The message body
        /// </summary>
        public String Body { get; set; }
        /// <summary>
        /// The email address of the sender
        /// </summary>
        public String From { get; set; }
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
    }
}
