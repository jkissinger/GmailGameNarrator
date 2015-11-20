using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace GmailGameNarrator
{
    class Gmail
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static string[] Scopes = { GmailService.Scope.GmailSend, GmailService.Scope.GmailReadonly, GmailService.Scope.GmailModify };
        static string ApplicationName = "Gmail Game Narrator";
        static GmailService service;

        /// <summary>
        /// Starts the gmail service
        /// </summary>
        public static void StartService()
        {
            log.Info("Starting Gmail service.");
            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials", "gmail.game.narrator");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                log.Info("Credential file saved to: " + credPath);
            }

            service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            log.Info("Gmail service started.");
        }

        /// <summary>
        /// Gets a list of all the unread messages
        /// </summary>
        public static IList<SimpleMessage> GetUnreadMessages()
        {
            IList<SimpleMessage> messages = new List<SimpleMessage>();

            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List("me");
            request.LabelIds = new Google.Apis.Util.Repeatable<string>(new string[] { "UNREAD" });

            IList <Message> msglist = request.Execute().Messages;
            if (msglist != null && msglist.Count > 0)
            {
                foreach (var msg in msglist)
                {
                    messages.Add(GetMessageById(msg.Id));
                }
            }

            log.Debug("No unread messages found.");
            return messages;
        }

        /// <summary>
        /// Gets a specific message given an id, returned as a SimpleMessage
        /// </summary>
        /// <returns>
        /// The message converted to a SimpleMessage object
        /// </returns>
        private static SimpleMessage GetMessageById(String id)
        {
            log.Debug("Retrieving message with id: " + id);
            UsersResource.MessagesResource.GetRequest r = service.Users.Messages.Get("me", id);
            Message msg = r.Execute();
            SimpleMessage simple = new SimpleMessage();
            foreach (var headers in msg.Payload.Headers)
            {
                if (headers.Name.Equals("Subject")) simple.Subject = headers.Value;
                if (headers.Name.Equals("From")) simple.From = headers.Value;
            }
            if (msg.Payload.Body != null && msg.Payload.Body.Data != null)
            {
                String strData = msg.Payload.Body.Data.Replace('_', '/').Replace('-', '+');
                byte[] data = Convert.FromBase64String(strData);
                string decodedString = Encoding.UTF8.GetString(data);
                simple.Body = decodedString;
            }
            else
            {
                foreach (var part in msg.Payload.Parts)
                {
                    if (part.Body.Data != null && part.MimeType.Equals("text/plain"))
                    {
                        String strData = part.Body.Data.Replace('_', '/').Replace('-', '+');
                        byte[] data = Convert.FromBase64String(strData);
                        string decodedString = Encoding.UTF8.GetString(data);
                        simple.Body = decodedString;
                    }
                }
            }
            simple.Message = msg;
            return simple;
        }

        /// <summary>
        /// Sends a message given the to, subject, and body
        /// </summary>
        public static Message SendMessage(String to, String subject, String body)
        {
            Message msg = new Message();
            String date = DateTime.Now.ToString();
            String from = service.Users.GetProfile("me").UserId;
            String email = "Date: " + date + "\n" +
                "From: " + from + "\n" +
                "To: " + to + "\n" +
                "Subject: " + subject + "\n\n" +
                body;
            email = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
            email = email.Replace('/', '_').Replace('+', '-');
            msg.Raw = email;
            return SendMessage(msg);
        }

        /// <summary>
        /// Sends a Message object
        /// </summary>
        private static Message SendMessage(Message email)
        {
            try
            {
                return service.Users.Messages.Send(email, "me").Execute();
            }
            catch (Exception e)
            {
                log.Error("An error occurred: " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// Marks a message as unread by it's id
        /// </summary>
        public static Message MarkMessageRead(String id)
        {
            ModifyMessageRequest mod = new ModifyMessageRequest();
            mod.RemoveLabelIds = new string[] { "UNREAD" };
            try
            {
                return service.Users.Messages.Modify(mod, "me", id).Execute();
            }
            catch (Exception e)
            {
                log.Error("An error occurred: " + e.Message);
            }

            return null;
        }
    }
}
