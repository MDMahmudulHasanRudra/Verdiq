using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Verdiq.API.Models;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/cause-lists")]
[Authorize]
public class CauseListsController : BaseController
{
    private readonly AppDbContext _db;

    public CauseListsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CauseList>>> Create([FromBody] CauseList causeList)
    {
        causeList.Id = Guid.NewGuid();
        causeList.CreatedAt = DateTime.UtcNow;

        _db.CauseLists.Add(causeList);
        await _db.SaveChangesAsync();

        return CreatedAtAction(null, ApiResponse<CauseList>.Created(causeList));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CauseList>>>> GetAll([FromQuery] DateTime? date)
    {
        var query = _db.CauseLists.AsQueryable();

        if (date.HasValue)
            query = query.Where(c => c.HearingDate.Date == date.Value.Date);

        var causeLists = await query.OrderBy(c => c.HearingDate).ToListAsync();
        return Ok(ApiResponse<IEnumerable<CauseList>>.Ok(causeLists));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var causeList = await _db.CauseLists.FindAsync(id);

        if (causeList is null)
            return NotFound(ApiResponse<object>.Fail("Cause list entry not found"));

        causeList.IsDeleted = true;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Cause list entry deleted successfully"));
    }
}
