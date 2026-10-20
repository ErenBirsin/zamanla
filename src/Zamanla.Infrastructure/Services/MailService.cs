using FluentEmail.Core;
using Zamanla.Application.Services;

namespace Zamanla.Infrastructure.Services;

internal sealed class MailService(IFluentEmail fluentEmail) : IMailService
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var sendResponse = await fluentEmail
            .To(to)
            .Subject(subject)
            .Body(body)
            .SendAsync(cancellationToken);

        if (!sendResponse.Successful)
        {
            throw new ArgumentException(string.Join(", ", sendResponse.ErrorMessages));
        }
    }
}
