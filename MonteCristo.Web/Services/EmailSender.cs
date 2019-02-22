using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MonteCristo.Web.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;
        public EmailSender(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            MailMessage msg = new MailMessage();
            try
            {
                string from = configuration.GetSection("AppSettings:mailSender_from").Value;
                string host = configuration.GetSection("AppSettings:mailSender_host").Value;
                string fromName = configuration.GetSection("AppSettings:mailSender_fromName").Value;
                string userName = configuration.GetSection("AppSettings:mailSender_userName").Value;
                string password = configuration.GetSection("AppSettings:mailSender_password").Value;
                int port = int.Parse(configuration.GetSection("AppSettings:mailSender_port").Value);
                bool enableSsl = bool.Parse(configuration.GetSection("AppSettings:mailSender_enableSsl").Value);

                msg.From = new MailAddress(from, fromName);
                msg.To.Add(email);
                msg.Subject = subject;
                msg.Body = message;
                msg.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient(host, port))
                {
                    smtp.EnableSsl = enableSsl;
                    smtp.Credentials = new System.Net.NetworkCredential(userName, password);
                    await smtp.SendMailAsync(msg);

                    // _logger.LogInformation($"SendEmailAsync {email} successfully");
                }
            }
            catch (Exception)
            {
                // _logger.LogError("Error SendEmailAsync: " + ex);
            }
            finally
            {
                if (msg != null)
                {
                    msg.Dispose();
                }
            }
        }
    }
}
