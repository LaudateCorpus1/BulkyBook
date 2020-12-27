using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public readonly IConfiguration _configuration;
        public readonly EmailOptions _options;
        public EmailSender(IConfiguration configuration, IOptions<EmailOptions> options)
        {
            _configuration = configuration;
            _options = options.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await Send("admin@bulky.com", email, subject, htmlMessage);
        }

        public async Task Send(string from, string to, string subject, string html)
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = html };

            // send email
            var smtp = new SmtpClient();
            await smtp.ConnectAsync(_configuration["Authentication:Email:host"], Convert.ToInt32(_configuration["Authentication:Email:port"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_configuration["Authentication:Email:auth:user"], _configuration["Authentication:Email:auth:pass"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
