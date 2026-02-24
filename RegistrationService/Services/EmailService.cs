using MailKit.Net.Smtp;
using MimeKit;

namespace RegistrationService.Services
{
    //We created the IEmailService in here and not in its own file like
    //"Iregistration" file bc lowkey im short on time and need to be fast my bad future brochacho reading this 
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string fullName, string confirmationToken);
        Task SendDeadlineReminderAsync( string toEmail, string fullName, string confirmationToken);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _frontendUrl;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _fromEmail = configuration["Email:FromEmail"] ?? "noreply@vibraatl.com";
            _fromName = configuration["Email:FromName"] ?? "Vibra Hackathon";
            _smtpServer = configuration["Email:SmtpServer"] ?? "smtp.gmail.com";  // ← Fixed typo
            _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = configuration["Email:SmtpUsername"] ?? "";
            _smtpPassword = configuration["Email:SmtpPassword"] ?? "";
            _frontendUrl = configuration["Email:FrontendUrl"] ?? "http://localhost:5173";
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string fullName, string confirmationToken)
        {
            var confirmationUrl = $"{_frontendUrl}/#/confirm/{confirmationToken}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "SHPE Hackathon: Confirm your registration!";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h1 style='color: #667eea;'>You're Registered! 🎉</h1>
                        <p>Hi <strong>{fullName}</strong>,</p>
                        <p>Thank you for registering for <strong>Vibra Hackathon</strong> on <strong>April 4th, 2026</strong>!</p>
                        
                        <p>To <strong>confirm your attendance</strong>, please click the button below:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationUrl}' 
                               style='display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                      color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                CONFIRM ATTENDANCE
                            </a>
                        </div>
                        
                        <p><strong>⏰ Deadline:</strong> April 1st, 2026 at 5:00 PM</p>
                        
                        <p style='color: #666; font-size: 14px;'>
                            Can't make it? No problem - just ignore this email and your spot will go to someone on the waitlist.
                        </p>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        
                        <p style='font-size: 12px; color: #999;'>
                            Or copy this link: <a href='{confirmationUrl}'>{confirmationUrl}</a>
                        </p>
                        
                        <p style='font-size: 12px; color: #999;'>
                            Questions? Email us at support@vibraatl.com
                        </p>
                    </div>
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        public async Task SendDeadlineReminderAsync(string toEmail, string fullName, string confirmationToken)
        {
            var confirmationUrl = $"{_frontendUrl}/confirm/{confirmationToken}";
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "⏰ LAST CHANCE: Confirm Your Vibra Registration Today!";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h1 style='color: #e74c3c;'>⚠️ Final Reminder!</h1>
                        <p>Hi <strong>{fullName}</strong>,</p>
                        <p>This is your <strong>LAST CHANCE</strong> to confirm your attendance for Vibra Hackathon!</p>
                        
                        <p style='background: #fff3cd; padding: 15px; border-left: 4px solid #ffc107;'>
                            <strong>⏰ Deadline: TODAY at 5:00 PM</strong>
                        </p>
                        
                        <p>If you don't confirm by today, your spot will be given to someone on the waitlist.</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationUrl}' 
                               style='display: inline-block; padding: 15px 30px; background: #e74c3c; 
                                      color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                CONFIRM NOW
                            </a>
                        </div>
                        
                        <p><strong>Event Details:</strong></p>
                        <ul>
                            <li>📅 Date: April 4th </li>
                            <li>📍 Location: 25 Park Pl NE, Atlanta, GA 30303</li>
                            <li>⏰ Time: 12:00PM </li>
                        </ul>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        
                        <p style='font-size: 12px; color: #999;'>
                            Or copy this link: <a href='{confirmationUrl}'>{confirmationUrl}</a>
                        </p>
                    </div>
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }


    }
}