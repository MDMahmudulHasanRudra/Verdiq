using Verdiq.Application.DTOs.Payroll;

namespace Verdiq.Application.Interfaces;

public interface IPayrollService
{
    Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto, Guid chamberId);
    Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid id, CreateEmployeeDto dto);
    Task DeleteEmployeeAsync(Guid id);
    Task<List<EmployeeResponseDto>> GetEmployeesAsync(Guid chamberId);
    Task<PayrollResponseDto> CreatePayrollAsync(CreatePayrollDto dto, Guid userId, Guid chamberId);
    Task<PayrollResponseDto> ApprovePayrollAsync(Guid id);
    Task<PayrollResponseDto> MarkPayrollPaidAsync(Guid id);
    Task<List<PayrollResponseDto>> GetPayrollsAsync(Guid chamberId, int? month, int? year);
    Task<AttendanceResponseDto> CreateAttendanceAsync(CreateAttendanceDto dto);
    Task<List<AttendanceResponseDto>> GetAttendancesAsync(Guid chamberId, DateTime from, DateTime to);
}
