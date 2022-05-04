using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions emailOptions;

        // thats the way to get data from appsettings e.g emailsender
        public EmailSender(IOptions<EmailOptions> options)
        {
            emailOptions = options.Value;
        }

        // For user register
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await Execute(emailOptions.SendGridKey, subject, htmlMessage, email);

        }

        

        private async Task Execute(string sendGridKey, string subject, string message, string email)
        {
            //string name = email.Substring(0, email.IndexOf('.') + 1);
            //if (name.Contains("@"))
            //{
            //    name = email.Substring(0, email.IndexOf('@') + 1);
            //}

            if (subject != "Confirm your email")
            {
                var client = new SendGridClient(sendGridKey);
                var from = new EmailAddress("albionademi5@gmail.com", "From " + email + " to ");
                var to = new EmailAddress("albionademi5@gmail.com", "End User");
                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
                var response = await client.SendEmailAsync(msg);
            }
            else
            {
                var client = new SendGridClient(sendGridKey);
                var from = new EmailAddress("albionademi5@gmail.com", "My Book");
                var to = new EmailAddress(email, "End User");
                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
                var response = await client.SendEmailAsync(msg);
            }
            
        }

    }
}
