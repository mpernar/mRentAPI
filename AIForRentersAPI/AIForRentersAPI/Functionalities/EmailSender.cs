using AIForRentersAPI.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIForRentersAPI.Functionalities
{
    public static class EmailSender
    {
        /// <summary>
        /// Method receives Request object and calls a method for generating email and sends an email to the client.
        /// </summary>
        /// <param name="request"></param>
        public static void SendEmail(Request request)
        {
            SmtpClient smtpClient = new SmtpClient();

            Uri uri = new Uri("smtps://smtp.gmail.com:465");

            //connecting to server
            smtpClient.Connect(uri);

            //authenticating user
            smtpClient.Authenticate(Sender.Email, Sender.Password);

            //method for generating email
            MimeMessage generatedEmail = GenerateEmail(request);

            //sending email
            smtpClient.Send(generatedEmail);

            smtpClient.Disconnect(true);
        }

        /// <summary>
        /// Method receives Request object and generates email for the client.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>MimeMessage object</returns>
        private static MimeMessage GenerateEmail(Request request)
        {
            //collecting data
            string senderAddress = Sender.Email;
            string clientAddress = request.Client.Email;
            string clientNameAndSurname = request.Client.Name + " " + request.Client.Surname;

            string messageSubject = request.ResponseSubject;
            string messageBody = request.ResponseBody;

            //creating new MimeMessage object
            MimeMessage message = new MimeMessage();

            //assigning data to the MimeMessage object
            message.From.Add(new MailboxAddress("AIForRenters", senderAddress));
            message.To.Add(new MailboxAddress(clientNameAndSurname, clientAddress));
            message.Subject = messageSubject;

            //creating new BodyBuilder object and assigning data to it
            BodyBuilder body = new BodyBuilder();
            body.TextBody = messageBody;

            message.Body = body.ToMessageBody();

            return message;
        }

        /// <summary>
        /// Method receives strings subject, body and address and calls a method for generating email and sends an email to the client.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="address"></param>
        public static void SendEmail(string subject, string body, string address)
        {
            SmtpClient smtpClient = new SmtpClient();

            Uri uri = new Uri("smtps://smtp.gmail.com:465");

            //connecting to server
            smtpClient.Connect(uri);

            //authenticating user
            smtpClient.Authenticate(Sender.Email, Sender.Password);

            //method for generating email
            MimeMessage generatedEmail = GenerateEmail(subject, body, address);

            //sending email
            smtpClient.Send(generatedEmail);

            smtpClient.Disconnect(true);
        }

        /// <summary>
        /// Method receives strings subject, body and address and generates email for the client.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="emailBody"></param>
        /// <param name="address"></param>
        /// <returns>MimeMessage object</returns>
        private static MimeMessage GenerateEmail(string subject, string emailBody, string address)
        {
            //collecting data
            string senderAddress = Sender.Email;
            string clientAddress = address;

            string messageSubject = subject;
            string messageBody = emailBody;

            //creating new MimeMessage object
            MimeMessage message = new MimeMessage();

            //assigning data to the MimeMessage object
            message.From.Add(new MailboxAddress("AIForRenters", senderAddress));
            message.To.Add(new MailboxAddress("Client", clientAddress));
            message.Subject = messageSubject;

            //creating new BodyBuilder object and assigning data to it
            BodyBuilder body = new BodyBuilder();
            body.TextBody = messageBody;

            message.Body = body.ToMessageBody();

            return message;
        }
    }
}
