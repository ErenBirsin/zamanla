namespace Zamanla.Domain.Shared;

public sealed record Email
{
    public Email(string value)
    {
        Value = value.Trim().ToLowerInvariant();
    }

    public string Value { get; private set; }
}
