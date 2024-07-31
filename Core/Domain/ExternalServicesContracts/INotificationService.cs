using FluentResults;

namespace Core.Domain.ExternalServicesContracts;

public interface INotificationService
{
    Task<Result> SendAsync(string toWhom, string subject, string body, bool isBodyHTML);
}