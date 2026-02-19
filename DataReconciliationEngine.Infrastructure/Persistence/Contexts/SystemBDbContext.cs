using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReconciliationEngine.Infrastructure.Persistence.Contexts
{
    public class SystemBDbContext : DbContext
    {
        public SystemBDbContext(DbContextOptions<SystemBDbContext> options) : base(options) { }

        // TODO: Ajoute tes DbSet quand tu connais les tables B
        // public DbSet<CompanyB> Companies { get; set; } = default!;
    }
}
