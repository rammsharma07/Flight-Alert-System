using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DashboardEntities
{
    public class NotificationAlertSummaryDto
    {
        public int MeetingId { get; set; }
      
        public List<string> SmsStates { get; set; }
        public List<string> SmsStatuses { get; set; }

        public List<string> EmailStates { get; set; }
        public List<string> EmailStatuses { get; set; }
    }

    public class NotificationAlertSettingEntity
    {
        public int MeetingId { get; set; }
        public int AttendeeId { get; set; }
        public string HcpType { get; set; }
        public bool EmailAlert { get; set; }
        public bool SmsAlert { get; set; }
        public string EmailState { get; set; }
        public string EmailStatus { get; set; }
        public string SmsState { get; set; }
        public string SmsStatus { get; set; }
    }
}
