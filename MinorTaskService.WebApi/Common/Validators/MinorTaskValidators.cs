using FluentValidation;
using MinorTaskService.WebApi.Common.Enums;
using MinorTaskService.WebApi.Common.Extensions;
using MinorTaskService.WebApi.Common.Options;
using MinorTaskService.WebApi.Mediator;

namespace MinorTaskService.WebApi.Common.Validators
{
    public class CreateMinorTaskCommandValidator : AbstractValidator<CreateMinorTaskCommand>
    {
        public CreateMinorTaskCommandValidator()
        {
            RuleFor(x => x.Name).ValidateName();
            RuleFor(x => x.Description).ValidateDescription();
            RuleFor(x => x.Latitude)
                .GreaterThanOrEqualTo(ValidationOptions.MinLatitude)
                .LessThanOrEqualTo(ValidationOptions.MaxLatitude)
                .WithMessage($"Latitude must be between {ValidationOptions.MinLatitude} and {ValidationOptions.MaxLatitude}");
            RuleFor(x => x.Longitude)
                .GreaterThanOrEqualTo(ValidationOptions.MinLongitude)
                .LessThanOrEqualTo(ValidationOptions.MaxLongitude)
                .WithMessage($"Longitude must be between {ValidationOptions.MinLongitude} and {ValidationOptions.MaxLongitude}");
            RuleFor(x => x.NumberVolunteers)
                .GreaterThanOrEqualTo(ValidationOptions.MinNumberVolunteers)
                .LessThanOrEqualTo(ValidationOptions.MaxNumberVolunteers)
                .WithMessage($"NumberVolunteers must be between {ValidationOptions.MinNumberVolunteers} and {ValidationOptions.MaxNumberVolunteers}");
            RuleFor(x => x.Encouragement)
                .GreaterThanOrEqualTo(ValidationOptions.MinEncouragement)
                .LessThanOrEqualTo(ValidationOptions.MaxEncouragement)
                .WithMessage($"Encouragement must be between {ValidationOptions.MinNumberVolunteers} and {ValidationOptions.MaxNumberVolunteers}");
        }
    }

    public class DeleteMinorTaskCommandValidator : AbstractValidator<DeleteMinorTaskCommand>
    {
        public DeleteMinorTaskCommandValidator()
        {
            RuleFor(x => x.MinorTaskId)
                .NotEmpty()
                .WithMessage("MinorTaskId  can not be empty");
        }
    }

    public class DeleteMinorTaskParticipantCommandValidator : AbstractValidator<DeleteMinorTaskParticipantCommand>
    {
        public DeleteMinorTaskParticipantCommandValidator()
        {
            RuleFor(x => x.MinorTaskId)
                .NotEmpty()
                .WithMessage("MinorTaskId can not be empty");
            RuleFor(x => x.ParticipantId)
                .NotEmpty()
                .WithMessage("ParticipantId can not be empty");
        }
    }

    public class GetMinorTaskByIdQueryValidator : AbstractValidator<GetMinorTaskByIdQuery>
    {
        public GetMinorTaskByIdQueryValidator()
        {
            RuleFor(x => x.MinorTaskId)
                .NotEmpty()
                .WithMessage("MinorTaskId  can not be empty");
        }
    }

    public class GetMinorTasksQueryValidator : AbstractValidator<GetMinorTasksQuery>
    {
        public GetMinorTasksQueryValidator()
        {
            RuleFor(x => x)
                .Must(HaveExactlyOnePaginationMethod)
                .WithMessage("You must specify either 'Limit' (for first N items) or both 'From' and 'To' (for range). But not both, and not none");

            When(x => x.Limit.HasValue, () => {
                RuleFor(x => x.Limit)
                    .GreaterThan(0)
                    .LessThanOrEqualTo(100)
                    .WithMessage("Limit must be between 1 and 1000");
            });

            When(x => x.From.HasValue && x.To.HasValue, () => {
                RuleFor(x => x.From)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("From must be >= 0");

                RuleFor(x => x.To)
                    .GreaterThan(x => x.From ?? 0)
                    .WithMessage("To must be greater than From");

                RuleFor(x => x)
                    .Must(x => x.To - x.From <= 1000)
                    .WithMessage("Range size (To - From) cannot exceed 1000");
            });
        }

        private static bool HaveExactlyOnePaginationMethod(GetMinorTasksQuery query)
        {
            bool hasLimit = query.Limit.HasValue;
            bool hasRange = query.From.HasValue && query.To.HasValue;

            return hasLimit ^ hasRange;
        }
    }

    public class UpdateMinorTaskCommandValidator : AbstractValidator<UpdateMinorTaskCommand>
    {
        public UpdateMinorTaskCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id can not be empty");
            RuleFor(x => x.Name).ValidateName();
            RuleFor(x => x.Description).ValidateDescription();
            RuleFor(x => x.Latitude)
                .GreaterThanOrEqualTo(ValidationOptions.MinLatitude)
                .LessThanOrEqualTo(ValidationOptions.MaxLatitude)
                .WithMessage($"Latitude must be between {ValidationOptions.MinLatitude} and {ValidationOptions.MaxLatitude}");
            RuleFor(x => x.Longitude)
                .GreaterThanOrEqualTo(ValidationOptions.MinLongitude)
                .LessThanOrEqualTo(ValidationOptions.MaxLongitude)
                .WithMessage($"Longitude must be between {ValidationOptions.MinLongitude} and {ValidationOptions.MaxLongitude}");
            RuleFor(x => x.NumberVolunteers)
                .GreaterThanOrEqualTo(ValidationOptions.MinNumberVolunteers)
                .LessThanOrEqualTo(ValidationOptions.MaxNumberVolunteers)
                .WithMessage($"NumberVolunteers must be between {ValidationOptions.MinNumberVolunteers} and {ValidationOptions.MaxNumberVolunteers}");
            RuleFor(x => x.Encouragement)
                .GreaterThanOrEqualTo(ValidationOptions.MinEncouragement)
                .LessThanOrEqualTo(ValidationOptions.MaxEncouragement)
                .WithMessage($"Encouragement must be between {ValidationOptions.MinNumberVolunteers} and {ValidationOptions.MaxNumberVolunteers}");

        }
    }

    public class UpdateMinorTaskStatusCommandValidator : AbstractValidator<UpdateMinorTaskStatusCommand>
    {
        readonly List<Guid> allowedStatuses = new()
        {
            StatusType.InSearch,
            StatusType.InProgress,
            StatusType.Completed
        };

        public UpdateMinorTaskStatusCommandValidator()
        {
            RuleFor(x => x.MinorTaskId)
                .NotEmpty()
                .WithMessage("MinorTaskId can not be empty");

            RuleFor(x => x.StatusId)
                .Must(allowedStatuses.Contains)
                .WithMessage($"Status should be one of: {String.Join(", ", allowedStatuses)}");
        }
    }
}
