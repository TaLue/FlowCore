using FlowCore.Application.DTOs.Department;
using FlowCore.Domain.Entities;
using FlowCore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var departments = await _repository.FindAsync(
            _ => true,
            include: q => q.Include(d => d.Manager).Include(d => d.Users));

        return Ok(departments.Select(d => new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            ManagerId = d.ManagerId,
            ManagerName = d.Manager?.Username ?? "",
            UserCount = d.Users.Count
        }));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var existing = await _repository.FindAsync(d => d.Name == dto.Name);
        if (existing.Any())
            return Conflict(new { message = $"Department '{dto.Name}' already exists" });

        var dept = new Department
        {
            Name = dto.Name,
            ManagerId = dto.ManagerId
        };

        await _repository.AddAsync(dept);

        // Reload with includes
        var created = await _repository.FindAsync(
            d => d.Id == dept.Id,
            include: q => q.Include(d => d.Manager).Include(d => d.Users));
        var result = created.First();

        return CreatedAtAction(nameof(GetAll), new DepartmentDto
        {
            Id = result.Id,
            Name = result.Name,
            ManagerId = result.ManagerId,
            ManagerName = result.Manager?.Username ?? "",
            UserCount = result.Users.Count
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var depts = await _repository.FindAsync(d => d.Id == id);
        var dept = depts.FirstOrDefault();
        if (dept == null) return NotFound();

        dept.Name = dto.Name;
        dept.ManagerId = dto.ManagerId;

        await _repository.UpdateAsync(dept);

        // Reload with includes
        var updated = await _repository.FindAsync(
            d => d.Id == id,
            include: q => q.Include(d => d.Manager).Include(d => d.Users));
        var result = updated.First();

        return Ok(new DepartmentDto
        {
            Id = result.Id,
            Name = result.Name,
            ManagerId = result.ManagerId,
            ManagerName = result.Manager?.Username ?? "",
            UserCount = result.Users.Count
        });
    }
}
