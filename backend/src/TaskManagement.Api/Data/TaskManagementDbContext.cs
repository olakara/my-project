using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data.Configurations;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Domain.Projects;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;
using TaskManagement.Api.Domain.Notifications;
using TaskManagement.Api.Domain.Tasks;

namespace TaskManagement.Api.Data;

public class TaskManagementDbContext : IdentityDbContext<ApplicationUser>
{
    public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectInvitation> ProjectInvitations => Set<ProjectInvitation>();
    public DbSet<DomainTask> Tasks => Set<DomainTask>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TaskHistory> TaskHistory => Set<TaskHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectMemberConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectInvitationConfiguration());
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
        modelBuilder.ApplyConfiguration(new TaskHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());

        // Configure Identity tables
        modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
    }
}
