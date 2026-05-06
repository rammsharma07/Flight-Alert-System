using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Meeting
{
    public class SaveAlertSettingsRequest
    {
        public List<AlertStateEnum> SmsStates { get; set; } = new();
        public List<AlertStatusEnum> SmsStatuses { get; set; } = new();
        public List<AlertStateEnum> EmailStates { get; set; } = new();
        public List<AlertStatusEnum> EmailStatuses { get; set; } = new();

        public int MeetingId { get; set; }
        public int AttendeeId { get; set; }
        public bool EmailAlert { get; set; }
        public bool SmsAlert { get; set; }
        public string EntityType { get; set; }
        public string EntityIds { get; set; }
        public string EntityName { get; set; }
    }

	public class AlertSettingOldValueDto
	{
		public bool EmailAlert { get; set; }
		public bool SmsAlert { get; set; }

		public List<AlertStateEnum> EmailStates { get; set; } = new();
		public List<AlertStatusEnum> EmailStatuses { get; set; } = new();
		public List<AlertStateEnum> SmsStates { get; set; } = new();
		public List<AlertStatusEnum> SmsStatuses { get; set; } = new();
	}

	public enum AlertStateEnum : int
    {
        Canceled = 1,
        InAir = 2,
        InGate = 3,
        OutGate = 4,
        Landed = 5,
        Scheduled = 6
    }

    public enum AlertStatusEnum : int
    {
        Delayed = 1,
        Early = 2,
        OnTime = 3
    }
}
