namespace Zamanla.Infrastructure.Options;

// e-postadaki linkler client'ın adresiyle oluşturulur
public sealed class ClientOptions
{
    public string BaseUrl { get; set; } = default!;
}
