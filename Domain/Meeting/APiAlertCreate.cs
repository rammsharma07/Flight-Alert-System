using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Meeting
{
    public class AlertStatusChangeFilters
    {
        public bool Baggage { get; set; }
        public bool AircraftRegistrationNumber { get; set; }
        public bool Gates { get; set; }
        public bool Terminal { get; set; }
        public bool Seats { get; set; }
        public bool AircraftType { get; set; }
    }

    public class Alert
    {
        public string Data { get; set; }
        public string AlertId { get; set; }
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AlertType { get; set; }
        public string IataCarrierCode { get; set; }
        public string IcaoCarrierCode { get; set; }
        public int FlightNumber { get; set; }
        public object FromFlight { get; set; }
        public object ToFlight { get; set; }
        public object DepartureAirport { get; set; }
        public object ArrivalAirport { get; set; }
        public bool Active { get; set; }
        public string Content { get; set; }
        public object ArrivalDate { get; set; }
        public string DepartureDate { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime LastUpdatedTimestamp { get; set; }
        public bool Status { get; set; }
        public bool Schedules { get; set; }
        public bool GaFlights { get; set; }
        public bool UnscheduledFlights { get; set; }
        public bool Codeshare { get; set; }
        public object EstimatedArrivalTimeThreshold { get; set; }
        public object EstimatedDepartureTimeThreshold { get; set; }
        public bool ChangeIndicator { get; set; }
        public AlertStatusChangeFilters StatusChangeFilters { get; set; }
    }
    public class ErrorResponse
    {
        public ProblemDetails ProblemDetails { get; set; }
    }
    public class ProblemDetails
    {
        public string Timestamp { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
