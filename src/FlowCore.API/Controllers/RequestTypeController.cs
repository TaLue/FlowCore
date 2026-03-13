using FlowCore.Domain.Entities;
using FlowCore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCore.API.Controllers;

[ApiController]
[Route("api/request-types")]
[Authorize]
public class RequestTypeController : ControllerBase
{
    private readonly IRepository<RequestType> _repository;

    public RequestTypeController(IRepository<RequestType> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var types = await _repository.FindAsync(rt => rt.IsActive);
        return Ok(types.Select(rt => new { rt.Id, rt.Name, rt.Code }));
    }
}
