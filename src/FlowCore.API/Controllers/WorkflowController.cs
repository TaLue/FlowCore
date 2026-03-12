using FlowCore.Application.DTOs.Workflow;
using FlowCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCore.API.Controllers;

[ApiController]
[Route("api/workflows")]
[Authorize(Roles = "Admin")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    public WorkflowController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _workflowService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _workflowService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowDto dto)
    {
        var result = await _workflowService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateWorkflowDto dto)
    {
        try
        {
            var result = await _workflowService.UpdateAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
