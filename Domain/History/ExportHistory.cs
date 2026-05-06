using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.History
{
    public class ExportHistory
    {
        public int Id { get; set; }
        public string HistoryType { get; set; }
        public string FileName { get; set; }
        public string Scope { get; set; }              
        public int RecordCount { get; set; }           
        public string MeetingsName { get; set; }       
        public string Filters { get; set; }
        public string FiltersApplied { get; set; }
        public string DataSummary { get; set; }
        public int? UserId { get; set; }    
        public string ExportedBy { get; set; }
        public DateTime ExportedAt { get; set; }
    }
}
