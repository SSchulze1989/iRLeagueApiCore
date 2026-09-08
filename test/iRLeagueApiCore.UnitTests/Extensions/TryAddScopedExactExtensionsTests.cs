using iRLeagueApiCore.Server.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace iRLeagueApiCore.UnitTests.Extensions;

public sealed class TryAddScopedExactExtensionsTests
{
    [Fact]
    public void Generic_ServiceWithImplementation_ShouldRegister_WhenServiceTypeDoesNotExist()
    {
        var services = new ServiceCollection();

        services.TryAddScopedExact<ITestService, TestService>();

        services.Should().ContainSingle(x =>
            x.ServiceType == typeof(ITestService)
            && x.ImplementationType == typeof(TestService)
            && x.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void Generic_ServiceWithImplementation_ShouldRegister_WhenServiceTypeExistsWithDifferentImplementation()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITestService, OtherTestService>();

        services.TryAddScopedExact<ITestService, TestService>();

        services.Count(x => x.ServiceType == typeof(ITestService)).Should().Be(2);
        services.Should().Contain(x => x.ServiceType == typeof(ITestService) && x.ImplementationType == typeof(OtherTestService));
        services.Should().Contain(x => x.ServiceType == typeof(ITestService) && x.ImplementationType == typeof(TestService));
    }

    [Fact]
    public void Generic_ServiceWithImplementation_ShouldNotRegister_WhenServiceAndImplementationMatch()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITestService, TestService>();

        services.TryAddScopedExact<ITestService, TestService>();

        services.Count(x => x.ServiceType == typeof(ITestService) && x.ImplementationType == typeof(TestService)).Should().Be(1);
    }

    [Fact]
    public void NonGeneric_ServiceWithImplementation_ShouldNotRegister_WhenServiceAndImplementationMatch()
    {
        var services = new ServiceCollection();
        services.AddScoped(typeof(ITestService), typeof(TestService));

        services.TryAddScopedExact(typeof(ITestService), typeof(TestService));

        services.Count(x => x.ServiceType == typeof(ITestService) && x.ImplementationType == typeof(TestService)).Should().Be(1);
    }

    [Fact]
    public void Generic_SelfBinding_ShouldRegister_WhenSelfBindingDoesNotExist()
    {
        var services = new ServiceCollection();

        services.TryAddScopedExact<SelfBoundService>();

        services.Should().ContainSingle(x =>
            x.ServiceType == typeof(SelfBoundService)
            && x.ImplementationType == typeof(SelfBoundService)
            && x.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void NonGeneric_SelfBinding_ShouldNotRegister_WhenSelfBindingExists()
    {
        var services = new ServiceCollection();
        services.AddScoped<SelfBoundService>();

        services.TryAddScopedExact(typeof(SelfBoundService));

        services.Count(x => x.ServiceType == typeof(SelfBoundService) && x.ImplementationType == typeof(SelfBoundService)).Should().Be(1);
    }

    [Fact]
    public void Generic_Factory_ShouldRegister_WhenServiceTypeDoesNotExist()
    {
        var services = new ServiceCollection();
        Func<IServiceProvider, ITestService> factory = _ => new TestService();

        services.TryAddScopedExact(factory);

        services.Should().ContainSingle(x =>
            x.ServiceType == typeof(ITestService)
            && x.ImplementationFactory == factory
            && x.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void Generic_Factory_ShouldNotRegister_WhenServiceAndFactoryMatch()
    {
        var services = new ServiceCollection();
        Func<IServiceProvider, ITestService> factory = _ => new TestService();
        services.AddScoped(factory);

        services.TryAddScopedExact(factory);

        services.Count(x => x.ServiceType == typeof(ITestService) && x.ImplementationFactory == factory).Should().Be(1);
    }

    [Fact]
    public void NonGeneric_Factory_ShouldRegister_WhenServiceTypeExistsWithDifferentFactory()
    {
        var services = new ServiceCollection();
        Func<IServiceProvider, object> factory1 = _ => new TestService();
        Func<IServiceProvider, object> factory2 = _ => new TestService();
        services.AddScoped(typeof(ITestService), factory1);

        services.TryAddScopedExact(typeof(ITestService), factory2);

        services.Count(x => x.ServiceType == typeof(ITestService)).Should().Be(2);
        services.Should().Contain(x => x.ServiceType == typeof(ITestService) && x.ImplementationFactory == factory1);
        services.Should().Contain(x => x.ServiceType == typeof(ITestService) && x.ImplementationFactory == factory2);
    }

    [Fact]
    public void NonGeneric_ServiceWithImplementation_ShouldThrow_WhenServiceTypeIsNull()
    {
        var services = new ServiceCollection();

        Action action = () => services.TryAddScopedExact((Type)null!, typeof(TestService));

        action.Should().Throw<ArgumentNullException>().WithParameterName("serviceType");
    }

    private interface ITestService { }
    private sealed class TestService : ITestService { }
    private sealed class OtherTestService : ITestService { }
    private sealed class SelfBoundService { }
}
