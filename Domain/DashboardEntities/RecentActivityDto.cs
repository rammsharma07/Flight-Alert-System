using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DashboardEntities
{
    public class RecentActivityDto
    {
        public string Header { get; set; }       
        public string SubText { get; set; }      
        public string Description { get; set; } 
        public DateTime DateTime { get; set; }
    }

    public class ActivityRawDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public int AttendeeId { get; set; }
        public string Action { get; set; }
        public string EntityAffected { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime DateTime { get; set; }
        public string Description { get; set; }

        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
