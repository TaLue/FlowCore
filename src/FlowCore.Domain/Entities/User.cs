namespace FlowCore.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Department Department { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
}
