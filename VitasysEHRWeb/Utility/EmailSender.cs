using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace VitasysEHR.Utility
{
    public class EmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("vitasysehrapp@gmail.com"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            //send Email
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 465, true);
                emailClient.Authenticate("vitasysehrapp@gmail.com", "xpomenortvkoakoe");
                emailClient.Send(emailToSend);
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
        }

        public Task EmailAttachmentAsync(string? email, string subject, string htmlMessage, byte[] file)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse("vitasysehrapp@gmail.com"));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var builder = new BodyBuilder();
            builder.TextBody = htmlMessage;
            builder.HtmlBody = string.Format($"<p>{htmlMessage}</p>");

            MimeKit.ContentType type = new MimeKit.ContentType("application", "pdf");
            builder.Attachments.Add("Invoice.pdf", file, type);

            message.Body = builder.ToMessageBody();

            //send Email
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 465, true);
                emailClient.Authenticate("vitasysehrapp@gmail.com", "xpomenortvkoakoe");
                emailClient.Send(message);
                emailClient.Disconnect(true);
            }

            return Task.CompletedTask;
        }
    }
}
