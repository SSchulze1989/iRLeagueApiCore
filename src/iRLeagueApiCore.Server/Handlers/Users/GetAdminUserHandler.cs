﻿using FluentValidation;
using iRLeagueApiCore.Common.Models.Users;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Users
{
    public record GetAdminUserRequest(string UserId) : IRequest<AdminUserModel>;

    public class GetAdminUserHandler : UsersHandlerBase<GetAdminUserHandler, GetAdminUserRequest>, IRequestHandler<GetAdminUserRequest, AdminUserModel>
    {
        public GetAdminUserHandler(ILogger<GetAdminUserHandler> logger, UserDbContext userDbContext, UserManager<ApplicationUser> userManager, 
            IEnumerable<IValidator<GetAdminUserRequest>> validators) : base(logger, userDbContext, userManager, validators)
        {
        }

        public async Task<AdminUserModel> Handle(GetAdminUserRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var user = await GetUserAsync(request.UserId)
                ?? throw new ResourceNotFoundException();
            var getUser = await MapToAdminUserModelAsync(user, new());
            return getUser;
        }
    }
}