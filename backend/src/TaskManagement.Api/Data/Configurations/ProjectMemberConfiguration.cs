using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Data.Configurations;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(pm => pm.JoinedTimestamp)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint - one membership per user per project
        builder.HasIndex(pm => new { pm.UserId, pm.ProjectId })
            .IsUnique();

        // Additional indexes
        builder.HasIndex(pm => pm.ProjectId);
        builder.HasIndex(pm => pm.UserId);
    }
}
