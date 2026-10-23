using Zamanla.Domain.Users;

namespace Zamanla.Application.Services;

public interface IPasswordResetMailService
{
    Task SendAsync(AppUser user, string code, CancellationToken cancellationToken = default);
}
