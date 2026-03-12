using BCrypt.Net;
using FlowCore.Domain.Entities;
using FlowCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlowCore.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        await context.Database.MigrateAsync();

        if (await context.Roles.AnyAsync()) return;

        logger.LogInformation("Seeding database...");

        // Roles
        var roleAdmin = new Role { Name = "Admin" };
        var roleUser = new Role { Name = "User" };
        var roleApprover = new Role { Name = "Approver" };
        context.Roles.AddRange(roleAdmin, roleUser, roleApprover);
        await context.SaveChangesAsync();

        // Department (no manager yet)
        var dept = new Department { Name = "General" };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        // Admin user
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
            Email = "admin@flowcore.local",
            DepartmentId = dept.Id,
            RoleId = roleAdmin.Id,
            IsActive = true
        };
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        // Set department manager
        dept.ManagerId = adminUser.Id;
        await context.SaveChangesAsync();

        // Request Types
        var rtLeave = new RequestType { Name = "Leave Request", Code = "LEAVE", Description = "Annual/sick leave request", IsActive = true };
        var rtPurchase = new RequestType { Name = "Purchase Request", Code = "PURCHASE", Description = "Purchase approval request", IsActive = true };
        context.RequestTypes.AddRange(rtLeave, rtPurchase);
        await context.SaveChangesAsync();

        // Default Workflow for Leave Request
        var workflow = new Workflow
        {
            RequestTypeId = rtLeave.Id,
            Name = "Leave Approval Workflow",
            IsActive = true,
            Steps = new List<WorkflowStep>
            {
                new() { StepOrder = 1, StepName = "Manager Approval", ApproverType = ApproverType.DepartmentManager, ApproverValue = "" },
                new() { StepOrder = 2, StepName = "HR Approval", ApproverType = ApproverType.Role, ApproverValue = "Approver" }
            }
        };
        context.Workflows.Add(workflow);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded successfully.");
    }
}
