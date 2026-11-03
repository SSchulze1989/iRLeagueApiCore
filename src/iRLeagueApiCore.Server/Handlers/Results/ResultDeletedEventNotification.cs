using MediatR;

namespace iRLeagueApiCore.Server.Handlers.Results;

public sealed record ResultDeletedEventNotification(long EventId) : INotification;
