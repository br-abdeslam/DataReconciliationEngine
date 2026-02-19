using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReconciliationEngine.Domain.Entities
{
    public class ComparisonResult
    {
        public int Id { get; set; }

        public int RunId { get; set; }
        public int ComparisonConfigId { get; set; }

        public string MatchKeyValue { get; set; } = string.Empty; // ex: Werfcode, CustomerNumber, etc.
        public bool ExistsInSystemA { get; set; }
        public bool ExistsInSystemB { get; set; }

        public string LogicalFieldName { get; set; } = string.Empty;
        public string? ValueSystemA { get; set; }
        public string? ValueSystemB { get; set; }

        public bool IsMatch { get; set; }
        public DateTime ComparedAt { get; set; } = DateTime.UtcNow;
    }
}
