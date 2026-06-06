namespace Verdiq.Application.DTOs.Payroll;

public class CreateEmployeeDto
{
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
    public Guid? UserId { get; set; }
}

public class EmployeeResponseDto
{
    public Guid Id { get; set; }
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
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreatePayrollDto
{
    public Guid EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Overtime { get; set; }
    public decimal Deductions { get; set; }
    public decimal TaxDeduction { get; set; }
}

public class PayrollResponseDto
{
    public Guid Id { get; set; }
    public string PayrollNumber { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Overtime { get; set; }
    public decimal Deductions { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal NetSalary { get; set; }
    public DateTime? PaidAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAttendanceDto
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string? Notes { get; set; }
}

public class AttendanceResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string? Notes { get; set; }
}
