using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Response.EventHub
{
	public class EventHubResponse
	{
		public string DocumentType { get; set; } // FlightStatus
		public string FlightType { get; set; } // Scheduled
		public string State { get; set; } // Landed
		public string ServiceType { get; set; } // J
		public string FlightNumber { get; set; } // 815

		public CarrierCode CarrierCode { get; set; }

		public Equipment Equipment { get; set; }

		public OriginationDate OriginationDate { get; set; }

		public Departure Departure { get; set; }

		public Arrival Arrival { get; set; }

		public string MessageTimestamp { get; set; } // 1720444050460
		public string MessageId { get; set; } // 2fc9acaa-396e-4ef1-803e-b1fac1cfad6c
		public string ScheduleInstanceKey { get; set; } // cbf326d58bda990b452e227c7a20ea09277d0ebcc579e3913486d2ce91b05b85
		public string OperatingInstanceKey { get; set; } // cbf326d58bda990b452e227c7a20ea09277d0ebcc579e3913486d2ce91b05b85
		public string StatusKey { get; set; } // 5f48f782f1b8fa3a8f8b0b075883bbdda11334836b3d3c849428cdbe7996e260

		public bool IsOperating { get; set; } // true
		public string AlertId { get; set; } // 90f4b0d0-afa6-462f-978a-0192e29cce35
	}

	public class CarrierCode
	{
		public string Iata { get; set; } // AI
		public string Icao { get; set; } // AIC
	}

	public class Equipment
	{
		public AircraftType AircraftType { get; set; }
		public string AircraftRegistrationNumber { get; set; } // VTRTZ
	}

	public class AircraftType
	{
		public string Iata { get; set; } // 32N
		public string Icao { get; set; } // A20N
	}

	public class OriginationDate
	{
		public DateTime Local { get; set; } // 2024-07-08T00:00:00
	}

	public class Departure
	{
		public Times Times { get; set; }
		public Airport Airport { get; set; }
		public string Terminal { get; set; } // 3
		public string Gate { get; set; } // 36
	}

	public class Times
	{
		public Scheduled Scheduled { get; set; }
		public Estimated Estimated { get; set; }
		public Actual Actual { get; set; }
	}

	public class Scheduled
	{
		public string Local { get; set; } // 2024-07-08T15:45:00
		public string Utc { get; set; } // 2024-07-08T10:15:00
	}

	public class Estimated
	{
		public string OutGateTimeliness { get; set; } // OnTime
		public TimeSpan OutGateVariation { get; set; } // 00:00:00
		public string OutGateLocal { get; set; } // 2024-07-08T15:45:00+05:30
		public string OutGateUtc { get; set; } // 2024-07-08T10:15:00+00:00
	}

	public class Actual
	{
		public string OffGroundLocal { get; set; } // 2024-07-08T16:14:00+05:30
		public string OffGroundUtc { get; set; } // 2024-07-08T10:44:00+00:00
	}

	public class Airport
	{
		public string Iata { get; set; } // DEL
		public string Icao { get; set; } // VIDP
	}

	public class Arrival
	{
		public Times Times { get; set; }
		public Airport Airport { get; set; }
		public string Gate { get; set; } // D12
		public string Baggage { get; set; } // 7A
	}

	public class ArivalTimes
	{
		public Scheduled Scheduled { get; set; }
		public Estimated Estimated { get; set; }
		public Actual Actual { get; set; }
	}

	public class ArivalScheduled
	{
		public string Local { get; set; } // 2024-07-08T18:40:00
		public string Utc { get; set; } // 2024-07-08T13:10:00
	}

	public class ArivalEstimated
	{
		public string InGateTimeliness { get; set; } // Delayed
		public TimeSpan InGateVariation { get; set; } // 00:04:00
		public DateTime InGateLocal { get; set; } // 2024-07-08T18:44:00+05:30
		public DateTime InGateUtc { get; set; } // 2024-07-08T13:14:00+00:00
	}

	public class ActualFlightStatus
	{
		public DateTime OnGroundLocal { get; set; } // 2024-07-08T18:35:00+05:30
		public DateTime OnGroundUtc { get; set; } // 2024-07-08T13:05:00+00:00
	}
}
