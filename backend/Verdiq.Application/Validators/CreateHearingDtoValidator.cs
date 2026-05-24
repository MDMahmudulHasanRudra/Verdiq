using FluentValidation;
using Verdiq.Application.DTOs.Hearing;

namespace Verdiq.Application.Validators;

public class CreateHearingDtoValidator : AbstractValidator<CreateHearingDto>
{
    public CreateHearingDtoValidator()
    {
        RuleFor(x => x.CaseId)
            .NotEmpty().WithMessage("Case ID is required");

        RuleFor(x => x.HearingDate)
            .NotEmpty().WithMessage("Hearing date is required");

        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("Time is required")
            .MaximumLength(20);

        RuleFor(x => x.Court)
            .NotEmpty().WithMessage("Court is required")
            .MaximumLength(255);

        RuleFor(x => x.HearingType)
            .NotEmpty().WithMessage("Hearing type is required")
            .MaximumLength(100);
    }
}
