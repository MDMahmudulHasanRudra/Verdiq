namespace Verdiq.Domain.Entities;

public class ChamberSettings : BaseEntity
{
    public Guid ChamberId { get; set; }
    public virtual Chamber Chamber { get; set; } = null!;
    public string SettingsJson { get; set; } = "{}";
    public Guid? UpdatedBy { get; set; }
    public virtual User? Updater { get; set; }
}
