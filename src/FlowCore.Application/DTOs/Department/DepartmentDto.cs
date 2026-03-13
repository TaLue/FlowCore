namespace FlowCore.Application.DTOs.Department;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}
