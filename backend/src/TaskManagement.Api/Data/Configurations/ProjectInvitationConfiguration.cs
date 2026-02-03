using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Data.Configurations;

public class ProjectInvitationConfiguration : IEntityTypeConfiguration<ProjectInvitation>
{
    public void Configure(EntityTypeBuilder<ProjectInvitation> builder)
    {
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pi => pi.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(pi => pi.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(pi => pi.CreatedTimestamp)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(pi => pi.Project)
            .WithMany(p => p.Invitations)
            .HasForeignKey(pi => pi.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pi => pi.Inviter)
            .WithMany()
            .HasForeignKey(pi => pi.InviterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(pi => pi.Email);
        builder.HasIndex(pi => pi.ProjectId);
        builder.HasIndex(pi => pi.Status);
    }
}
