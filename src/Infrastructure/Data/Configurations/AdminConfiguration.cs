using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using src.Core.Entities;

namespace src.Infrastructure.Data.Configurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.HasIndex(a => a.Username)
            .IsUnique();

        builder.HasIndex(a => a.Email)
            .IsUnique();
    }
}