using FluentValidation;
using Verdiq.Application.DTOs.Case;

namespace Verdiq.Application.Validators;

public class CreateCaseDtoValidator : AbstractValidator<CreateCaseDto>
{
    public CreateCaseDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(500);

        RuleFor(x => x.CaseType)
            .NotEmpty().WithMessage("Case type is required")
            .MaximumLength(100);

        RuleFor(x => x.Court)
            .NotEmpty().WithMessage("Court is required")
            .MaximumLength(255);

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required");

        RuleFor(x => x.Priority)
            .Must(p => p is null || new[] { "low", "medium", "high" }.Contains(p.ToLower()))
            .WithMessage("Priority must be: Low, Medium, or High");
    }
}
