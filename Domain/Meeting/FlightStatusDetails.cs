using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Base;

namespace Domain.Meeting
{
	[Table("FlightStatusDetails")]
    public class FlightStatusDetails: BaseModel
    {
		[ForeignKey(nameof(MeetingUser))]
		public int MeetingUserID { get; set; }
		public virtual MeetingUser? MeetingUsers { get; set; }
		public string DocumentType { get; set; }
		public string FlightType { get; set; }
		public string State { get; set; }
		public string ServiceType { get; set; }
		public string FlightNumber { get; set; }
		public string CarrierCodeIata { get; set; }
		public string CarrierCodeIcao { get; set; }
		public string EquipmentAircraftTypeIata { get; set; }
		public string EquipmentAircraftTypeIcao { get; set; }
		public string EquipmentAircraftRegistrationNumber { get; set; }
		public DateTime OriginationDateLocal { get; set; }
		public string DepartureTimesScheduledLocal { get; set; }
		public string DepartureTimesScheduledUtc { get; set; }
		public string DepartureTimesEstimatedOutGateTimeliness { get; set; }
		public TimeSpan DepartureTimesEstimatedOutGateVariation { get; set; }
		public string DepartureTimesEstimatedOutGateLocal { get; set; }
		public string DepartureTimesEstimatedOutGateUtc { get; set; }
		public string DepartureTimesActualOffGroundLocal { get; set; }
		public string DepartureTimesActualOffGroundUtc { get; set; }
		public string DepartureAirportIata { get; set; }
		public string DepartureAirportIcao { get; set; }
		public string DepartureTerminal { get; set; }
		public string DepartureGate { get; set; }
		public string ArrivalTimesScheduledLocal { get; set; }
		public string ArrivalTimesScheduledUtc { get; set; }
		public string ArrivalTimesEstimatedInGateTimeliness { get; set; }
		public TimeSpan ArrivalTimesEstimatedInGateVariation { get; set; }
		public string ArrivalTimesEstimatedInGateLocal { get; set; }
		public string ArrivalTimesEstimatedInGateUtc { get; set; }
		public string ArrivalTimesActualOnGroundLocal { get; set; }
		public string ArrivalTimesActualOnGroundUtc { get; set; }
		public string ArrivalAirportIata { get; set; }
		public string ArrivalAirportIcao { get; set; }
		public string ArrivalGate { get; set; }
		public string ArrivalBaggage { get; set; }
		public string MessageTimestamp { get; set; }
		public string MessageId { get; set; }
		public string ScheduleInstanceKey { get; set; }
		public string OperatingInstanceKey { get; set; }
		public string StatusKey { get; set; }
		public bool IsOperating { get; set; }
		public string AlertId { get; set; }
	}

}
