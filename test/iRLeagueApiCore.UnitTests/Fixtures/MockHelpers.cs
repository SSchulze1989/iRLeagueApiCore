// From https://github.com/dotnet/aspnetcore/blob/main/src/Identity/test/Shared/MockHelpers.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq.Protected;
using System.Linq.Expressions;
using System.Text;

namespace Microsoft.AspNetCore.Identity.Test
{
    public static class MockHelpers
    {
        public static StringBuilder LogMessage = new StringBuilder();

        public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
            return mgr;
        }

        public static Mock<RoleManager<TRole>> MockRoleManager<TRole>(IRoleStore<TRole> store = null) where TRole : class
        {
            store ??= new Mock<IRoleStore<TRole>>().Object;
            var roles = new List<IRoleValidator<TRole>>();
            roles.Add(new RoleValidator<TRole>());
            return new Mock<RoleManager<TRole>>(store, roles, MockLookupNormalizer(),
                new IdentityErrorDescriber(), null);
        }

        public static UserManager<TUser> TestUserManager<TUser>(IUserStore<TUser> store = null) where TUser : class
        {
            store ??= new Mock<IUserStore<TUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<TUser>>();
            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<TUser>>
            {
                new PasswordValidator<TUser>()
            };
            var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
                userValidators, pwdValidators, MockLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<TUser>>>().Object);
            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
            return userManager;
        }

        public static RoleManager<TRole> TestRoleManager<TRole>(IRoleStore<TRole> store = null) where TRole : class
        {
            store ??= new Mock<IRoleStore<TRole>>().Object;
            var roles = new List<IRoleValidator<TRole>>
            {
                new RoleValidator<TRole>()
            };
            return new RoleManager<TRole>(store, roles,
                MockLookupNormalizer(),
                new IdentityErrorDescriber(),
                null);
        }

        public static ILookupNormalizer MockLookupNormalizer()
        {
            var normalizerFunc = new Func<string, string>(i =>
            {
                if (i == null)
                {
                    return null;
                }
                else
                {
                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(i)).ToUpperInvariant();
                }
            });
            var lookupNormalizer = new Mock<ILookupNormalizer>();
            lookupNormalizer.Setup(i => i.NormalizeName(It.IsAny<string>())).Returns(normalizerFunc);
            lookupNormalizer.Setup(i => i.NormalizeEmail(It.IsAny<string>())).Returns(normalizerFunc);
            return lookupNormalizer.Object;
        }

        public static Mock<IValidator<T>> MockValidator<T>()
        {
            return new Mock<IValidator<T>>();
        }

        public static IValidator<T> TestValidator<T>()
        {
            var mockValidator = MockValidator<T>();
            mockValidator.Setup(x => x.Validate(It.IsAny<T>()))
                .Returns(new FluentValidation.Results.ValidationResult()).Verifiable();
            mockValidator.Setup(x => x.ValidateAsync(It.IsAny<T>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult()).Verifiable();
            return mockValidator.Object;
        }

        public static IEnumerable<IValidator<T>> TestValidators<T>()
        {
            return new IValidator<T>[] { TestValidator<T>() };
        }

        /// <summary>
        /// Get a test mediator that returns the given result for specified reuqest type 
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="match">Predicate to check if request contains correct information. <para/>Returns true by default</param>
        /// <param name="result">Result that should be returned from <see cref="IMediator.Send(object, System.Threading.CancellationToken)"/></param>
        /// <param name="throws">If set a call to <see cref="IMediator.Send(object, System.Threading.CancellationToken)"/> will throw the provided Exception instead</param>
        /// <returns>Configured <see cref="IMediator"/></returns>
        public static IMediator TestMediator<TRequest, TResult>(Expression<Func<TRequest, bool>> match = default,
            TResult result = default, Exception throws = default)
            where TRequest : IRequest<TResult>
        {
            match ??= x => true;
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.Is(match), default))
                .ReturnsAsync(() =>
                {
                    if (throws != null)
                    {
                        throw throws;
                    }
                    return result;
                })
                .Verifiable();
            return mockMediator.Object;
        }

        public static HttpMessageHandler TestMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> result)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage message, CancellationToken cancellationToken) =>
                {
                    return result.Invoke(message);
                });
            return mockHttpMessageHandler.Object;
        }
    }
}