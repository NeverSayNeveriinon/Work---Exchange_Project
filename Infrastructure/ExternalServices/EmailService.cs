using System.Net;
using System.Net.Mail;
using Core.Domain.ExternalServicesContracts;
using FluentResults;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.ExternalServices;

// TODO: Options Pattern 
public class EmailService : INotificationService
{
    private readonly IConfiguration _config;
    
    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    
    public async Task<Result> SendAsync(string toEmail, string subject, string body, bool isBodyHTML)
    {
        var MailServer = _config["EmailSettings:MailServer"];
        var Port = _config["EmailSettings:MailPort"];
        
        var FromEmail = _config["EmailSettings:FromEmail"];
        var Password = _config["EmailSettings:Password"];

        if ( new List<string?>(){MailServer,Port,FromEmail,Password}.Exists(string.IsNullOrEmpty) )
            throw new InvalidOperationException("Check EmailSettings(MailServer,MailPort,FromEmail,Password) In Your Configuration");
        
        var PortNumber = int.Parse(Port);
        var client = new SmtpClient(MailServer, PortNumber)
        {
            Credentials = new NetworkCredential(FromEmail, Password),
            EnableSsl = true,
        };
        
        MailMessage mailMessage = new MailMessage(FromEmail!, toEmail, subject, body) { IsBodyHtml = isBodyHTML };
        
        try { await client.SendMailAsync(mailMessage); }
        catch (Exception e) { return Result.Fail("There is Something Wrong in Sending The Confirmation Email to You, Please Try Again"); }
        
        return Result.Ok().WithSuccess("The Confirmation Link Has Successfully Sent\n!!Please Check Your Email!!");
    }
}