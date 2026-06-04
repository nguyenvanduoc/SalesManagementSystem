using System.Collections.Generic;

namespace SalesManagementSystem.Models.ViewModels
{
    public class AuditLogRoot
    {
        public List<AuditLogTable> Tables { get; set; } = new List<AuditLogTable>();
    }

    public class AuditLogTable
    {
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public string Action { get; set; }
        public List<AuditLogChange> Changes { get; set; } = new List<AuditLogChange>();
    }

    public class AuditLogChange
    {
        public string Column { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}
