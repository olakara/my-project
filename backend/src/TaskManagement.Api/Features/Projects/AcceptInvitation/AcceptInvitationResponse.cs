namespace TaskManagement.Api.Features.Projects.AcceptInvitation;

public class AcceptInvitationResponse
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
