using Microsoft.EntityFrameworkCore;
using VERA.Registry.Models;

namespace VERA.Registry.Data
{
    // This class controls how the Registry tables are stored in the database
    public class RegistryDbContext : DbContext
    {
        // Pass the database settings into the Registry database context
        public RegistryDbContext(DbContextOptions<RegistryDbContext> options)
            : base(options)
        {
        }

        // Table for uploaded purchase order PDF documents
        public DbSet<PurchaseOrderDocument> PurchaseOrderDocuments { get; set; }

        // Table for businesses or organisations that issue purchase orders
        public DbSet<RegistryIssuer> RegistryIssuers { get; set; }

        // Table for purchase orders registered in the VERA Registry
        public DbSet<RegisteredPurchaseOrder> RegisteredPurchaseOrders { get; set; }

        // Table for financing claims linked to registered purchase orders
        public DbSet<FinancingClaim> FinancingClaims { get; set; }


        // Configure relationships, indexes and database rules
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Keep the normal Entity Framework setup
            base.OnModelCreating(modelBuilder);


            // -----------------------------
            // REGISTRY ISSUER RULES
            // -----------------------------

            // A CIPC registration number can only belong to one issuer
            modelBuilder.Entity<RegistryIssuer>()
                .HasIndex(i => i.CIPCRegistrationNumber)
                .IsUnique();

            // Each issuer must have a unique VERA issuer ID
            modelBuilder.Entity<RegistryIssuer>()
                .HasIndex(i => i.VeraIssuerId)
                .IsUnique();


            // -----------------------------
            // PURCHASE ORDER RULES
            // -----------------------------

            // Each registered purchase order must have a unique VERA PO ID
            modelBuilder.Entity<RegisteredPurchaseOrder>()
                .HasIndex(p => p.VeraPOId)
                .IsUnique();

            // The same issuer cannot register the same PO number twice
            modelBuilder.Entity<RegisteredPurchaseOrder>()
                .HasIndex(p => new
                {
                    p.RegistryIssuerId,
                    p.PONumber
                })
                .IsUnique();


            // -----------------------------
            // ISSUER AND PO RELATIONSHIP
            // -----------------------------

            // One issuer can have many registered purchase orders
            modelBuilder.Entity<RegisteredPurchaseOrder>()
                .HasOne(p => p.RegistryIssuer)
                .WithMany(i => i.PurchaseOrders)
                .HasForeignKey(p => p.RegistryIssuerId);


            // -----------------------------
            // FINANCING CLAIM RELATIONSHIP
            // -----------------------------

            // One purchase order can have financing claims linked to it
            modelBuilder.Entity<FinancingClaim>()
                .HasOne(f => f.RegisteredPurchaseOrder)
                .WithMany(p => p.FinancingClaims)
                .HasForeignKey(f => f.RegisteredPurchaseOrderId);


            // -----------------------------
            // PURCHASE ORDER DOCUMENT RULES
            // -----------------------------

            // One purchase order can have multiple uploaded document versions
            modelBuilder.Entity<PurchaseOrderDocument>()
                .HasOne(d => d.RegisteredPurchaseOrder)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.RegisteredPurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create an index on the document hash
            // This helps us quickly check if the same PDF was uploaded before
            modelBuilder.Entity<PurchaseOrderDocument>()
                .HasIndex(d => d.DocumentHash);

            // Store extracted PO amounts with 2 decimal places
            // Example: R125000.50
            modelBuilder.Entity<PurchaseOrderDocument>()
                .Property(d => d.ExtractedAmount)
                .HasPrecision(18, 2);
        }
    }
}