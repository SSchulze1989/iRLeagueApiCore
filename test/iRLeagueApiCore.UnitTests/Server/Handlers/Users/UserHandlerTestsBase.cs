using AutoFixture.Dsl;
using FluentValidation;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.Logging;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Users;
public abstract class UserHandlerTestsBase<THandler, TRequest> : IClassFixture<IdentityFixture>
{
    protected readonly Fixture fixture;
    protected readonly IdentityFixture identityFixture;
    protected readonly ILogger<THandler> logger;
    protected readonly IEnumerable<IValidator<TRequest>> validators;

    public UserHandlerTestsBase(IdentityFixture identityFixture)
    {
        this.identityFixture = identityFixture;
        fixture = new();
        logger = Mock.Of<ILogger<THandler>>();
        validators = Array.Empty<IValidator<TRequest>>();
    }

    protected UserManager<ApplicationUser> TestUserManager(IEnumerable<ApplicationUser>? users = null)
    {
        users ??= fixture.CreateMany<ApplicationUser>();
        var manager = MockHelpers.MockUserManager<ApplicationUser>();
        manager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => users.FirstOrDefault(x => x.Id == id)!);
        manager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => users.FirstOrDefault(x => x.UserName == name)!);
        return manager.Object;
    }

    protected IPostprocessComposer<ApplicationUser> UserBuilder()
    {
        return fixture.Build<ApplicationUser>()
            .With(x => x.FullName, () => GetFullName(fixture.Create<string>().Substring(0,10), fixture.Create<string>().Substring(0,10)));
    }

    protected string GetFullName(string firstname, string lastname)
    {
        return $"{firstname};{lastname}";
    }

    protected (string? firstname, string? lastname) GetFirstnameLastname(string? fullName)
    {
        var parts = fullName?.Split(';');
        return (parts?.ElementAtOrDefault(0), parts?.ElementAtOrDefault(1));
    }
}
