using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DashboardEntities
{
    public class NotificationHistoryDto
    {
        public string UserName { get; set; }
        public string FlightNumber { get; set; }
        public string SkipReason { get; set; }
        public bool EmailSent { get; set; }
        public bool SmsSent { get; set; }
        public DateTime LoggedAtUtc { get; set; }
    }
}
