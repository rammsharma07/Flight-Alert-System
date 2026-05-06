using Domain.Meeting;
using Domain.Response.EventHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Response
{
	public static class EventHubResponseMapper
	{
		public static FlightStatusDetails MapToUserFlightStatus(EventHubResponse eventHubResponse)
		{
			return new FlightStatusDetails
			{
				DocumentType = eventHubResponse.DocumentType,
				FlightType = eventHubResponse.FlightType,
				State = eventHubResponse.State,
				ServiceType = eventHubResponse.ServiceType,
				FlightNumber = eventHubResponse.FlightNumber,
				CarrierCodeIata = eventHubResponse.CarrierCode?.Iata,
				CarrierCodeIcao = eventHubResponse.CarrierCode?.Icao,
				EquipmentAircraftTypeIata = eventHubResponse.Equipment?.AircraftType?.Iata,
				EquipmentAircraftTypeIcao = eventHubResponse.Equipment?.AircraftType?.Icao,
				EquipmentAircraftRegistrationNumber = eventHubResponse.Equipment?.AircraftRegistrationNumber,
				OriginationDateLocal = eventHubResponse.OriginationDate?.Local ?? DateTime.MinValue,
				DepartureTimesScheduledLocal = eventHubResponse.Departure?.Times?.Scheduled?.Local,
				DepartureTimesScheduledUtc = eventHubResponse.Departure?.Times?.Scheduled?.Utc,
				DepartureTimesEstimatedOutGateTimeliness = eventHubResponse.Departure?.Times?.Estimated?.OutGateTimeliness,
				DepartureTimesEstimatedOutGateVariation = eventHubResponse.Departure?.Times?.Estimated?.OutGateVariation ?? TimeSpan.Zero,
				DepartureTimesEstimatedOutGateLocal = eventHubResponse.Departure?.Times?.Estimated?.OutGateLocal,
				DepartureTimesEstimatedOutGateUtc = eventHubResponse.Departure?.Times?.Estimated?.OutGateUtc,
				DepartureTimesActualOffGroundLocal = eventHubResponse.Departure?.Times?.Actual?.OffGroundLocal,
				DepartureTimesActualOffGroundUtc = eventHubResponse.Departure?.Times?.Actual?.OffGroundUtc,
				DepartureAirportIata = eventHubResponse.Departure?.Airport?.Iata,
				DepartureAirportIcao = eventHubResponse.Departure?.Airport?.Icao,
				DepartureTerminal = eventHubResponse.Departure?.Terminal,
				DepartureGate = eventHubResponse.Departure?.Gate,
				ArrivalTimesScheduledLocal = eventHubResponse.Arrival?.Times?.Scheduled?.Local,
				ArrivalTimesScheduledUtc = eventHubResponse.Arrival?.Times?.Scheduled?.Utc,
				ArrivalTimesEstimatedInGateTimeliness = eventHubResponse.Arrival?.Times?.Estimated?.OutGateTimeliness,
				ArrivalTimesEstimatedInGateVariation = eventHubResponse.Arrival?.Times?.Estimated?.OutGateVariation ?? TimeSpan.Zero,
				ArrivalTimesEstimatedInGateLocal = eventHubResponse.Arrival?.Times?.Estimated?.OutGateLocal ,
				ArrivalTimesEstimatedInGateUtc = eventHubResponse.Arrival?.Times?.Estimated?.OutGateUtc,
				ArrivalTimesActualOnGroundLocal = eventHubResponse.Arrival?.Times?.Actual?.OffGroundLocal,
				ArrivalTimesActualOnGroundUtc = eventHubResponse.Arrival?.Times?.Actual?.OffGroundUtc,
				ArrivalAirportIata = eventHubResponse.Arrival?.Airport?.Iata,
				ArrivalAirportIcao = eventHubResponse.Arrival?.Airport?.Icao,
				ArrivalGate = eventHubResponse.Arrival?.Gate,
				ArrivalBaggage = eventHubResponse.Arrival?.Baggage,
				MessageTimestamp = eventHubResponse.MessageTimestamp,
				MessageId = eventHubResponse.MessageId,
				ScheduleInstanceKey = eventHubResponse.ScheduleInstanceKey,
				OperatingInstanceKey = eventHubResponse.OperatingInstanceKey,
				StatusKey = eventHubResponse.StatusKey,
				IsOperating = eventHubResponse.IsOperating,
				AlertId = eventHubResponse.AlertId
			};
		}
	}
}
