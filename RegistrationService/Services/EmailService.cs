using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RegistrationService.DTOs;


namespace RegistrationService.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string fullName, string confirmationToken);
        Task SendDeadlineReminderAsync(string toEmail, string fullName, string confirmationToken);
        Task SendRegistrationEmailWithTeam(string toEmail, string fullName, string teamName);
        Task SendRegistrationEmailNoTeam(string toEmail, string fullName);
        Task SendTeamNews(string toEmail, string fullName, string teamName, List<AdminStudentDto> teammates);

    }

    public class EmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _frontendUrl;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["Email:ResendApiKey"] ?? "";
            _fromEmail = configuration["Email:FromEmail"] ?? "onboarding@resend.dev";
            _fromName = configuration["Email:FromName"] ?? "Vibra Hackathon";
            _frontendUrl = configuration["Email:FrontendUrl"] ?? "http://165.245.130.1";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var payload = new
            {
                from = $"{_fromName} <{_fromEmail}>",
                to = new[] { toEmail },
                subject = subject,
                html = htmlBody
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Resend API error: {responseBody}");
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string fullName, string confirmationToken)
        {
            var confirmationUrl = $"{_frontendUrl}/#/confirm/{confirmationToken}";
            var subject = "SHPE Hackathon: Confirm your registration!";
            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h1 style='color: #667eea;'>You're Registered! 🎉</h1>
                    <p>Hi <strong>{fullName}</strong>,</p>
                    <p>Thank you for registering for <strong>Vibra Hackathon</strong> on <strong>April 4th, 2026</strong>!</p>
                    <p>To <strong>confirm your attendance</strong>, please click the button below:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationUrl}' style='display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            CONFIRM ATTENDANCE
                        </a>
                    </div>
                    <p><strong>⏰ Deadline:</strong> April 1st, 2026 at 5:00 PM</p>
                    <p style='color: #666; font-size: 14px;'>Can't make it? Just ignore this email.</p>
                    <p style='font-size: 12px; color: #999;'>Or copy this link: <a href='{confirmationUrl}'>{confirmationUrl}</a></p>
                    <p style='font-size: 12px; color: #999;'>Questions? Email us at support@vibraatl.com</p>
                </div>";

            await SendEmailAsync(toEmail, fullName, subject, html);
        }

        public async Task SendDeadlineReminderAsync(string toEmail, string fullName, string confirmationToken)
        {
            var confirmationUrl = $"{_frontendUrl}/#/confirm/{confirmationToken}";
            var subject = "⏰ LAST CHANCE: Confirm Your Vibra Registration Today!";
            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h1 style='color: #e74c3c;'>⚠️ Final Reminder!</h1>
                    <p>Hi <strong>{fullName}</strong>,</p>
                    <p>Sorry for the last confirmation email, it was the wrong email send!</p>
                    <p>This is the <strong>CORRECT Link</strong> to confirm your attendance for Vibra Hackathon!</p>
                    <p style='background: #fff3cd; padding: 15px; border-left: 4px solid #ffc107;'><strong>⏰ Deadline: TODAY at 5:00 PM</strong></p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationUrl}' style='display: inline-block; padding: 15px 30px; background: #e74c3c; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            CONFIRM NOW
                        </a>
                    </div>
                    <p><strong>Event Details:</strong></p>
                    <ul>
                        <li>📅 Date: April 4th</li>
                        <li>📍 Location: 25 Park Pl NE, Atlanta, GA 30303</li>
                        <li>⏰ Time: 12:00PM</li>
                    </ul>
                    <p style='font-size: 12px; color: #999;'>Or copy this link: <a href='{confirmationUrl}'>{confirmationUrl}</a></p>
                </div>";

            await SendEmailAsync(toEmail, fullName, subject, html);
        }

        public async Task SendRegistrationEmailWithTeam(string toEmail, string fullName, string teamName)
        {
            var subject = "You're Registered for Vibra ATL 2026! 🎉";
            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h1 style='color: #72aabf;'>Thank You for Registering!</h1>
                    <p>Hi <strong>{fullName}</strong>,</p>
                    <p>Thank you for registering for <strong>Vibra ATL 2026</strong>! We're excited to have you join us.</p>
                    
                    <div style='background: #f0f8ff; padding: 20px; border-left: 4px solid #72aabf; margin: 20px 0;'>
                        <p><strong>📅 Event Details:</strong></p>
                        <ul style='list-style: none; padding: 0;'>
                            <li>📅 <strong>Date:</strong> April 4-5, 2026</li>
                            <li>📍 <strong>Location:</strong> Georgia State University - Creative Media Industries Institute</li>
                            <li>🏢 <strong>Address:</strong> 25 Park Pl NE, Atlanta, GA 30303</li>
                            <li>⏰ <strong>Start Time:</strong> April 4th at 12:00 PM</li>
                        </ul>
                    </div>

                    <div style='background: #fff3cd; padding: 20px; border-left: 4px solid #d43f27; margin: 20px 0;'>
                        <p><strong>👥 Your Team:</strong> <span style='color: #d43f27; font-size: 1.2em;'>{teamName}</span></p>
                        <p>For other teammates joining your team, please share the team name <strong>&quot;{teamName}&quot;</strong> with them so they can sign up and join!</p>
                        <p style='font-size: 0.9em; color: #666;'><em>Remember: Teams can have up to 4 members.</em></p>
                    </div>

                    <p style='color: #666; font-size: 14px;'>
                        We look forward to seeing you on April 4th!
                    </p>

                    <p style='font-size: 12px; color: #999; margin-top: 40px; border-top: 1px solid #eee; padding-top: 20px;'>
                        This email was sent to {toEmail} because you registered for Vibra ATL Hackathon.<br>
                        If you did not register, please ignore this email or <a href='mailto:shpe.gastate@gmail.com'>contact us</a>.<br>
                        <br>
                        <strong>SHPE Georgia State University</strong><br>
                        25 Park Pl NE, Atlanta, GA 30303
                    </p>
                </div>";

            await SendEmailAsync(toEmail, fullName, subject, html);
        }

        public async Task SendRegistrationEmailNoTeam(string toEmail, string fullName)
        {
            var subject = "You're Registered for Vibra ATL 2026! 🎉";
            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h1 style='color: #72aabf;'>Thank You for Registering!</h1>
                    <p>Hi <strong>{fullName}</strong>,</p>
                    <p>Thank you for registering for <strong>Vibra ATL 2026</strong>! We're excited to have you join us.</p>
                    
                    <div style='background: #f0f8ff; padding: 20px; border-left: 4px solid #72aabf; margin: 20px 0;'>
                        <p><strong>📅 Event Details:</strong></p>
                        <ul style='list-style: none; padding: 0;'>
                            <li>📅 <strong>Date:</strong> April 4-5, 2026</li>
                            <li>📍 <strong>Location:</strong> Georgia State University - Creative Media Industries Institute</li>
                            <li>🏢 <strong>Address:</strong> 25 Park Pl NE, Atlanta, GA 30303</li>
                            <li>⏰ <strong>Start Time:</strong> April 4th at 12:00 PM</li>
                        </ul>
                    </div>

                    <div style='background: #e8f5e9; padding: 20px; border-left: 4px solid #72aabf; margin: 20px 0;'>
                        <p><strong>👥 Team Assignment:</strong></p>
                        <p>Don't worry! Our SHPE team will assign you to a team and notify you by <strong>April 2nd, 2026</strong> with your team details.</p>
                        <p style='font-size: 0.9em; color: #666;'><em>We'll make sure you're paired with great teammates!</em></p>
                    </div>

                    <p style='color: #666; font-size: 14px;'>
                        We look forward to seeing you on April 4th!
                    </p>

                    <p style='font-size: 12px; color: #999; margin-top: 40px; border-top: 1px solid #eee; padding-top: 20px;'>
                        This email was sent to {toEmail} because you registered for Vibra ATL Hackathon.<br>
                        If you did not register, please ignore this email or <a href='mailto:shpe.gastate@gmail.com'>contact us</a>.<br>
                        <br>
                        <strong>SHPE Georgia State University</strong><br>
                        25 Park Pl NE, Atlanta, GA 30303
                    </p>
                </div>";

            await SendEmailAsync(toEmail, fullName, subject, html);
        }

        public async Task SendTeamNews(string toEmail, string fullName, string teamName, List<AdminStudentDto> teammates)
        {
            var subject = "Your teammates for Vibra ATL 2026! 🎉";
            
            // Dynamically build teammates HTML
            var teammatesHtml = string.Join("", teammates
                .Where(t => t.Email != toEmail) // exclude the current student
                .Select(t => $"<li>👤 <strong>{t.FullName}</strong> — {t.Email}</li>"));

            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h1 style='color: #72aabf;'>Thank You for Registering!</h1>
                    <p>Hi <strong>{fullName}</strong>,</p>
                    <p>Thank you for registering for <strong>Vibra ATL 2026</strong>! Here are your teammates:</p>
                    
                    <ul style='list-style: none; padding: 0;'>
                        {teammatesHtml}
                    </ul>

                    <div style='background: #f0f8ff; padding: 20px; border-left: 4px solid #72aabf; margin: 20px 0;'>
                        <p><strong>📅 Event Details:</strong></p>
                        <ul style='list-style: none; padding: 0;'>
                            <li>📅 <strong>Date:</strong> April 4-5, 2026</li>
                            <li>📍 <strong>Location:</strong> Georgia State University - Creative Media Industries Institute</li>
                            <li>🏢 <strong>Address:</strong> 25 Park Pl NE, Atlanta, GA 30303</li>
                            <li>⏰ <strong>Start Time:</strong> April 4th at 12:00 PM</li>
                        </ul>
                    </div>

                    <p style='color: #666; font-size: 14px;'>
                        We look forward to seeing you on April 4th!
                    </p>

                    <p style='font-size: 12px; color: #999; margin-top: 40px; border-top: 1px solid #eee; padding-top: 20px;'>
                        This email was sent to {toEmail} because you registered for Vibra ATL Hackathon.<br>
                        If you did not register, please ignore this email or <a href='mailto:shpe.gastate@gmail.com'>contact us</a>.<br>
                        <br>
                        <strong>SHPE Georgia State University</strong><br>
                        25 Park Pl NE, Atlanta, GA 30303
                    </p>
                </div>";

            await SendEmailAsync(toEmail, fullName, subject, html);
        }
    }
}
