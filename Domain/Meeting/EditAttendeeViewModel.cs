using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Meeting
{
    public class EditAttendeeModel
    {
        public MeetingUser attendee { get; set; }
        public SaveAlertSettingsRequest Alert { get; set; }
    }
}
