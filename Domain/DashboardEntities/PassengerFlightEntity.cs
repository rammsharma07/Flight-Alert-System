using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DashboardEntities
{
    public class PassengerFlightEntity
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }  

        public string AttendeeType { get; set; }

        public string CarrierCode { get; set; }
        public string DepartureFlightNumber { get; set; }
        public string CarrierName { get; set; }

        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }

        public DateTime? DepartureDateTime { get; set; }
        public DateTime? ArrivalDateTime { get; set; }

        public string State { get; set; }

        public bool Connecting { get; set; }
        public string FlightType { get; set; }
    }

}
