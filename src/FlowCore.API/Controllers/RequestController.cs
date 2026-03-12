using System.Security.Claims;
using FlowCore.Application.DTOs.Request;
using FlowCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCore.API.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class RequestController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role) ?? "User";

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _requestService.GetAllAsync(CurrentUserId, CurrentUserRole);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _requestService.GetByIdAsync(id, CurrentUserId, CurrentUserRole);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
    {
        var result = await _requestService.CreateAsync(dto, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRequestDto dto)
    {
        try
        {
            var result = await _requestService.UpdateAsync(id, dto, CurrentUserId);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException e) { return Forbid(e.Message); }
        catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); }
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        try
        {
            var result = await _requestService.SubmitAsync(id, CurrentUserId);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException e) { return Forbid(e.Message); }
        catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); }
    }
}
