using FluentValidation;

namespace DatabaseApp.Application.Queue.Commands.DeleteQueue;

public class DeleteQueueCommandValidator : AbstractValidator<DeleteQueueCommand>
{
    public DeleteQueueCommandValidator()
    {
        RuleFor(x => x.OutdatedClassList).NotEmpty().NotNull();
        RuleFor(x => x.UserId).Must(NullOrValidUserId);
        RuleFor(x => x.ClassId).Must(NullOrValidClassId);
    }

    private bool NullOrValidUserId(long? userId)
    {
        switch (userId)
        {
            case null:
            case > 0:
                return true;
            default:
                return false;
        }
    }
    
    private bool NullOrValidClassId(int? classId)
    {
        switch (classId)
        {
            case null:
            case > 0:
                return true;
            default:
                return false;
        }
    }
}