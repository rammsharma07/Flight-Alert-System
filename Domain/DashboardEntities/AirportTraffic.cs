using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DashboardEntities
{
    public class AirportRequest
    {
        public string CodeType { get; set; }
        public string Code { get; set; }
    }

    public class AirportTrafficResponse
    {
        public string Code { get; set; }
        public string CodeType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? DelayIndex { get; set; }
        public string City { get; set; }
    }

    public class AirportLatLongData
    {
        public string Icao { get; set; }
        public string Iata { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class AirportTrafficEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string CodeType { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? DelayIndex { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
