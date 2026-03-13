using FlowCore.Domain.Entities;
using FlowCore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCore.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IRepository<Department> _repository;

    public DepartmentController(IRepository<Department> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _repository.FindAsync(_ => true);
        return Ok(departments.Select(d => new { d.Id, d.Name }));
    }
}
