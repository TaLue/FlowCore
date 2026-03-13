using System.Security.Claims;
using FlowCore.Application.DTOs.Request;
using FlowCore.Domain.Entities;
using FlowCore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowCore.API.Controllers;

[ApiController]
[Route("api/requests/{requestId}/attachments")]
[Authorize]
public class AttachmentController : ControllerBase
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<AttachmentController> _logger;

    private static readonly HashSet<string> AllowedExtensions =
        [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".gif", ".zip", ".txt"];

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public AttachmentController(
        IRepository<Request> requestRepository,
        IRepository<Attachment> attachmentRepository,
        IConfiguration config,
        ILogger<AttachmentController> logger)
    {
        _requestRepository = requestRepository;
        _attachmentRepository = attachmentRepository;
        _config = config;
        _logger = logger;
    }

    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id : throw new UnauthorizedAccessException("Invalid user identity");
    private string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role) ?? "User";

    [HttpGet]
    public async Task<IActionResult> GetAll(int requestId)
    {
        var request = await GetRequestAsync(requestId);
        if (request == null) return NotFound();
        if (!CanAccess(request)) return Forbid();

        var attachments = await _attachmentRepository.FindAsync(a => a.RequestId == requestId);
        return Ok(attachments.Select(a => new AttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            FileSize = a.FileSize,
            UploadedAt = a.UploadedAt
        }));
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(int requestId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = "File size exceeds 10 MB limit" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { message = $"File type '{ext}' is not allowed" });

        var request = await GetRequestAsync(requestId);
        if (request == null) return NotFound();
        if (!CanAccess(request)) return Forbid();

        var basePath = _config["FileStorage:BasePath"] ?? "/app/uploads";
        var uploadDir = Path.Combine(basePath, requestId.ToString());
        Directory.CreateDirectory(uploadDir);

        var safeFileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadDir, safeFileName);

        try
        {
            await using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file {FileName}", file.FileName);
            return StatusCode(500, new { message = "Failed to save file" });
        }

        var attachment = new Attachment
        {
            RequestId = requestId,
            FileName = file.FileName,
            FilePath = fullPath,
            FileSize = file.Length,
            UploadedBy = CurrentUserId
        };

        await _attachmentRepository.AddAsync(attachment);

        return Ok(new AttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt
        });
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int requestId, int id)
    {
        var request = await GetRequestAsync(requestId);
        if (request == null) return NotFound();
        if (!CanAccess(request)) return Forbid();

        var attachments = await _attachmentRepository.FindAsync(
            a => a.Id == id && a.RequestId == requestId);
        var attachment = attachments.FirstOrDefault();
        if (attachment == null) return NotFound();

        if (!System.IO.File.Exists(attachment.FilePath))
            return NotFound(new { message = "File not found on server" });

        var contentType = GetContentType(attachment.FileName);
        return PhysicalFile(attachment.FilePath, contentType, attachment.FileName);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int requestId, int id)
    {
        var request = await GetRequestAsync(requestId);
        if (request == null) return NotFound();

        // Only uploader or Admin can delete
        var attachments = await _attachmentRepository.FindAsync(
            a => a.Id == id && a.RequestId == requestId);
        var attachment = attachments.FirstOrDefault();
        if (attachment == null) return NotFound();

        if (CurrentUserRole != "Admin" && attachment.UploadedBy != CurrentUserId)
            return Forbid();

        if (System.IO.File.Exists(attachment.FilePath))
            System.IO.File.Delete(attachment.FilePath);

        await _attachmentRepository.DeleteAsync(attachment);
        return NoContent();
    }

    private async Task<Request?> GetRequestAsync(int requestId)
    {
        var results = await _requestRepository.FindAsync(r => r.Id == requestId);
        return results.FirstOrDefault();
    }

    private bool CanAccess(Request request) =>
        CurrentUserRole == "Admin" || request.RequesterId == CurrentUserId;

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
