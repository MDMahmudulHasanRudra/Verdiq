using Verdiq.Application.DTOs.Template;

namespace Verdiq.Application.Interfaces;

public interface ITemplateService
{
    Task<(bool Success, string Message, TemplateResponseDto? Data)> CreateAsync(CreateTemplateDto dto);
    Task<IEnumerable<TemplateResponseDto>> GetAllAsync(string? category = null);
    Task<TemplateResponseDto?> GetByIdAsync(Guid id);
    Task<string> RenderTemplateAsync(Guid templateId, Dictionary<string, string> variables);
}
