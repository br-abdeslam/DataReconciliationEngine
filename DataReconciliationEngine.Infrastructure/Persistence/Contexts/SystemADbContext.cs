using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReconciliationEngine.Infrastructure.Persistence.Contexts
{
    public class SystemADbContext : DbContext
    {
        public SystemADbContext(DbContextOptions<SystemADbContext> options) : base(options) { }

        // TODO: Ajoute tes DbSet quand tu connais les tables A
        // public DbSet<SiteA> Sites { get; set; } = default!;
    }
}
