using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DashboardEntities
{
    public class AttendeeOverviewDto
    {
        public int TotalAttendees { get; set; }
        public int ArrivalsRemaining { get; set; }
        public int Delayed { get; set; }
        public int DeparturesRemaining { get; set; }
    }

    public class FlightOverviewDto
    {
        public int Landed { get; set; }
        public int InAir { get; set; }
        public int Scheduled { get; set; }
        public int Cancelled { get; set; }
    }


    public class AttendeeTypeDto
    {
        public string AttendeeType { get; set; }
        public int Count { get; set; }
    }

    public class PassengerFlightDetailsDto
    {
        public string AttendeeName { get; set; }
        public string AttendeeType { get; set; }
        public string CarrierCode { get; set; }
        public string FlightNumber { get; set; }
        public string CarrierName { get; set; }
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public string State { get; set; }
        public bool IsConnecting { get; set; }
        public string LegText { get; set; }
        public string NotificationLevel { get; set; }
        public string FlightGroupType { get; set; } 

    }

    public class DashboardResponseDto
    {
        public AttendeeOverviewDto AttendeeOverview { get; set; }
        public FlightOverviewDto FlightOverview { get; set; }
        public List<AttendeeTypeDto> AttendeeType { get; set; }
        public List<PassengerFlightDetailsDto> PassengerFlightDetails { get; set; }
        public List<AirportWeatherDto> AirportWeather { get; set; }
        public int IncompleteCount { get; set; }

        public List<AirportTrafficResponse> AirportTraffic { get; set; }

        public int TimeMismatchCount { get; set; }
    }

    public class DashboardOverviewDto
    {
        public AttendeeOverviewDto ArrivalAttendee { get; set; }
        public AttendeeOverviewDto DepartureAttendee { get; set; }

        public FlightOverviewDto ArrivalFlights { get; set; }
        public FlightOverviewDto DepartureFlights { get; set; }

        public int IncompleteCount { get; set; }
        public int TimeMismatchCount { get; set; }
    }

    public class PassengerFlightResponseDto
    {
        public List<PassengerFlightDetailsDto> PassengerFlightDetails { get; set; }    
    }

    public class AirportTrafficResponseDto
    {
        public List<AirportTrafficResponse> AirportTraffic { get; set; }
        public string? LastRefreshTime { get; set; }
    }

    public class NotificationAlertSetting
    {
        public int? AttendeeId { get; set; }
        public string HcpType { get; set; }
    }

    public class DashboardOverviewCountsDto
    {
        public string Type { get; set; }

        public int Total { get; set; }
        public int Remaining { get; set; }
        public int Arrived { get; set; }
        public int Delayed { get; set; }

        public int Landed { get; set; }
        public int InAir { get; set; }
        public int Scheduled { get; set; }
        public int Cancelled { get; set; }
    }
}
