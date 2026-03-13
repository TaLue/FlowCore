using FlowCore.Domain.Entities;
using FlowCore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCore.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRepository<Role> _repository;

    public RoleController(IRepository<Role> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _repository.FindAsync(_ => true);
        return Ok(roles.Select(r => new { r.Id, r.Name }));
    }
}
