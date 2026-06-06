using Verdiq.Domain.Enums;

namespace Verdiq.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public DateTime JoinDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string? NidNo { get; set; }
    public string? TinNo { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}

public class Payroll : BaseEntity
{
    public string PayrollNumber { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Overtime { get; set; }
    public decimal Deductions { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public DateTime? PaidAt { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
}

public class Attendance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string? Notes { get; set; }
}
