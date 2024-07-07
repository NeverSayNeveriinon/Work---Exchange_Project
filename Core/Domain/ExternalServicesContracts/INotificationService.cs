namespace Core.Domain.ExternalServicesContracts;

public interface INotificationService
{ 
    Task SendAsync(string toWhom, string subject, string body, bool isBodyHTML);
}