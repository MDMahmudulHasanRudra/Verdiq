using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Payroll;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Services;

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _context;
    public PayrollService(AppDbContext context) => _context = context;

    public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto, Guid chamberId)
    {
        var count = await _context.Set<Employee>().CountAsync(e => e.ChamberId == chamberId);
        var emp = new Employee
        {
            EmployeeCode = $"EMP-{DateTime.UtcNow:yyyy}-{count + 1:D4}",
            FullName = dto.FullName, Email = dto.Email, Phone = dto.Phone,
            Designation = dto.Designation, Department = dto.Department,
            JoinDate = dto.JoinDate, BaseSalary = dto.BaseSalary,
            BankName = dto.BankName, BankAccountNo = dto.BankAccountNo,
            NidNo = dto.NidNo, TinNo = dto.TinNo, UserId = dto.UserId,
            ChamberId = chamberId
        };
        _context.Set<Employee>().Add(emp);
        await _context.SaveChangesAsync();
        return MapEmployee(emp);
    }

    public async Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid id, CreateEmployeeDto dto)
    {
        var emp = await _context.Set<Employee>().FindAsync(id) ?? throw new KeyNotFoundException("Employee not found");
        emp.FullName = dto.FullName; emp.Email = dto.Email; emp.Phone = dto.Phone;
        emp.Designation = dto.Designation; emp.Department = dto.Department;
        emp.JoinDate = dto.JoinDate; emp.BaseSalary = dto.BaseSalary;
        emp.BankName = dto.BankName; emp.BankAccountNo = dto.BankAccountNo;
        emp.NidNo = dto.NidNo; emp.TinNo = dto.TinNo; emp.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapEmployee(emp);
    }

    public async Task DeleteEmployeeAsync(Guid id)
    {
        var emp = await _context.Set<Employee>().FindAsync(id) ?? throw new KeyNotFoundException("Employee not found");
        emp.IsDeleted = true; emp.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<EmployeeResponseDto>> GetEmployeesAsync(Guid chamberId)
    {
        return await _context.Set<Employee>()
            .Where(e => e.ChamberId == chamberId && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => MapEmployee(e))
            .ToListAsync();
    }

    public async Task<PayrollResponseDto> CreatePayrollAsync(CreatePayrollDto dto, Guid userId, Guid chamberId)
    {
        var emp = await _context.Set<Employee>().FindAsync(dto.EmployeeId)
            ?? throw new KeyNotFoundException("Employee not found");

        var count = await _context.Set<Payroll>().CountAsync(p => p.ChamberId == chamberId);
        var netSalary = dto.GrossSalary + (dto.Bonus ?? 0) + (dto.Overtime ?? 0) - dto.Deductions - dto.TaxDeduction;

        var payroll = new Payroll
        {
            PayrollNumber = $"PR-{dto.Year}-{count + 1:D4}",
            EmployeeId = dto.EmployeeId, Month = dto.Month, Year = dto.Year,
            GrossSalary = dto.GrossSalary, Bonus = dto.Bonus, Overtime = dto.Overtime,
            Deductions = dto.Deductions, TaxDeduction = dto.TaxDeduction, NetSalary = netSalary,
            ChamberId = chamberId, CreatedById = userId
        };
        _context.Set<Payroll>().Add(payroll);
        await _context.SaveChangesAsync();
        return MapPayroll(payroll, emp.FullName, emp.EmployeeCode);
    }

    public async Task<PayrollResponseDto> ApprovePayrollAsync(Guid id)
    {
        var p = await _context.Set<Payroll>().Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Payroll not found");
        p.Status = PayrollStatus.Approved; p.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapPayroll(p, p.Employee.FullName, p.Employee.EmployeeCode);
    }

    public async Task<PayrollResponseDto> MarkPayrollPaidAsync(Guid id)
    {
        var p = await _context.Set<Payroll>().Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Payroll not found");
        p.Status = PayrollStatus.Paid; p.PaidAt = DateTime.UtcNow; p.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapPayroll(p, p.Employee.FullName, p.Employee.EmployeeCode);
    }

    public async Task<List<PayrollResponseDto>> GetPayrollsAsync(Guid chamberId, int? month, int? year)
    {
        var q = _context.Set<Payroll>().Include(p => p.Employee)
            .Where(p => p.ChamberId == chamberId && !p.IsDeleted);
        if (month.HasValue) q = q.Where(p => p.Month == month.Value);
        if (year.HasValue) q = q.Where(p => p.Year == year.Value);
        return await q.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
            .Select(p => MapPayroll(p, p.Employee.FullName, p.Employee.EmployeeCode))
            .ToListAsync();
    }

    public async Task<AttendanceResponseDto> CreateAttendanceAsync(CreateAttendanceDto dto)
    {
        var att = new Attendance
        {
            EmployeeId = dto.EmployeeId, Date = dto.Date,
            Status = Enum.Parse<AttendanceStatus>(dto.Status),
            CheckIn = dto.CheckIn, CheckOut = dto.CheckOut, Notes = dto.Notes
        };
        _context.Set<Attendance>().Add(att);
        await _context.SaveChangesAsync();
        return new AttendanceResponseDto
        {
            Id = att.Id, EmployeeId = att.EmployeeId, Date = att.Date,
            Status = att.Status.ToString(), CheckIn = att.CheckIn,
            CheckOut = att.CheckOut, Notes = att.Notes
        };
    }

    public async Task<List<AttendanceResponseDto>> GetAttendancesAsync(Guid chamberId, DateTime from, DateTime to)
    {
        return await _context.Set<Attendance>().Include(a => a.Employee)
            .Where(a => a.Employee.ChamberId == chamberId && a.Date >= from && a.Date <= to && !a.IsDeleted)
            .OrderByDescending(a => a.Date)
            .Select(a => new AttendanceResponseDto
            {
                Id = a.Id, EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.FullName,
                Date = a.Date, Status = a.Status.ToString(),
                CheckIn = a.CheckIn, CheckOut = a.CheckOut, Notes = a.Notes
            }).ToListAsync();
    }

    private static EmployeeResponseDto MapEmployee(Employee e) => new()
    {
        Id = e.Id, EmployeeCode = e.EmployeeCode, FullName = e.FullName,
        Email = e.Email, Phone = e.Phone, Designation = e.Designation,
        Department = e.Department, JoinDate = e.JoinDate, BaseSalary = e.BaseSalary,
        BankName = e.BankName, BankAccountNo = e.BankAccountNo,
        NidNo = e.NidNo, TinNo = e.TinNo, Status = e.Status.ToString(),
        CreatedAt = e.CreatedAt
    };

    private static PayrollResponseDto MapPayroll(Payroll p, string name, string code) => new()
    {
        Id = p.Id, PayrollNumber = p.PayrollNumber, EmployeeId = p.EmployeeId,
        EmployeeName = name, EmployeeCode = code, Month = p.Month, Year = p.Year,
        GrossSalary = p.GrossSalary, Bonus = p.Bonus, Overtime = p.Overtime,
        Deductions = p.Deductions, TaxDeduction = p.TaxDeduction, NetSalary = p.NetSalary,
        PaidAt = p.PaidAt, Status = p.Status.ToString(), CreatedAt = p.CreatedAt
    };
}
