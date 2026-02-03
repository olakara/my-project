using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Api.Domain.Tasks;

namespace TaskManagement.Api.Data.Configurations;

public class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
{
    public void Configure(EntityTypeBuilder<TaskHistory> builder)
    {
        builder.HasKey(th => th.Id);

        builder.Property(th => th.ChangeType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(th => th.OldValue)
            .HasMaxLength(1000);

        builder.Property(th => th.NewValue)
            .HasMaxLength(1000);

        builder.Property(th => th.ChangedTimestamp)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(th => th.Task)
            .WithMany(t => t.History)
            .HasForeignKey(th => th.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(th => th.ChangedByUser)
            .WithMany()
            .HasForeignKey(th => th.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(th => th.TaskId);
        builder.HasIndex(th => th.ChangedBy);
        builder.HasIndex(th => th.ChangedTimestamp);
    }
}
