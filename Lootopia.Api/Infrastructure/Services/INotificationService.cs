namespace Lootopia.Api.Infrastructure.Services;

public interface INotificationService
{
    Task SendAsync(Guid userId, string type, string title, string body, CancellationToken cancellationToken = default);
}
