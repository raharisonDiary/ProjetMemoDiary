using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SocialGasy.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string targetEmail, string subject, string body)
        {
            try
            {
                var myEmail = "anaranao@gmail.com";
                var myPassword = "kaody_manokana_avy_amin_google"; 

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(myEmail, myPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(myEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(targetEmail);

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"[EMAIL SUCCESS] Nalefa tany amin'ny {targetEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] Tsy lasa ny mailaka: {ex.Message}");
            }
        }
    }
}