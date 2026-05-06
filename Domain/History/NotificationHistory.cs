using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.History
{
    public class NotificationHistory
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
        public string? EventType { get; set; }
        public bool? EmailSent { get; set; }
        public bool? SmsSent { get; set; }
        public DateTime? LoggedAtUtc { get; set; }
        public bool? EmailAlert { get; set; }
        public bool? SmsAlert { get; set; }
        public string? EmailStatus { get; set; }
        public string? EmailState { get; set; }
        public string? SmsStatus { get; set; }
        public string? SmsState { get; set; }

        [Column("emailContant")]
        public string? emailContant { get; set; }

        [Column("smsContant")]
        public string? smsContant { get; set; }

        public string? CarrierCode { get; set; }
        public string? CarrierName { get; set; }
        public int? MeetingID { get; set; }
        public string? MeetingName { get; set; }
        public string? Trigger { get; set; }
        public int TotalRecords { get; set; }
    }

    public class NotifQueryDto
    {
        public int? MeetingID { get; set; } = 0;
        public int? AttendeeID { get; set; } = 0;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Dictionary<string, string>? Filters { get; set; }
        public string SearchTerm { get; set; } = "";
        public string SortColumn { get; set; } = "LoggedAtUtc";
        public string SortDirection { get; set; } = "DESC";
        public bool IsExport { get; set; } = false;
        public List<string> MeetingNames { get; set; } = new List<string>();
    }

}
