namespace Domain.EntityHistory
{
	public class EntityHistoryDto
	{
		public int Id { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? Email { get; set; }
		public string? PhoneNo { get; set; }
		public long? CreatedBy { get; set; }
		public DateTime? CreatedDate { get; set; }
		public long? ModifiedBy { get; set; }
		public DateTime? ModifiedDate { get; set; }
		public string? Message { get; set; }

		public int TotalCount { get; set; }
	}

	public class EntityHistoryAuditDto
	{
		public int? MeetingId { get; set; }
		public int? AttendeeId { get; set; }
		public string? Module { get; set; }
		public string? SubModule { get; set; }
		public string? EntityAffected { get; set; }
		public string? Action { get; set; }
		public long? User { get; set; }
		public DateTime? CreatedDate { get; set; }
		public string? Description { get; set; }

	}
	public class EntityFieldHistoryChangeDto
	{
		public int EntityHistoryId { get; set; }
		public string Module { get; set; }
		public string? SubModule { get; set; }
		public string? EntityAffected { get; set; }
		public DateTime? CreatedDate { get; set; }
		public long? User { get; set; }
		public string? UserName { get; set; }
		public string FieldName { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }

		public bool HasAlertSetting { get; set; }
	}

	public class EntityHistoryGridDto
	{
		public int Id { get; set; }
		public int MeetingId { get; set; }
		public int? AttendeeId { get; set; }
		public string Module { get; set; }
		public string SubModule { get; set; }
		public string? EntityAffected { get; set; }
		public string Action { get; set; }
		public string ChangesSummary { get; set; }
		public string User { get; set; }
		public string RecordName { get; set; }
		public DateTime DateTime { get; set; }
		public string Description { get; set; }
		public int TotalCount { get; set; }
		public bool HasAlertSetting { get; set; }
	}

	public class AlertSettingHistoryAuditDto
	{
		public int? MeetingId { get; set; }          
		public int? AttendeeId { get; set; }         
		public string? AttendeeType { get; set; }  

		public string Module { get; set; }
		public string? SubModule { get; set; }       

		public string? EntityAffected { get; set; }  
		public string? Action { get; set; }        

		public long? User { get; set; }
		public DateTime? CreatedDate { get; set; }

		public string? Description { get; set; }
	}

	public class AlertSettingFieldHistoryChangeDto
	{
		public int EntityHistoryId { get; set; }

		public string Module { get; set; }
		public string? SubModule { get; set; }

		public string? EntityAffected { get; set; }
		public DateTime? CreatedDate { get; set; }

		public long? User { get; set; }
		public string? UserName { get; set; }

		public string FieldName { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
	}
	public class MeetingLookupDto
	{
		public int Id { get; set; }
		public string MeetingName { get; set; }
	}




}
