using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReconciliationEngine.Domain.Entities
{
    public class TableComparisonConfiguration
    {
        public int Id { get; set; }
        public string ComparisonName { get; set; } = string.Empty;

        public string SystemA_Table { get; set; } = string.Empty;
        public string SystemB_Table { get; set; } = string.Empty;

        public string MatchColumn_SystemA { get; set; } = string.Empty;
        public string MatchColumn_SystemB { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<FieldMappingConfiguration> FieldMappings { get; set; } = new List<FieldMappingConfiguration>();
    }
}
