namespace Core.Domain.ExternalServicesContracts;

public interface INotificationService
{
    Task<(bool isValid, string message)> SendAsync(string toWhom, string subject, string body, bool isBodyHTML);
}