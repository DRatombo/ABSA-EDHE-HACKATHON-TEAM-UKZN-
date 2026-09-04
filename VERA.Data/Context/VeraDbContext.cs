using Microsoft.EntityFrameworkCore;
using VERA.Models.Entities;

namespace VERA.Data.Context
{
    public class VeraDbContext : DbContext
    {
        public VeraDbContext(DbContextOptions<VeraDbContext> options)
            : base(options)
        {
        }

        public DbSet<Business> Businesses { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<VerificationResult> VerificationResults { get; set; }
        public DbSet<RiskFlag> RiskFlags { get; set; }
        public DbSet<FundingOffer> FundingOffers { get; set; }
        public DbSet<FulfilmentRecord> FulfilmentRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Business 1 --- many Opportunities
            modelBuilder.Entity<Business>()
                .HasMany(b => b.Opportunities)
                .WithOne(o => o.Business)
                .HasForeignKey(o => o.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // Opportunity 1 --- many Verification Results
            modelBuilder.Entity<Opportunity>()
                .HasMany(o => o.VerificationResults)
                .WithOne(v => v.Opportunity)
                .HasForeignKey(v => v.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Opportunity 1 --- many Risk Flags
            modelBuilder.Entity<Opportunity>()
                .HasMany(o => o.RiskFlags)
                .WithOne(r => r.Opportunity)
                .HasForeignKey(r => r.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Opportunity 1 --- many Funding Offers
            modelBuilder.Entity<Opportunity>()
                .HasMany(o => o.FundingOffers)
                .WithOne(f => f.Opportunity)
                .HasForeignKey(f => f.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Opportunity 1 --- 0/1 Fulfilment Record
            modelBuilder.Entity<Opportunity>()
                .HasOne(o => o.FulfilmentRecord)
                .WithOne(f => f.Opportunity)
                .HasForeignKey<FulfilmentRecord>(f => f.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Financial precision for SQL Server
            modelBuilder.Entity<Opportunity>()
                .Property(o => o.POValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Opportunity>()
                .Property(o => o.FulfilmentCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Opportunity>()
                .Property(o => o.SMEContribution)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Opportunity>()
                .Property(o => o.FundingGap)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Opportunity>()
                .Property(o => o.EstimatedFundingCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Opportunity>()
                .Property(o => o.PlatformFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Opportunity>()
                .Property(o => o.RemainingMargin)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Opportunity>()
                .Property(o => o.RemainingMarginPercentage)
                .HasPrecision(8, 2);

            modelBuilder.Entity<FundingOffer>()
                .Property(f => f.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FundingOffer>()
                .Property(f => f.FundingCost)
                .HasPrecision(18, 2);

            // Useful indexes
            modelBuilder.Entity<Opportunity>()
                .HasIndex(o => o.Fingerprint);

            modelBuilder.Entity<Opportunity>()
                .HasIndex(o => o.PONumber);

            modelBuilder.Entity<Business>()
                .HasIndex(b => b.RegistrationNumber);
        }
    }
}