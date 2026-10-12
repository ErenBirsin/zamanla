using FluentValidation;
using GenericRepository;
using Zamanla.Application.Services;
using Zamanla.Domain.Shared;
using Zamanla.Domain.Tenants;
using Zamanla.Domain.Tenants.ValueObjects;
using Zamanla.Domain.Users;
using Zamanla.Domain.Users.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace Zamanla.Application.Auth;

public sealed record RegisterBusinessCommand(
    string BusinessName,
    string Slug,
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Result<AuthTokens>>;

public sealed class RegisterBusinessCommandValidator : AbstractValidator<RegisterBusinessCommand>
{
    public RegisterBusinessCommandValidator()
    {
        RuleFor(p => p.BusinessName)
            .NotEmpty().WithMessage("Geçerli bir işletme adı girin")
            .MaximumLength(100).WithMessage("İşletme adı en fazla 100 karakter olabilir");
        RuleFor(p => p.Slug)
            .NotEmpty().WithMessage("Geçerli bir kısa link girin")
            .MaximumLength(100).WithMessage("Kısa link en fazla 100 karakter olabilir")
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Kısa link yalnızca küçük harf, rakam ve tire içerebilir");
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("Geçerli bir ad girin")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir");
        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Geçerli bir soyad girin")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir");
        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Geçerli bir mail adresi girin")
            .EmailAddress().WithMessage("Geçerli bir mail adresi girin")
            .MaximumLength(254).WithMessage("Mail adresi en fazla 254 karakter olabilir");
        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("Geçerli bir şifre girin")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalı");
    }
}

internal sealed class RegisterBusinessCommandHandler(
    ITenantRepository tenantRepository,
    IAppUserRepository appUserRepository,
    IUnitOfWork unitOfWork,
    IJwtProvider jwtProvider) : IRequestHandler<RegisterBusinessCommand, Result<AuthTokens>>
{
    public async Task<Result<AuthTokens>> Handle(RegisterBusinessCommand request, CancellationToken cancellationToken)
    {
        Slug slug = new(request.Slug);
        if (await tenantRepository.SlugExistsAsync(slug, cancellationToken))
        {
            return Result<AuthTokens>.Failure(409, "Bu kısa link daha önce alınmış");
        }

        Email email = new(request.Email);
        if (await appUserRepository.GetByEmailIgnoringTenantAsync(email, cancellationToken) is not null)
        {
            return Result<AuthTokens>.Failure(409, "Bu mail adresi daha önce kullanılmış");
        }

        Tenant tenant = new(new Name(request.BusinessName), slug, new TimeZoneId("Europe/Istanbul"));
        AppUser owner = new(
            tenant.Id,
            new FirstName(request.FirstName),
            new LastName(request.LastName),
            email,
            new Password(request.Password),
            UserRole.Owner);

        tenantRepository.Add(tenant);
        appUserRepository.Add(owner);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await jwtProvider.CreateTokensAsync(owner, cancellationToken);
    }
}
