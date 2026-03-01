using SalesApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.RegularExpressions;

namespace SalesApi.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
               .HasColumnType("uuid")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.Quantity)
               .IsRequired();

        builder.Property(i => i.UnitPrice)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(i => i.Discount)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.IsCancelled)
            .IsRequired();

        builder.OwnsOne(i => i.Product, product =>
        {
            product.Property(p => p.Id)
                .HasColumnName("ProductId")
                .IsRequired();

            product.Property(p => p.Name)
                .HasColumnName("ProductName")
                .HasMaxLength(150)
                .IsRequired();

            product.Property(p => p.SKU)
                .HasColumnName("ProductSKU")
                .HasMaxLength(50)
                .IsRequired();
        });
    }
}