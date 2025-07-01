using FluentResults;
using Hangfire;

namespace DatabaseApp.Application.Services.ReminderService.Common;

public static class ExecutionTimeProvider
{
    public static Result<DateTime> GetNextExecutionTime(string jobId, string queue = "dba_queue")
    {
        try
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            
            var scheduledCount = monitoringApi.ScheduledCount();
            
            var offset = 0;
            const int batchSize = 1000;

            while (offset < scheduledCount)
            {
                var scheduledJobs = monitoringApi.ScheduledJobs(offset, batchSize);

                var scheduledJobDto = scheduledJobs
                    .FirstOrDefault(x => x.Key == jobId && x.Value.Job.Queue == queue)
                    .Value;

                if (scheduledJobDto != null)
                    return Result.Ok(scheduledJobDto.EnqueueAt);

                offset += batchSize;
            }

            return Result.Fail("Job not found");
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}