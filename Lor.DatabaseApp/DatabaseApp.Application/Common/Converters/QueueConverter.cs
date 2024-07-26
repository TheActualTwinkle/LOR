using DatabaseApp.Application.Queue;

namespace DatabaseApp.Application.Common.Converters;

public static class QueueConverter
{
    public static Task Handle(this QueueDto queueDto, List<string> queueList)
    {
        queueDto.QueueList = queueList;

        return Task.CompletedTask;
    }
}