using System.Net;
using System.Net.Mail;
using Core.Domain.ExternalServicesContracts;
using Microsoft.Extensions.Configuration;

namespace Core.Services;

// TODO: Options Pattern 
public class EmailService : INotificationService
{
    private readonly IConfiguration _config;
    
    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    
    public Task SendAsync(string toEmail, string subject, string body, bool isBodyHTML)
    {
        string MailServer = _config["EmailSettings:MailServer"];
        int Port = int.Parse(_config["EmailSettings:MailPort"]);
        
        string FromEmail = _config["EmailSettings:FromEmail"];
        string Password = _config["EmailSettings:Password"];
        
        var client = new SmtpClient(MailServer, Port)
        {
            Credentials = new NetworkCredential(FromEmail, Password),
            EnableSsl = true,
        };
        MailMessage mailMessage = new MailMessage(FromEmail, toEmail, subject, body)
        {
            IsBodyHtml = isBodyHTML
        };
        return client.SendMailAsync(mailMessage);
    }
}