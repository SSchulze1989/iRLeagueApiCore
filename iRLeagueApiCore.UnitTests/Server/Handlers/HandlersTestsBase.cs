using FluentValidation;
using FluentValidation.Results;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers
{
    public abstract class HandlersTestsBase<THandler, TRequest, TResult> : IClassFixture<DbTestFixture> 
        where THandler : IRequestHandler<TRequest, TResult> 
        where TRequest : class, IRequest<TResult>
    {
        protected readonly DbTestFixture fixture;
        protected readonly ILogger<THandler> logger;

        protected const long testLeagueId = 1;
        protected const long testSeasonId = 1;
        protected const long testScoringId = 1;

        public HandlersTestsBase(DbTestFixture fixture)
        {
            this.fixture = fixture;
            logger = Mock.Of<ILogger<THandler>>();
        }

        protected abstract TRequest DefaultRequest();

        protected abstract THandler CreateTestHandler(LeagueDbContext dbContext, IValidator<TRequest> validator);

        protected ValidationResult ValidationFailed()
        {
            return new ValidationResult(new ValidationFailure[] { new ValidationFailure("Property", "Error") });
        }

        protected virtual void DefaultAssertions(TRequest request, TResult result, LeagueDbContext dbContext)
        {
            Assert.NotNull(result);
        }

        /// <summary>
        /// Run the <see cref="IRequestHandler{TRequest, TResonse}.Handle"/> method and perform default assertions
        /// <para/>Assertions can be modified by overriding <see cref="DefaultAssertions"/>
        /// </summary>
        /// <returns><typeparamref name="TResult"/> Result of the handle method</returns>
        public virtual async Task<TResult> HandleDefaultAsync()
        {
            using var tx = new TransactionScope();
            using var dbContext = fixture.CreateDbContext();

            var request = DefaultRequest();
            var handler = CreateTestHandler(dbContext, MockHelpers.TestValidator<TRequest>());

            var result = await handler.Handle(request, default);
            DefaultAssertions(request, result, dbContext);
            return result;
        }

        /// <summary>
        /// Run the <see cref="IRequestHandler{TRequest, TResonse}.Handle"/> method and assert throwing <see cref="ResourceNotFoundException"/>
        /// </summary>
        /// <param name="request">Request for a not existing resource</param>
        public async Task HandleNotFoundRequestAsync(TRequest request = null)
        {
            request ??= DefaultRequest();
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await HandleSpecialAsync(request, null));
        }

        /// <summary>
        /// Run the <see cref="IRequestHandler{TRequest, TResonse}.Handle"/> method and perform the provided assertions
        /// </summary>
        /// <param name="request">Request for a not existing resource</param>
        /// <param name="assertions">Assertions to be performed</param>
        /// <returns></returns>
        public virtual async Task<TResult> HandleSpecialAsync(TRequest request, Action<TResult> assertions)
        {
            using var tx = new TransactionScope();
            using var dbContext = fixture.CreateDbContext();

            var handler = CreateTestHandler(dbContext, MockHelpers.TestValidator<TRequest>());

            var result = await handler.Handle(request, default);
            assertions?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Run the <see cref="IRequestHandler{TRequest, TResonse}.Handle"/> method and assert throwing <see cref="ValidationException"/>
        /// </summary
        public virtual async Task HandleValidationFailedAsync()
        {
            using var tx = new TransactionScope();
            using var dbContext = fixture.CreateDbContext();
            var mockValidator = MockHelpers.MockValidator<TRequest>();
            mockValidator.Setup(x => x.Validate(It.IsAny<TRequest>()))
                .Returns(ValidationFailed());
            mockValidator.Setup(x => x.ValidateAsync(It.IsAny<TRequest>(), default))
                .ReturnsAsync(ValidationFailed());

            var request = DefaultRequest();
            var handler = CreateTestHandler(dbContext, mockValidator.Object);

            await Assert.ThrowsAsync<ValidationException>(async () => await handler.Handle(request, default));
        }
    }
}
