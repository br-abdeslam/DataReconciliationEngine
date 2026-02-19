using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataReconciliationEngine.Infrastructure.Persistence.Contexts;

/// <summary>
/// Used only by EF Core CLI/PMC tools (Add-Migration, Update-Database).
/// Not used at runtime — Program.cs registers the real DbContext with DI.
/// </summary>
public class ReconciliationDbContextFactory : IDesignTimeDbContextFactory<ReconciliationDbContext>
{
    public ReconciliationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ReconciliationDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ReconciliationLocalDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;")
            .Options;

        return new ReconciliationDbContext(options);
    }
}