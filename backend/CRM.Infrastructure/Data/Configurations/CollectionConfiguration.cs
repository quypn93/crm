using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(255);
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.HasIndex(c => c.Name).IsUnique();
    }
}

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class ProductFormConfiguration : IEntityTypeConfiguration<ProductForm>
{
    public void Configure(EntityTypeBuilder<ProductForm> builder)
    {
        builder.ToTable("ProductForms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        builder.ToTable("ProductSpecifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class CollectionMaterialConfiguration : IEntityTypeConfiguration<CollectionMaterial>
{
    public void Configure(EntityTypeBuilder<CollectionMaterial> builder)
    {
        builder.ToTable("CollectionMaterials");
        builder.HasKey(x => new { x.CollectionId, x.MaterialId });
        builder.HasOne(x => x.Collection).WithMany(c => c.Materials).HasForeignKey(x => x.CollectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Material).WithMany().HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CollectionColorConfiguration : IEntityTypeConfiguration<CollectionColor>
{
    public void Configure(EntityTypeBuilder<CollectionColor> builder)
    {
        builder.ToTable("CollectionColors");
        builder.HasKey(x => new { x.CollectionId, x.ColorFabricId });
        builder.HasOne(x => x.Collection).WithMany(c => c.Colors).HasForeignKey(x => x.CollectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ColorFabric).WithMany().HasForeignKey(x => x.ColorFabricId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CollectionFormConfiguration : IEntityTypeConfiguration<CollectionForm>
{
    public void Configure(EntityTypeBuilder<CollectionForm> builder)
    {
        builder.ToTable("CollectionForms");
        builder.HasKey(x => new { x.CollectionId, x.ProductFormId });
        builder.HasOne(x => x.Collection).WithMany(c => c.Forms).HasForeignKey(x => x.CollectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductForm).WithMany().HasForeignKey(x => x.ProductFormId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CollectionSpecificationConfiguration : IEntityTypeConfiguration<CollectionSpecification>
{
    public void Configure(EntityTypeBuilder<CollectionSpecification> builder)
    {
        builder.ToTable("CollectionSpecifications");
        builder.HasKey(x => new { x.CollectionId, x.ProductSpecificationId });
        builder.HasOne(x => x.Collection).WithMany(c => c.Specifications).HasForeignKey(x => x.CollectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductSpecification).WithMany().HasForeignKey(x => x.ProductSpecificationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductionDaysOptionConfiguration : IEntityTypeConfiguration<ProductionDaysOption>
{
    public void Configure(EntityTypeBuilder<ProductionDaysOption> builder)
    {
        builder.ToTable("ProductionDaysOptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
    }
}

public class DepositTransactionConfiguration : IEntityTypeConfiguration<DepositTransaction>
{
    public void Configure(EntityTypeBuilder<DepositTransaction> builder)
    {
        builder.ToTable("DepositTransactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(100);
        builder.Property(x => x.BankName).HasMaxLength(100);
        builder.Property(x => x.AccountNumber).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Source).HasMaxLength(20);
        builder.Property(x => x.ExternalId).HasColumnName("CassoId").HasMaxLength(100);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.ExternalId).IsUnique().HasFilter("[CassoId] IS NOT NULL");
        builder.HasOne(x => x.MatchedOrder)
            .WithMany()
            .HasForeignKey(x => x.MatchedOrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
