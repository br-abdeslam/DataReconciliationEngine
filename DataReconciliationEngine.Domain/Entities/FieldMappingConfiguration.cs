using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReconciliationEngine.Domain.Entities
{
    public class FieldMappingConfiguration
    {
        public int Id { get; set; }

        public int ComparisonConfigId { get; set; }
        public TableComparisonConfiguration? ComparisonConfig { get; set; }

        public string LogicalFieldName { get; set; } = string.Empty;
        public string SystemA_Column { get; set; } = string.Empty;
        public string SystemB_Column { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
