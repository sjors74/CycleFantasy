using CycleManager.Services;
using CycleManager.Services.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CycleManager.Tests.Unit.Services
{
    public class EmailSenderTests
    {
        private readonly EmailSender _service;

        public EmailSenderTests()
        {
            var settings = Options.Create(new SmtpSettings
            {
                From = "test@example.com",
                Host = "localhost",
                Port = 1,
                Username = "test",
                Password = "test"
            });

            _service = new EmailSender(settings);
        }

        [Fact]
        public async Task SendEmailAsync_WithEmptyHtmlMessage_ThrowsWhenSmtpConnectionFails()
        {
            // Act
            var act = async () =>
                await _service.SendEmailAsync(
                    "recipient@example.com",
                    "Test",
                    "");

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }
    }
}