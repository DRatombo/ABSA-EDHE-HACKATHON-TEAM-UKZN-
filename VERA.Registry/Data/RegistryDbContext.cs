using Microsoft.EntityFrameworkCore;
using VERA.Registry.Models;

namespace VERA.Registry.Data
{
    public class RegistryDbContext : DbContext
    {
        public RegistryDbContext(DbContextOptions<RegistryDbContext> options)
            : base(options)
        {
        }
        public DbSet<PurchaseOrderDocument> PurchaseOrderDocuments { get; set; }

        public DbSet<RegistryIssuer> RegistryIssuers { get; set; }

        public DbSet<RegisteredPurchaseOrder> RegisteredPurchaseOrders { get; set; }

        public DbSet<FinancingClaim> FinancingClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RegistryIssuer>()
                .HasIndex(i => i.CIPCRegistrationNumber)
                .IsUnique();

            modelBuilder.Entity<RegistryIssuer>()
                .HasIndex(i => i.VeraIssuerId)
                .IsUnique();

            modelBuilder.Entity<RegisteredPurchaseOrder>()
                .HasIndex(p => p.VeraPOId)
                .IsUnique();

            modelBuilder.Entity<RegisteredPurchaseOrder>()
                .HasIndex(p => new { p.RegistryIssuerId, p.PONumber })
                .IsUnique();

            modelBuilder.Entity<RegisteredPurchaseOrder>()
                .HasOne(p => p.RegistryIssuer)
                .WithMany(i => i.PurchaseOrders)
                .HasForeignKey(p => p.RegistryIssuerId);

            modelBuilder.Entity<FinancingClaim>()
                .HasOne(f => f.RegisteredPurchaseOrder)
                .WithMany(p => p.FinancingClaims)
                .HasForeignKey(f => f.RegisteredPurchaseOrderId);

            modelBuilder.Entity<PurchaseOrderDocument>()
    .HasOne(d => d.RegisteredPurchaseOrder)
    .WithMany(p => p.Documents)
    .HasForeignKey(d => d.RegisteredPurchaseOrderId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrderDocument>()
                .HasIndex(d => d.DocumentHash);
        }
    }
}