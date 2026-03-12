using System.Security.Claims;
using FlowCore.Application.DTOs.Approval;
using FlowCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCore.API.Controllers;

[ApiController]
[Route("api/approvals")]
[Authorize]
public class ApprovalController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _approvalService.GetPendingAsync(CurrentUserId);
        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApprovalActionDto dto)
    {
        try
        {
            await _approvalService.ApproveAsync(id, dto, CurrentUserId);
            return Ok(new { message = "Approved successfully" });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException e) { return Forbid(e.Message); }
        catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); }
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalActionDto dto)
    {
        try
        {
            await _approvalService.RejectAsync(id, dto, CurrentUserId);
            return Ok(new { message = "Rejected successfully" });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException e) { return Forbid(e.Message); }
        catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); }
    }

    [HttpPost("{id}/return")]
    public async Task<IActionResult> Return(int id, [FromBody] ApprovalActionDto dto)
    {
        try
        {
            await _approvalService.ReturnAsync(id, dto, CurrentUserId);
            return Ok(new { message = "Returned successfully" });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException e) { return Forbid(e.Message); }
        catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); }
    }
}
