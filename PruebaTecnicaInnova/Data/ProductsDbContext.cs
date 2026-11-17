
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaInnova.Models;

namespace PruebaTecnicaInnova.Data;

public class ProductsDbContext : DbContext
{
    public ProductsDbContext(
        DbContextOptions<ProductsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);
        });
    }
}
