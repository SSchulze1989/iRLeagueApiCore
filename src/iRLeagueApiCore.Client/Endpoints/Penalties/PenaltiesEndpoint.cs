﻿using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Client.Endpoints.Penalties;
internal sealed class PenaltiesEndpoint : EndpointBase, IPenaltiesEndpoint, IGetAllEndpoint<PenaltyModel>, IPostEndpoint<PenaltyModel, PostPenaltyModel>
{
    public PenaltiesEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) 
        : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Penalties");
    }

    public IPenaltyByIdEndpoint WithId(long id)
    {
        return new PenaltyByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }

    async Task<ClientActionResult<IEnumerable<PenaltyModel>>> IGetEndpoint<IEnumerable<PenaltyModel>>.Get(CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.GetAsClientActionResult<IEnumerable<PenaltyModel>>(QueryUrl, cancellationToken);
    }

    async Task<ClientActionResult<PenaltyModel>> IPostEndpoint<PenaltyModel, PostPenaltyModel>.Post(PostPenaltyModel model, CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.PostAsClientActionResult<PenaltyModel>(QueryUrl, model, cancellationToken);
    }
}
