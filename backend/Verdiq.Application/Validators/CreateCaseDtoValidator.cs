using FluentValidation;
using Verdiq.Application.DTOs.Case;

namespace Verdiq.Application.Validators;

public class CreateCaseDtoValidator : AbstractValidator<CreateCaseDto>
{
    public CreateCaseDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CaseNumber).MaximumLength(50);
        RuleFor(x => x.CourtName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CaseType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FilingDate).NotEmpty();
    }
}
