using SalesApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.RegularExpressions;

namespace SalesApi.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
               .HasColumnType("uuid")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.SaleNumber)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(s => s.SaleNumber)
            .IsUnique();

        builder.Property(s => s.Date)
               .IsRequired();

        builder.Property(s => s.TotalAmount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(s => s.Customer, customer =>
        {
            customer.Property(c => c.Id)
                .HasColumnName("CustomerId")
                .IsRequired();

            customer.Property(c => c.Name)
                .HasColumnName("CustomerName")
                .HasMaxLength(150)
                .IsRequired();
        });

        builder.OwnsOne(s => s.Branch, branch =>
        {
            branch.Property(b => b.Id)
                .HasColumnName("BranchId")
                .IsRequired();

            branch.Property(b => b.Name)
                .HasColumnName("BranchName")
                .HasMaxLength(150)
                .IsRequired();

            branch.Property(b => b.Code)
                .HasColumnName("BranchCode")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey("SaleId")
            .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
    }
}
