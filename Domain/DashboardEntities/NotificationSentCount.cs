using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DashboardEntities
{
    public class NotificationChannelCountCount
    {
        public int MeetingId { get; set; }
        public int TotalEmails { get; set; }
        public int EmailSent { get; set; }
        public int EmailFailed { get; set; }

        public int TotalSms { get; set; }
        public int SmsSent { get; set; }
        public int SmsFailed { get; set; }
    }
}
