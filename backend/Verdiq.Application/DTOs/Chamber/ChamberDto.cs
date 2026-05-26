namespace Verdiq.Application.DTOs.Chamber;

public class CreateChamberDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
}

public class UpdateChamberDto
{
    public string? Name { get; set; }
    public string? Logo { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
}

public class ChamberResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string SubscriptionPlan { get; set; } = string.Empty;
    public int UsersCount { get; set; }
    public int CasesCount { get; set; }
    public int ClientsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
