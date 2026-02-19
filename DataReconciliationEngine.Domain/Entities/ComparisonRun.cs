using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReconciliationEngine.Domain.Entities
{
    public class ComparisonRun
    {
        public int Id { get; set; }
        public int ComparisonConfigId { get; set; }

        public DateTime RunDate { get; set; } = DateTime.UtcNow;

        public int TotalRecords { get; set; }
        public int TotalMismatches { get; set; }
        public int TotalMissingInA { get; set; }
        public int TotalMissingInB { get; set; }
    }
}
