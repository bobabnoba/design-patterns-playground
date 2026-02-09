using Microsoft.EntityFrameworkCore;

namespace Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderEntity>(b =>
        {
            b.HasKey(o => o.Id);

            b.Property(o => o.CustomerId)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(o => o.Total)
                .HasPrecision(18, 2);

            b.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItemEntity>(b =>
        {
            b.HasKey(i => i.Id);

            b.Property(i => i.Sku)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(i => i.UnitPrice)
                .HasPrecision(18, 2);
        });
    }
}