﻿namespace iRLeagueApiCore.Server.Handlers.Schedules;

public record DeleteScheduleRequest(long ScheduleId) : IRequest<Unit>;

public sealed class DeleteScheduleHandler : ScheduleHandlerBase<DeleteScheduleHandler, DeleteScheduleRequest, Unit>
{
    public DeleteScheduleHandler(ILogger<DeleteScheduleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteScheduleRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteScheduleRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        await DeleteSchedule(request.ScheduleId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task DeleteSchedule(long scheduleId, CancellationToken cancellationToken)
    {
        var schedule = await dbContext.Schedules
            .SingleOrDefaultAsync(x => x.ScheduleId == scheduleId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dbContext.Remove(schedule);
    }
}
