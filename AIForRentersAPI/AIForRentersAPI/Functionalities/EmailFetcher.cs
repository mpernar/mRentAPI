using HtmlAgilityPack;
using OpenPop.Mime;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIForRentersAPI.Functionalities
{
    public static class EmailFetcher
    {
        /// <summary>
        /// Method fetches all new incoming emails from user's mailbox and shapes them into
        /// ReceivedData objects.
        /// </summary>
        /// <returns>List of ReceivedData objects</returns>
        public static List<ReceivedData> ShapeReceivedData()
        {
            List<ReceivedData> listOfReceivedData = new List<ReceivedData>();

            using (Pop3Client client = new Pop3Client())
            {
                string username = Sender.Email.Remove(Sender.Email.IndexOf("@"), 10);

                // Connect to the server
                client.Connect("pop.gmail.com", 995, true);

                // Authenticate ourselves towards the server
                client.Authenticate(username, Sender.Password);

                // Get the number of messages in the inbox
                int messageCount = client.GetMessageCount();

                // Messages are numbered in the interval: [1, messageCount]
                // Ergo: message numbers are 1-based.
                // Most servers give the latest message the highest number
                for (int i = messageCount; i > 0; i--)
                {
                    string clientNameSurname = ChangeEncoding(client.GetMessage(i).Headers.From.DisplayName);
                    string emailSubject = ChangeEncoding(client.GetMessage(i).Headers.Subject);
                    string clientAddress = ChangeEncoding(client.GetMessage(i).Headers.From.Address);

                    string emailBody = ExtractMessageBody(client.GetMessage(i));

                    ReceivedData newReceivedData = new ReceivedData
                    {
                        ClientNameSurname = clientNameSurname,
                        EmailAddress = clientAddress,
                        EmailSubject = emailSubject,
                        EmailBody = emailBody
                    };

                    listOfReceivedData.Add(newReceivedData);
                }
            }
            return listOfReceivedData;
        }

        private static string ChangeEncoding(string displayName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Encoding wind1252 = Encoding.GetEncoding(1252);
            Encoding utf8 = Encoding.UTF8;
            byte[] wind1252Bytes = wind1252.GetBytes(displayName);
            byte[] utf8Bytes = Encoding.Convert(wind1252, utf8, wind1252Bytes);
            string utf8String = Encoding.UTF8.GetString(utf8Bytes);

            return utf8String;
        }

        /// <summary>
        /// This method receives Message object and extract message body from it.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>String that represents message body</returns>
        private static string ExtractMessageBody(Message message)
        {
            StringBuilder builder = new StringBuilder();
            OpenPop.Mime.MessagePart plainText = message.FindFirstPlainTextVersion();
            if (plainText != null)
            {
                // We found some plaintext!
                builder.Append(plainText.GetBodyAsText());
            }
            else
            {
                // Might include a part holding html instead
                OpenPop.Mime.MessagePart html = message.FindFirstHtmlVersion();

                if (html != null)
                {
                    // We found some html!
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html.GetBodyAsText());
                    //this xpath selects all p tags having its class as MsoNormal
                    var itemList = doc.DocumentNode.SelectNodes("//div[@class='WordSection1']//p[@class='MsoNormal']").Select(p => p.InnerText).ToList();
                    foreach (var item in itemList)
                    {
                        builder.Append(item);
                    }
                }
            }
            return builder.ToString();
        }
    }
}
