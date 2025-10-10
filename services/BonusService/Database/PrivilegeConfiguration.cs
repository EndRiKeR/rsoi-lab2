using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RsoiLab2.Services.Tickets.Database.Models;

namespace Tickets.Database;

public class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.HasKey(e => e.Id);
            
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(80)
            .HasConversion<string>();

        builder.Property(e => e.Balance)
            .IsRequired(false); // NULLABLE

        builder.HasIndex(e => e.Username)
            .IsUnique();
    }
}
