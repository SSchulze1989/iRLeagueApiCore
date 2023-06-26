namespace iRLeagueApiCore.Server.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class RequireSubscriptionAttribute : Attribute
{
}
