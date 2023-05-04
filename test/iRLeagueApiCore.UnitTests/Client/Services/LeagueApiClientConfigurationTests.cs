using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.Service;
using Microsoft.Extensions.DependencyInjection;

namespace iRLeagueApiCore.UnitTests.Client.Services;
public sealed class LeagueApiClientConfigurationTests
{
    private readonly IServiceCollection services = new ServiceCollection();

    [Fact]
    public void ShouldRegisterNewTokenStore()
    {
        var mockTokenStore1 = Mock.Of<ITokenStore>();
        var mockTokenStore2 = Mock.Of<ITokenStore>();
        services.AddScoped(sp => mockTokenStore1);

        var sut = CreateSut();
        sut.UseTokenStore(sp => mockTokenStore2);

        var tokenStore = services.BuildServiceProvider().GetRequiredService<ITokenStore>();
        tokenStore.Should().NotBeSameAs(mockTokenStore1);
        tokenStore.Should().BeSameAs(mockTokenStore2);
    }

    private LeagueApiClientConfiguration CreateSut()
    {
        return new(services);
    }
}
