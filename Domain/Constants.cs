using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DashboardEntities;

namespace Domain
{
    public class Constants
    {
        public const string CarrierCode = "AI";
        public const string Content = "status";
        public const string CodeType = "IATA,ICAO";
        public const string Version = "v2";
        public const string status = "OnTime";
        public const string Scheduled = "Scheduled";
        public const string Delayed = "Delayed";
        public const string MessageId = "MessageId";
        public const string InAir = "InAir";
        public const string Landed = "Landed";
        public const string InGate = "InGate";
        public const string NoRecord = "No Takeoff Info";
        public const string Canceled = "Canceled";
        public const string OutGate = "OutGate";
        public const string FlightStatusAlert = "Flight Status Alerts";
        public const string FlightStatusAlertSent = "Flight status alerts has been sent";
        public const string FlightNotAvailable = "Flight not available.";
        public const string DuplicateRecord = "Duplicate records found.";
        public const string DataNotFound = "Data Not Found";
        public const string Manually = "Manually";
        public const string Early = "Early";
        public const string DefaultTableName = "tblAttendee";
        public const string DefaultOrders = "[\"0\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"7\",\"8\",\"9\",\"10\",\"11\",\"12\",\"13\",\"14\",\"15\",\"16\",\"17\"]";
		public const string Today = "today";
		public const string Yesterday = "yesterday";
		public const string last7days = "last7days";
        public const string Arrival = "Arrival";
        public const string Depart = "Depart";
        public const string Filtered = "Filtered";
        public const string All = "All";
        public const string FilteredData= "Filtered Data";
        public const string AllData = "All Data";   
        public const string Selected = "Selected";
        public const string AllMeetings = "All Meetings";
        public const int AirportRefreshTimeHour = -3;

        // Flight status and state change message constant
        public const string FlightStatusAndStateChangedTo = "Flight status and state both changed to {0}/{1}";
        public const string FlightStatusAndStateChangedFromTo = "Flight status and state both changed from {0}/{1} to {2}/{3}";
        public const string FlightStatusChangedTo = "Flight status changed to {0}";
        public const string FlightStatusChangedFromTo = "Flight status changed from {0} to {1}";
        public const string FlightStateChangedTo = "Flight state changed to {0}";
        public const string FlightStateChangedFromTo = "Flight state changed from {0} to {1}";
        public const string NoStatusStateChange = "No status/state change";

        private static Dictionary<string, AirportLatLongData> _airportDictionary = new();
        public enum RefreshType
        {
            Weather = 1,
            Airport = 2
        }
    }
}
