using Domain.Base;

namespace Domain.Meeting
{
    public class Meetings : BaseModel
    {
        public string MeetingName { get; set; }
        public string EmailState { get; set; }
        public string EmailStatus { get; set; }

        public string SmsState { get; set; }
        public string SmsStatus { get; set; }
    }

    public class MeetingDetails
    {

        public int Id { get; set; }
        public string MeetingName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public bool EmailAlert { get; set; }
        public bool SmsAlert { get; set; }
        public string EmailState { get; set; }
        public string EmailStatus { get; set; }

        public string SmsState { get; set; }
        public string SmsStatus { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class ActivityMeetingDetails : BaseModel
	{

        public int Id { get; set; }
		public int MeetingId { get; set; }
		public string MeetingName { get; set; }
        public int updatedby { get; set; }
        public DateTime dateTime { get; set; }

        public string Module { get; set; }

        public string Action { get; set; }

		public  string Description { get; set; }

	}
}
