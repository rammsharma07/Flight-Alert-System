using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Meeting
{
    public class NotificationTab
    {
        public List<MeetingUserDetails> AttendeeDetails { get; set; }
            = new List<MeetingUserDetails>();

        public List<SaveAlertSettingsRequest> MeetingAlertSettings { get; set; }
       = new List<SaveAlertSettingsRequest>();
        public int MeetingID { get; set; }

    }
}
