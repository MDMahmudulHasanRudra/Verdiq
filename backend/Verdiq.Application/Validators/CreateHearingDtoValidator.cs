using FluentValidation;
using Verdiq.Application.DTOs.Hearing;

namespace Verdiq.Application.Validators;

public class CreateHearingDtoValidator : AbstractValidator<CreateHearingDto>
{
    public CreateHearingDtoValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.HearingDate).NotEmpty();
    }
}
