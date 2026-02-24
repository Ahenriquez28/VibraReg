using Microsoft.EntityFrameworkCore;
using RegistrationService.Data;

namespace RegistrationService.Services
{
    public class ScheduledJobs
    {
        //I didnt even fr make this file here idk whats up
        public static async Task SendConfirmationEmailsJob(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            
            var students = await context.RegisteredUsers
                .Where(s => s.Status == "registered" 
                         && s.ConfirmationSentAt == null)
                .ToListAsync();
            
            foreach (var student in students)
            {
                try
                {
                    await emailService.SendConfirmationEmailAsync(
                        student.Email,
                        student.FullName,
                        student.ConfirmationToken ?? ""
                    );
                    
                    student.ConfirmationSentAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send to {student.Email}: {ex.Message}");
                }
            }
            
            await context.SaveChangesAsync();
        }

        public static async Task SendDeadlineRemindersJob(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            
            var students = await context.RegisteredUsers
                .Where(s => s.Status == "registered")
                .ToListAsync();
            
            foreach (var student in students)
            {
                try
                {
                    await emailService.SendDeadlineReminderAsync(
                        student.Email,
                        student.FullName,
                        student.ConfirmationToken ?? ""
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send to {student.Email}: {ex.Message}");
                }
            }
        }

        public static async Task CleanupUnconfirmedJob(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            
            var now = DateTime.UtcNow;
            var unconfirmed = await context.RegisteredUsers
                .Where(s => s.Status == "registered" 
                         && s.ConfirmationDeadline < now)
                .ToListAsync();
            
            foreach (var student in unconfirmed)
            {
                student.Status = "removed";
            }
            
            await context.SaveChangesAsync();
            
            Console.WriteLine($"Removed {unconfirmed.Count} unconfirmed students");
        }
    }
}