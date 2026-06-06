using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Payroll;
using Verdiq.Application.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/payroll")]
[Authorize]
public class PayrollController : BaseController
{
    private readonly IPayrollService _service;
    public PayrollController(IPayrollService service) => _service = service;

    [HttpGet("employees")]
    public async Task<ActionResult<ApiResponse<List<EmployeeResponseDto>>>> GetEmployees()
        => Ok(ApiResponse<List<EmployeeResponseDto>>.Ok(await _service.GetEmployeesAsync(GetChamberId())));

    [HttpPost("employees")]
    public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> CreateEmployee([FromBody] CreateEmployeeDto dto)
        => Ok(ApiResponse<EmployeeResponseDto>.Ok(await _service.CreateEmployeeAsync(dto, GetChamberId())));

    [HttpPut("employees/{id}")]
    public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> UpdateEmployee(Guid id, [FromBody] CreateEmployeeDto dto)
        => Ok(ApiResponse<EmployeeResponseDto>.Ok(await _service.UpdateEmployeeAsync(id, dto)));

    [HttpDelete("employees/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEmployee(Guid id)
    {
        await _service.DeleteEmployeeAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Employee deleted"));
    }

    [HttpGet("payrolls")]
    public async Task<ActionResult<ApiResponse<List<PayrollResponseDto>>>> GetPayrolls([FromQuery] int? month, [FromQuery] int? year)
        => Ok(ApiResponse<List<PayrollResponseDto>>.Ok(await _service.GetPayrollsAsync(GetChamberId(), month, year)));

    [HttpPost("payrolls")]
    public async Task<ActionResult<ApiResponse<PayrollResponseDto>>> CreatePayroll([FromBody] CreatePayrollDto dto)
        => Ok(ApiResponse<PayrollResponseDto>.Ok(await _service.CreatePayrollAsync(dto, GetUserId(), GetChamberId())));

    [HttpPost("payrolls/{id}/approve")]
    public async Task<ActionResult<ApiResponse<PayrollResponseDto>>> ApprovePayroll(Guid id)
        => Ok(ApiResponse<PayrollResponseDto>.Ok(await _service.ApprovePayrollAsync(id)));

    [HttpPost("payrolls/{id}/pay")]
    public async Task<ActionResult<ApiResponse<PayrollResponseDto>>> MarkPaid(Guid id)
        => Ok(ApiResponse<PayrollResponseDto>.Ok(await _service.MarkPayrollPaidAsync(id)));

    [HttpGet("attendance")]
    public async Task<ActionResult<ApiResponse<List<AttendanceResponseDto>>>> GetAttendance([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(ApiResponse<List<AttendanceResponseDto>>.Ok(await _service.GetAttendancesAsync(GetChamberId(), from, to)));

    [HttpPost("attendance")]
    public async Task<ActionResult<ApiResponse<AttendanceResponseDto>>> CreateAttendance([FromBody] CreateAttendanceDto dto)
        => Ok(ApiResponse<AttendanceResponseDto>.Ok(await _service.CreateAttendanceAsync(dto)));
}
