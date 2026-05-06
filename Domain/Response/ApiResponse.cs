namespace Domain.Response
{

    public class ApiResponse
    {
        public Data[] Data { get; set; }
        public Paging Paging { get; set; }
    }

    public class Data
    {
        public Carrier Carrier { get; set; }
        public string ServiceSuffix { get; set; }
        public int FlightNumber { get; set; }
        public string FlightType { get; set; }
        public DepartureInfo Departure { get; set; }
        public ArrivalInfo Arrival { get; set; }
        public int ElapsedTime { get; set; }
        public double CargoTonnage { get; set; }
        public AircraftType AircraftType { get; set; }
        public ServiceType ServiceType { get; set; }
        public SegmentInfo SegmentInfo { get; set; }
        public Codeshare Codeshare { get; set; }
        public string ScheduleInstanceKey { get; set; }
        public string StatusKey { get; set; }
        public StatusDetail[] StatusDetails { get; set; }
    }

    public class Carrier
    {
        public string Iata { get; set; }
        public string Icao { get; set; }
    }

    public class DepartureInfo
    {
        public Airport Airport { get; set; }
        public string Terminal { get; set; }
        public Country Country { get; set; }
        public Date Date { get; set; }
        public Time Time { get; set; }
    }

    public class ArrivalInfo
    {
        public Airport Airport { get; set; }
        public string Terminal { get; set; }
        public Country Country { get; set; }
        public Date Date { get; set; }
        public Time Time { get; set; }
    }

    public class Terminal
    {
        public string Value { get; set; }
    }

    public class Airport
    {
        public string Iata { get; set; }
        public string Icao { get; set; }
    }

    public class Country
    {
        public string Code { get; set; }
    }

    public class Date
    {
        public string Local { get; set; }
        public string Utc { get; set; }
    }

    public class Time
    {
        public string Local { get; set; }
        public string Utc { get; set; }
    }

    public class AircraftType
    {
        public string Iata { get; set; }
    }

    public class ServiceType
    {
        public string Iata { get; set; }
    }

    public class SegmentInfo
    {
        public int NumberOfStops { get; set; }
        public IntermediateAirports IntermediateAirports { get; set; }
    }

    public class IntermediateAirports
    {
        public string[] Iata { get; set; }
    }

    public class Codeshare
    {
        public string[] JointOperationAirlineDesignators { get; set; }
        public MarketingFlight[] MarketingFlights { get; set; }
    }

    public class MarketingFlight
    {
        public string Code { get; set; }
        public string ServiceNumber { get; set; }
        public string Suffix { get; set; }
    }

    public class StatusDetail
    {
        public string State { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Equipment Equipment { get; set; }
        public DepatureFlight Departure { get; set; }
        public ArrivalFlight Arrival { get; set; }
    }
    public class DepatureFlight
    {
        public EstimatedTime estimatedTime { get; set; }
        public actualTime actualtime { get; set; }
    }
    public class actualTime
    {
        public offGround offground { get; set; }
    }
    public class offGround
    {
        public string local { get; set; }
        public string Utc { get; set; }
    }
    public class onGround
    {
        public string local { get; set; }
        public string Utc { get; set; }
    }
    public class ArrivalactualTime
    {
        public onGround onground { get; set; }
    }

    public class ArrivalFlight
    {
        public ArrivalEstimatedTime estimatedTime { get; set; }
        public ArrivalactualTime actualtime { get; set; }
    }
    public class ArrivalEstimatedTime
    {
        public string inGateTimeliness { get; set; }
        public string inGateVariation { get; set; }
        public InGate inGate { get; set; }
    }
    public class InGate
    {
        public string local { get; set; }
        public string Utc { get; set; }
    }

    public class EstimatedTime
    {
        public string outGateTimeliness { get; set; }
        public string outGateVariation { get; set; }
        public OutGate outGate { get; set; }
    }
    public class OutGate
    {
        public string local { get; set; }
        public string Utc { get; set; }
    }

    public class Equipment
    {
        public string AircraftRegistrationNumber { get; set; }
        public ActualAircraftType ActualAircraftType { get; set; }
    }

    public class ActualAircraftType
    {
        public string Iata { get; set; }
        public string Icao { get; set; }
    }

    public class Paging
    {
        public int Limit { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string Next { get; set; }
    }

    public class AlertResponse
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }

}
