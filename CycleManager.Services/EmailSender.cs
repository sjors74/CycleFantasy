using CycleManager.Services.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.RegularExpressions;

namespace CycleManager.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;
        
        public EmailSender(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var plainTextMessage = ConvertHtmlToPlainText(htmlMessage);
            await SendEmailAsync(email, subject, htmlMessage, plainTextMessage);
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage, string plainTextMessage)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_smtpSettings.From));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder 
            { 
                HtmlBody = htmlMessage,
                TextBody = plainTextMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij verzenden e-mail: {ex.Message}");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        private string ConvertHtmlToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Vervang <br>, <p>, enz. met linebreaks
            string text = Regex.Replace(html, @"<\s*(br|p)\s*/?>", "\n", RegexOptions.IgnoreCase);

            // Verwijder overige HTML-tags
            text = Regex.Replace(text, "<.*?>", string.Empty);

            // Decodeer HTML-entiteiten
            text = System.Net.WebUtility.HtmlDecode(text);

            return text.Trim();
        }
    }
}
