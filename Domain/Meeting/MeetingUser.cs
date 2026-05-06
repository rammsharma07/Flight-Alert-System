using Domain.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Meeting
{
    public class MeetingUser : BaseModel
    {
        [ForeignKey(nameof(Meetings))]
        [DisplayName("Meeting Name")]
        [Required(ErrorMessage = "{0} is required.")]
        public int MeetingID { get; set; }

        [DisplayName("First Name")]
        [Required(ErrorMessage = "{0} is required.")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [Required(ErrorMessage = "{0} is required.")]
        public string LastName { get; set; }

        [DisplayName("Email")]
        [RegularExpression(@"^\S+@\S+\.\S+$", ErrorMessage = "Please enter valid {0}.")]
        [Required(ErrorMessage = "{0} is required.")]
        public string? EmailId { get; set; }

        [DisplayName("Phone Number")]
        [Required(ErrorMessage = "{0} is required.")]
        public string? PhoneNumber { get; set; }


        [DisplayName("Attendee Type")]
        [Required(ErrorMessage = "{0} is required.")]
        public string? AttendeeType { get; set; }

        [DisplayName("Airline")]
        public string? DepartureAirline { get; set; }

        [DisplayName("Arrival Airline")]
        public string? ArrivalAirline { get; set; }

        [DisplayName("Flight Number")]
        [Required(ErrorMessage = "{0} is required.")]
        public string? DepartureFlightNumber { get; set; }

        [DisplayName("Arrival Flight Number")]
        public string? ArrivalFlightNumber { get; set; }

        [DisplayName("Departure Date")]
        public DateTime DepartureDateTime { get; set; }

        [DisplayName("Arrival Date")]
        public DateTime ArrivalDateTime { get; set; }

        [DisplayName("Origin Airport")]
        [Required(ErrorMessage = "{0} is required.")]
        public string OriginAirport { get; set; }

        [DisplayName("Destination Airport")]
        [Required(ErrorMessage = "{0} is required.")]
        public string DestinationAirport { get; set; }

        [DisplayName("Carrier Code")]
        [RegularExpression(@"^[a-zA-Z0-9 .,_]*$", ErrorMessage = "Please enter a valid {0}.")]
        [Required(ErrorMessage = "{0} is required.")]
        public string? CarrierCode { get; set; }

        [DisplayName("Code Type")]
        [RegularExpression(@"^[a-zA-Z0-9 .,_]*$", ErrorMessage = "Please enter a valid {0}.")]
        public string? CodeType { get; set; }

        [DisplayName("Flight Label")]
        [Required(ErrorMessage = "{0} is required.")]
        [RegularExpression(@"^[a-zA-Z0-9 .,_]*$", ErrorMessage = "Please enter a valid {0}.")]
        public string FlightType { get; set; }

        [DisplayName("Departure Time")]
        [Required(ErrorMessage = "{0} is required.")]
        public virtual string DepartureTime { get; set; }

        [DisplayName("Arrival Time")]
        [Required(ErrorMessage = "{0} is required.")]
        public virtual string ArrivalTime { get; set; }
        [DisplayName("Alert Id")]
        public string? AlertId { get; set; }
        public virtual Meetings? Meetings { get; set; }
        public Boolean? EmailSend { get; set; }
        public Boolean? SmsSend { get; set; }
        public string? state { get; set; }
        public string? Status { get; set; }

        [DisplayName("Carrier Name")]
        public string? CarrierName { get; set; }
        public long? LastSequenceNumber { get; set; }  // EventHub sequence number
        public string? LastEventOffset { get; set; } // EventHub offset

        [DisplayName("Last Sync Date")]
        public string? LastSyncDateTime { get; set; }
		public bool Connecting { get; set; }

		public string? EmailMessageId { get; set; }
		public string? EmailStatus { get; set; }

		
		public string? SmsMessageId { get; set; }
		public string? SmsStatus { get; set; }
		

	}

    public class MeetingUserDetails
    {
        public int Id { get; set; }
        public int MeetingID { get; set; }
        public string MeetingName { get; set; }
        public string FullName { get; set; }
        public string EmailId { get; set; }
        public string PhoneNumber { get; set; }
        public string AttendeeType { get; set; }
        public string DepartureAirline { get; set; }
        public string ArrivalAirline { get; set; }
        public string DepartureFlightNumber { get; set; }
        public string ArrivalFlightNumber { get; set; }
        public DateTime? DepartureDateTime { get; set; }
        public DateTime? ArrivalDateTime { get; set; }
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CarrierCode { get; set; }
        public string CodeType { get; set; }
        public string FlightType { get; set; }
        public string FlightStatus { get; set; }
        public string LastSyncDateTime { get; set; }
        public string? LastSyncDateTimeUtc { get; set; }
        public string State { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CarrierName { get; set; }
		public Boolean? Connecting { get; set; }
    }

    public class EventHubUserLog : BaseModel
    {
        public string? AlertId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? FlightNumber { get; set; }
        public DateTime? DepartureDateTime { get; set; }

        public bool? IsSkipped { get; set; }
        public string? SkipReason { get; set; }

        public string? StatusFrom { get; set; }
        public string? StatusTo { get; set; }
        public string? StateFrom { get; set; }
        public string? StateTo { get; set; }

        public bool? EmailSent { get; set; }
        public bool? SmsSent { get; set; }

        public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;
        public bool? Emailalert { get; set; }
        public bool? Smsalert { get; set; }
        public List<string>? EmailStatus { get; set; }
        public List<string>? EmailState { get; set; }
        public List<string>? SmsStatus { get; set; }
        public List<string>? SmsState { get; set; }
        public string? EmailContent { get; set; }
        public string? SmsContent { get; set; }
        public string? CarrierCode { get; set; }
        public string? CarrierName { get; set; }
        public int? MeetingID { get; set; }
        public string? MeetingName { get; set; }
        public string? Trigger { get; set; }

        public string? EmailMessageId { get; set; }
        public string? SmsMessageId { get; set; }


	}

    public class AttendeeSearchRequest
    {
        public int MeetingID { get; set; }
        public string SearchTerm { get; set; } = "";
        public string AttendeeName { get; set; } = "";
        public string AttendeeType { get; set; } = "";
        public string SortBy { get; set; } = "name";
        public string SortOrder { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AttendeeFilterData
    {
        public List<string> Names { get; set; } = new();
        public List<string> Types { get; set; } = new();
    }

	public class PagedResponse<T>
	{
		public List<T> Data { get; set; } = new();
		public int TotalRecords { get; set; }
		public int FilteredRecords { get; set; }
	}
}
