using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DashboardEntities
{
    public class AirportWeatherDto
    {
        public string AirportCode { get; set; }
        public double? Temperature { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string? LastRefreshTime { get; set; }
    }

    public class AirportWeather
    {
        public int Id { get; set; }

        public string AirportCode { get; set; }

        public string City { get; set; }

        public double? Temperature { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
