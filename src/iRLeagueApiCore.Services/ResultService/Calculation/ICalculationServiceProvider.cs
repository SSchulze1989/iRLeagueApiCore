namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    public interface ICalculationServiceProvider<TConfig, TService, TIn, TOut> where TService : ICalculationService<TIn, TOut>
    {
        public TService GetCalculationService(TConfig config);
    }
}
