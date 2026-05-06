using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Meeting
{
    public class AlertSettingsModel
    {
        public int AttendeeId { get; set; } // Required to link with MeetingUser
        public List<AlertTypeEnum> SelectedSmsAlerts { get; set; } = new();
        public List<AlertTypeEnum> SelectedEmailAlerts { get; set; } = new();
        public Boolean EmailAlerts { get; set; }
        public Boolean SMSAlerts { get; set; }

    }

    public enum AlertTypeEnum
    {
        Cancelled,
        Delayed,
        InAir,
        Early,
        InGate,
        OnTime,
        OutGate,
        Landed,
        Scheduled
    }
}
