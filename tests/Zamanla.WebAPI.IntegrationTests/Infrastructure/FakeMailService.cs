using System.Collections.Concurrent;
using Zamanla.Application.Services;

namespace Zamanla.WebAPI.IntegrationTests.Infrastructure;

public sealed record SentMail(string To, string Subject, string Body, string Html);

public sealed class FakeMailService : IMailService
{
    private readonly ConcurrentQueue<SentMail> _mails = new();

    public IReadOnlyList<SentMail> Mails => _mails.ToList();

    public bool Fail { get; set; }

    public Task SendAsync(string to, string subject, string htmlBody, string textBody, CancellationToken cancellationToken = default)
    {
        if (Fail)
        {
            throw new InvalidOperationException("SMTP sunucusuna ulaşılamadı");
        }

        _mails.Enqueue(new SentMail(to, subject, textBody, htmlBody));
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _mails.Clear();
        Fail = false;
    }
}
