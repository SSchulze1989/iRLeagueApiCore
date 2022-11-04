﻿using iRLeagueApiCore.Services.ResultService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal sealed class SessionResultCalculationServiceProvider :
        ICalculationServiceProvider<SessionResultCalculationConfiguration, SessionResultCalculationData, SessionResultCalculationResult>
    {
        public ICalculationService<SessionResultCalculationData, SessionResultCalculationResult> GetCalculationService(SessionResultCalculationConfiguration config)
        {
            return config.ScoringKind switch
            {
                Common.Enums.ScoringKind.Member => new MemberSessionResultCalculationService(config),
                Common.Enums.ScoringKind.Team => throw new NotImplementedException("Team scoring is not implemented"),
                Common.Enums.ScoringKind.Custom => throw new NotImplementedException("Custom scoring is not implemented"),
                _ => throw new InvalidOperationException($"Unknown Scoring Kind: {config.ScoringKind}"),
            };
        }
    }
}
