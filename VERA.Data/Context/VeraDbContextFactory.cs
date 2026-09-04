using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VERA.Data.Context
{
    public class VeraDbContextFactory : IDesignTimeDbContextFactory<VeraDbContext>
    {
        public VeraDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<VeraDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=VERADb;Trusted_Connection=True;TrustServerCertificate=True;"
            );

            return new VeraDbContext(optionsBuilder.Options);
        }
    }
}