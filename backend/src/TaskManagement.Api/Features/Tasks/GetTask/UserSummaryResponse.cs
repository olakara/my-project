namespace TaskManagement.Api.Features.Tasks.GetTask;

public class UserSummaryResponse
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}
