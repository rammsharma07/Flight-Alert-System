using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.FlightState
{
 
        public class FlightStateAlert
        {
            public alert alert { get; set; }
        }

        public class alert
        {
            public Rule rule { get; set; }
            public FlightStatus flightStatus { get; set; }


        }



        public class Rule
        {


            public string id { get; set; }
            public string carrierFsCode { get; set; }
            public string flightNumber { get; set; }
            public string departureAirportFsCode { get; set; }
            public string arrivalAirportFsCode { get; set; }
            public DateTime departure { get; set; }
            public DateTime arrival { get; set; }
            public string name { get; set; }
            public RuleEvents ruleEvents { get; set; }
            public Delivery delivery { get; set; }
        }

        public class RuleEvents
        {
            public List<RuleEvent> ruleEvent { get; set; }
        }

        public class RuleEvent
        {
            public string type { get; set; }
        }

        public class Delivery
        {
            public string format { get; set; }
            public string destination { get; set; }
        }

        public class FlightStatus
        {
            public string status { get; set; }
            public string departureGate { get; set; }
            public string arrivalTerminal { get; set; }
            public string arrivalGate { get; set; }
            public string baggage { get; set; }
            public string scheduledEquipmentIataCode { get; set; }
            public string tailNumber { get; set; }
            public string departureTerminal { get; set; }
            public DateTime? statusUpdatedAtUtc { get; set; }
            public string statusUpdateSource { get; set; }


            public string flightId { get; set; }
            public string carrierFsCode { get; set; }
            public string operatingCarrierFsCode { get; set; }
            public string primaryCarrierFsCode { get; set; }
            public string flightNumber { get; set; }
            public string departureAirportFsCode { get; set; }
            public string arrivalAirportFsCode { get; set; }
            public departureDate departureDate { get; set; }

            public arrivalDate? arrivalDate { get; set; }

            public delays? delays { get; set; }

        public operationalTimes? operationalTimes { get; set; }
    }


        public class departureDate
        {
            public string dateUtc { get; set; }
            public string dateLocal { get; set; }
        }

        public class delays
    {
            public string? departureGateDelayMinutes { get; set; }
            public string? departureRunwayDelayMinutes { get; set; }
            public string? arrivalGateDelayMinutes { get; set; }

            public string? arrivalRunwayDelayMinutes { get; set; }

        };

        public class arrivalDate
        {
            public string dateUtc { get; set; }
            public string dateLocal { get; set; }
        }
        public class FlightStateAlertDto
        {
            public int Id { get; set; }

            // Rule Info
            [JsonPropertyName("rule_id")]
            public string? RuleId { get; set; }

            [JsonPropertyName("rule_name")]
            public string? RuleName { get; set; }

            [JsonPropertyName("rule_event_type")]
            public string? RuleEventType { get; set; }

            [JsonPropertyName("delivery_format")]
            public string? DeliveryFormat { get; set; }

            [JsonPropertyName("delivery_destination")]
            public string? DeliveryDestination { get; set; }

            // Flight Info
            [JsonPropertyName("carrier_fs_code")]
            public string? CarrierFsCode { get; set; }

            [JsonPropertyName("flight_number")]
            public string? FlightNumber { get; set; }

            [JsonPropertyName("departure_airport_fs_code")]
            public string? DepartureAirportFsCode { get; set; }

            [JsonPropertyName("arrival_airport_fs_code")]
            public string? ArrivalAirportFsCode { get; set; }

            [JsonPropertyName("departure")]
            public DateTime? Departure { get; set; }

            [JsonPropertyName("arrival")]
            public DateTime? Arrival { get; set; }

            [JsonPropertyName("flight_id")]
            public string? FlightId { get; set; }

            [JsonPropertyName("operating_carrier_fs_code")]
            public string? OperatingCarrierFsCode { get; set; }

            [JsonPropertyName("primary_carrier_fs_code")]
            public string? PrimaryCarrierFsCode { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }


            [JsonPropertyName("departure_date_utc")]
            public string? DepartureDateUtc { get; set; }



            [JsonPropertyName("arrival_date_utc")]
            public string? ArrivalDateUtc { get; set; }

            public string? departureGateDelayMinutes { get; set; }
            public string? departureRunwayDelayMinutes { get; set; }
            public string? arrivalGateDelayMinutes { get; set; }

            public string? arrivalRunwayDelayMinutes { get; set; }

        public string? scheduledGateDepartureUtc { get; set; }
        public string? actualGateDepartureUtc { get; set; }
        public string? actualGateArrivalUtc { get; set; }
        public string? scheduledGateArrivalUtc { get; set; }
        public string? scheduledGateDepartureLocal{ get; set; }
        public string? actualGateDepartureLocal { get; set; }
        public string? actualGateArrivalLocal { get; set; }
        public string? scheduledGateArrivalLocal { get; set; }


        // Timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        }

    public class operationalTimes {
        public scheduledGateDeparture? scheduledGateDeparture { get; set; }
        public actualGateDeparture? actualGateDeparture { get; set; }
        public actualGateArrival? actualGateArrival { get; set; }
        public scheduledGateArrival? scheduledGateArrival { get; set; }

    }

    public class scheduledGateDeparture
    {
        public string? dateUtc { get; set; }
        public string? dateLocal { get; set; }

    }

    public class actualGateDeparture
    {
        public string? dateUtc { get; set; }
        public string? dateLocal { get; set; }

    }

    public class actualGateArrival {
        public string? dateUtc { get; set; }
        public string? dateLocal { get; set; }

    }

    public class scheduledGateArrival
    {
        public string? dateUtc { get; set; }
        public string? dateLocal { get; set; }

    }

    public class resultResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
    }

