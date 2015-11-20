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
        /// The sender of the message
        /// </summary>
        public String From { get; set; }
        /// <summary>
        /// The date time the message was sent
        /// </summary>
        public DateTime SentTime { get; set; }
    }
}
