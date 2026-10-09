using FluentValidation;
using Shouldly;
using TS.MediatR;
using Zamanla.Application.Behaviors;

namespace Zamanla.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed record TestCommand(string Name) : IRequest<string>;

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş olamaz");
        }
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_CallsNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestCommand, string>([new TestCommandValidator()]);
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(
            new TestCommand("Saç kesimi"),
            () =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            },
            TestContext.Current.CancellationToken);

        // Assert
        nextCalled.ShouldBeTrue();
        result.ShouldBe("ok");
    }

    [Fact]
    public async Task Handle_WhenRequestIsInvalid_ThrowsAndDoesNotCallNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestCommand, string>([new TestCommandValidator()]);
        var nextCalled = false;

        // Act
        var exception = await Should.ThrowAsync<ValidationException>(() =>
            behavior.Handle(
                new TestCommand(""),
                () =>
                {
                    nextCalled = true;
                    return Task.FromResult("ok");
                },
                TestContext.Current.CancellationToken));

        // Assert
        exception.Errors.ShouldContain(e => e.ErrorMessage == "Ad boş olamaz");
        nextCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenNoValidatorsExist_CallsNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestCommand, string>([]);
        var nextCalled = false;

        // Act
        await behavior.Handle(
            new TestCommand(""),
            () =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            },
            TestContext.Current.CancellationToken);

        nextCalled.ShouldBeTrue();
    }
}
