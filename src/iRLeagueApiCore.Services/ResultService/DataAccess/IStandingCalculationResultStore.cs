﻿using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess;

internal interface IStandingCalculationResultStore
{
    public Task StoreCalculationResult(StandingCalculationResult result, CancellationToken cancellationToken = default);
}
