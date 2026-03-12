namespace FlowCore.Application.DTOs.Request;

public class CreateRequestDto
{
    public int RequestTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
}
