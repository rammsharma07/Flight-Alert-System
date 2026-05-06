using Application.Interfaces.Dashboard;
using Domain;
using Domain.DashboardEntities;
using Domain.Meeting;
using Domain.Response;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Persistance.Interfaces.Dashboard;
using System.Net;
using Application.Interfaces.EntityHistory;
using Serilog;
using static Domain.Constants;
using Persistance.Interfaces.Meeting;
using Persistance.Interfaces.History;
using Domain.History;

namespace Application.Services.Dashboard
{
    public class DashboardService(IDashboardRepository dashboardRepository, IConfiguration configuration, IEntityHistoryService entityHistoryService, IMeetingUserRepository meetingsUserRepository, INotificationHistory notificationHistroyRepository) : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository = dashboardRepository;
        private readonly IEntityHistoryService _entityHIstoryService = entityHistoryService;
        private readonly IMeetingUserRepository _meetingsUserRepository = meetingsUserRepository;
        private readonly INotificationHistory _notificationHistroyRepository = notificationHistroyRepository;
        private readonly IConfiguration _configuration = configuration;
        private static readonly Dictionary<string, AirportLatLongData> _airportDictionary = new();
        private static bool _dataLoaded = false;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Fetch all meeting names.
        /// </summary>
        /// <returns>List of MeetingName</returns>
        public async Task<List<MeetingName>> GetAllMeetingNames()
        {
            return await _dashboardRepository.GetAllMeetingNames();
        }

        /// <summary>
        /// Get dashboard overview data for a specific meeting.
        /// </summary>
        /// <returns>DashboardOverviewDto</returns>
        public async Task<DashboardOverviewDto> GetDashboardOverview(int meetingId)
        {
            var countsTask = _dashboardRepository.GetDashboardOverviewCounts(meetingId);// Get Counts For Arrival and Departure
            var incompleteTask = _dashboardRepository.GetIncompleteCount(meetingId);// Get Incomplete Information rows Counts for meeting 
            var mismatchTask = _dashboardRepository.GetTimeMismatchCount(meetingId);// Get Time Mismatch rows Counts for meeting
            await Task.WhenAll(countsTask, incompleteTask, mismatchTask);
            var counts = countsTask.Result;
            var arrival = counts.FirstOrDefault(x => x.Type == Constants.Arrival) ?? new();// Get Arrival Flight Counts
            var departure = counts.FirstOrDefault(x => x.Type == Constants.Depart) ?? new();// Get Departure Flight Counts

            return new DashboardOverviewDto
            {
                // Arrivals Attendee Overview 
                ArrivalAttendee = new AttendeeOverviewDto
                {
                    TotalAttendees = arrival.Total,
                    ArrivalsRemaining = arrival.Remaining,
                    DeparturesRemaining = arrival.Arrived,
                    Delayed = arrival.Delayed
                },
                // Departures Attendee Overview
                DepartureAttendee = new AttendeeOverviewDto
                {
                    TotalAttendees = departure.Total,
                    ArrivalsRemaining = departure.Remaining,
                    DeparturesRemaining = departure.Arrived,
                    Delayed = departure.Delayed
                },
                // Arrivals Flight Overview
                ArrivalFlights = new FlightOverviewDto
                {
                    Landed = arrival.Landed,
                    InAir = arrival.InAir,
                    Scheduled = arrival.Scheduled,
                    Cancelled = arrival.Cancelled
                },
                // Departures Flight Overview
                DepartureFlights = new FlightOverviewDto
                {
                    Landed = departure.Landed,
                    InAir = departure.InAir,
                    Scheduled = departure.Scheduled,
                    Cancelled = departure.Cancelled
                },
                IncompleteCount = incompleteTask.Result,
                TimeMismatchCount = mismatchTask.Result
            };
        }

        /// <summary>
        /// Get attendee types for a meeting.
        /// </summary>
        /// <returns>List of attendee types</returns>
        public async Task<List<AttendeeTypeDto>> GetAttendeeType(int meetingId)
        {
            return await _dashboardRepository.GetAttendeeTypes(meetingId);
        }

        /// <summary>
        /// Get paginated passenger flight info for a meeting.
        /// </summary>
        /// <returns>Passenger flight details with pagination</returns>
        public async Task<PassengerFlightResponseDto> GetPassengerFlights(int meetingId, int page, int pageSize)
        {
            var users = await _dashboardRepository.GetPassengerFlightsData(meetingId, page, pageSize);// Get all passenger flight data for the meeting
            if (users.Count == 0)
                return new PassengerFlightResponseDto { PassengerFlightDetails = new List<PassengerFlightDetailsDto>() };

            // Fetch all notification settings
            var allAlertSettings = await _meetingsUserRepository.GetAllNotificationAlertSettings();
            var alertSettings = allAlertSettings.Data;

            var passengerFlights = new List<PassengerFlightDetailsDto>();
            var connectingCandidates = users
                .Where(x => x.Connecting)
                .ToList();

            var connectingGroups = connectingCandidates // Connecting flight Grouping logic
                .GroupBy(x => new
                {
                    Name = (x.FirstName + " " + x.LastName).Trim().ToLower(),
                    Email = (x.Email ?? "").Trim().ToLower(),
                    LabelType = x.FlightType.Split(' ')[0].ToLower()
                })
                .Where(g => g.Count() > 1)
                .ToList();

            var usedRows = new HashSet<PassengerFlightEntity>();

            foreach (var group in connectingGroups) // Process connecting flights 
            {
                var groupType = group.Key.LabelType;
                var flights = group.OrderBy(x => x.DepartureDateTime).ToList();
                for (int i = 0; i < flights.Count; i++)
                {
                    var flightData = flights[i];
                    usedRows.Add(flightData);
                    passengerFlights.Add(new PassengerFlightDetailsDto
                    {
                        AttendeeName = flightData.FirstName + " " + flightData.LastName,
                        AttendeeType = flightData.AttendeeType,
                        CarrierCode = flightData.CarrierCode,
                        FlightNumber = flightData.DepartureFlightNumber,
                        CarrierName = flightData.CarrierName,
                        OriginAirport = flightData.OriginAirport,
                        DestinationAirport = flightData.DestinationAirport,
                        DepartureDateTime = flightData.DepartureDateTime ?? DateTime.MinValue,
                        ArrivalDateTime = flightData.ArrivalDateTime ?? DateTime.MinValue,
                        State = flightData.State,
                        IsConnecting = true,
                        LegText = $"Leg {i + 1} of {flights.Count}",
                        NotificationLevel = GetNotificationLevel(flightData, alertSettings, meetingId),
                        FlightGroupType = groupType
                    });
                }
            }

            var normalFlights = users.Where(x => !usedRows.Contains(x)).ToList();// Process non-connecting flights
            foreach (var flightsData in normalFlights)
            {
                passengerFlights.Add(new PassengerFlightDetailsDto
                {
                    AttendeeName = flightsData.FirstName + " " + flightsData.LastName,
                    AttendeeType = flightsData.AttendeeType,
                    CarrierCode = flightsData.CarrierCode,
                    FlightNumber = flightsData.DepartureFlightNumber,
                    CarrierName = flightsData.CarrierName,
                    OriginAirport = flightsData.OriginAirport,
                    DestinationAirport = flightsData.DestinationAirport,
                    DepartureDateTime = flightsData.DepartureDateTime ?? DateTime.MinValue,
                    ArrivalDateTime = flightsData.ArrivalDateTime ?? DateTime.MinValue,
                    State = flightsData.State,
                    IsConnecting = false,
                    LegText = "",
                    NotificationLevel = GetNotificationLevel(flightsData, alertSettings, meetingId),
                    FlightGroupType = flightsData.FlightType
                });
            }

            return new PassengerFlightResponseDto
            {
                PassengerFlightDetails = passengerFlights,
            };
        }

        /// <summary>
        /// Get airport traffic data for a meeting, optionally refresh.
        /// </summary>
        /// <returns>Airport delay and traffic info</returns>
        public async Task<AirportTrafficResponseDto> GetAirportTrafficByMeeting(int meetingId, bool refresh = false)
        {
            if (meetingId <= 0)
            {
                return new AirportTrafficResponseDto
                {
                    AirportTraffic = new List<AirportTrafficResponse>()
                };
            }

            // Get distinct airports for meeting
            var destinationAirports = await _dashboardRepository.GetDistinctDestinationAirports(meetingId);
            if (destinationAirports == null || !destinationAirports.Any())
            {
                return new AirportTrafficResponseDto
                {
                    AirportTraffic = new List<AirportTrafficResponse>()
                };
            }

            // Resolve airport codes and prepare requests
            var airportRequests = destinationAirports
                .Select(a => ResolveAirportCode(a))
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => new AirportRequest
                {
                    Code = code,
                    CodeType = code.Length == 3 ? "iata" : "icao"
                })
                .ToList();

            if (!airportRequests.Any())
            {
                return new AirportTrafficResponseDto
                {
                    AirportTraffic = new List<AirportTrafficResponse>()
                };
            }

            // Fetch traffic data from DB/API
            var trafficData = await GetAirportTrafficData(airportRequests, refresh, meetingId);
            // fetch last refresh time 
            var lastRefresh = await _dashboardRepository.GetLastRefreshTimeAgo(meetingId, RefreshType.Airport);

            return new AirportTrafficResponseDto
            {
                AirportTraffic = trafficData.Data,
                LastRefreshTime = lastRefresh
            };

        }

        /// <summary>
        /// Get notification level name
        /// </summary>
        /// <returns>Notitication setting name</returns>
        private string GetNotificationLevel(PassengerFlightEntity flight, List<SaveAlertSettingsRequest> alertSettings, int meetingId)
        {
            // Attendee-level (specific user)
            if (alertSettings.Any(a => a.AttendeeId == flight.Id && (a.EmailAlert || a.SmsAlert)))
                return "Attendee";

            // AttendeeType-level (only if EmailAlert/SmsAlert is ON)
            if (alertSettings.Any(a =>
                !string.IsNullOrEmpty(a.EntityType) &&
                a.EntityType.Equals(flight.AttendeeType, StringComparison.OrdinalIgnoreCase) &&
                (a.EmailAlert || a.SmsAlert)))
            {
                return "AttendeeType";
            }

            // Meeting-level (fallback)
            if (alertSettings.Any(a => a.MeetingId == meetingId && (a.EmailAlert || a.SmsAlert)))
                return "Meeting";

            // If nothing return meeting
            return "Meeting";
        }

        /// <summary>
        /// Get meeting notification alert summary.
        /// </summary>
        /// <returns>Summary of alert settings</returns>
        public async Task<NotificationAlertSummaryDto> GetMeetingAlertSummary(int meetingId)
        {
            if (meetingId <= 0)
            {
                return new NotificationAlertSummaryDto
                {
                    MeetingId = meetingId,
                    SmsStates = new List<string>(),
                    SmsStatuses = new List<string>(),
                    EmailStates = new List<string>(),
                    EmailStatuses = new List<string>()
                };
            }
            var data = await _dashboardRepository.GetMeetingAlertSetting(meetingId);
            // If no alert settings found for the meeting, return empty lists to avoid null reference issues on frontend
            if (data == null)
            {
                return new NotificationAlertSummaryDto
                {
                    MeetingId = meetingId,
                    SmsStates = new List<string>(),
                    SmsStatuses = new List<string>(),
                    EmailStates = new List<string>(),
                    EmailStatuses = new List<string>()
                };
            }
            // Get State Values from Enum
            List<string> MapStates(string value)
            {
                return string.IsNullOrWhiteSpace(value)
                    ? new List<string>()
                    : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(x => Enum.IsDefined(typeof(AlertStateEnum), int.Parse(x))
                                ? ((AlertStateEnum)int.Parse(x)).ToString()
                                : x)
                           .ToList();
            }
            // Get Status Values from Enum 
            List<string> MapStatuses(string value)
            {
                return string.IsNullOrWhiteSpace(value)
                    ? new List<string>()
                    : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(x => Enum.IsDefined(typeof(AlertStatusEnum), int.Parse(x))
                                ? ((AlertStatusEnum)int.Parse(x)).ToString()
                                : x)
                           .ToList();
            }
            return new NotificationAlertSummaryDto
            {
                MeetingId = meetingId,
                SmsStates = MapStates(data.SmsState),
                SmsStatuses = MapStatuses(data.SmsStatus),
                EmailStates = MapStates(data.EmailState),
                EmailStatuses = MapStatuses(data.EmailStatus)
            };
        }

        /// <summary>
        /// Gets flight priority for sorting arrivals and departures.
        /// </summary>
        /// <returns>Flight sorting priority value</returns>
        private int GetFlightPriority(PassengerFlightDetailsDto flightType)
        {
            if (string.IsNullOrEmpty(flightType.FlightGroupType))
                return 2;
            var type = flightType.FlightGroupType.ToLower();

            if (type.Contains(Constants.Arrival.ToLower()))
                return 0;
            if (type.Contains(Constants.Depart.ToLower()))
                return 1;

            return 2;
        }

        /// <summary>
        /// Gets flight date and time for sorting.
        /// </summary>
        /// <returns>Flight date and time value</returns>
        private DateTime GetFlightDateTime(PassengerFlightDetailsDto flightDateTime)
        {
            if (flightDateTime.FlightGroupType?.ToLower().Contains(Constants.Arrival.ToLower()) == true)
                return flightDateTime.ArrivalDateTime;

            return flightDateTime.DepartureDateTime;
        }

        /// <summary>
        /// Gets notification history for the meeting.
        /// </summary>
        /// <returns>Notification history list</returns>
        public async Task<List<NotificationHistoryDto>> GetNotificationHistory(int meetingId)
        {
            var queryDto = new NotifQueryDto
            {
                MeetingID = meetingId,
                PageSize = 10
            };
            var response = await _notificationHistroyRepository.GetNotificationHistory(queryDto);
            var result = response.Data
                .Select(x => new NotificationHistoryDto
                {
                    UserName = x.UserName ?? "",
                    FlightNumber = x.FlightNumber ?? "",
                    SkipReason = x.SkipReason ?? "",
                    EmailSent = x.EmailSent ?? false,
                    SmsSent = x.SmsSent ?? false,
                    LoggedAtUtc = x.LoggedAtUtc ?? DateTime.MinValue
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// Parses airport CSV data line.
        /// </summary>
        /// <returns>Parsed airport data fields</returns>
        private static string[] ParseAirportData(string csvLine)
        {
            var parsedFields = new List<string>();
            bool insideQuotes = false;
            var currentField = new System.Text.StringBuilder();

            // Process each character in the line
            foreach (char currentChar in csvLine)
            {
                if (currentChar == '"')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (currentChar == ',' && !insideQuotes)
                {
                    parsedFields.Add(currentField.ToString().Trim().Trim('"'));
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(currentChar);
                }
            }
            parsedFields.Add(currentField.ToString().Trim().Trim('"'));
            return parsedFields.ToArray();
        }

        /// <summary>
        /// Loads airport data from CSV file.
        /// </summary>
        /// <returns>None</returns>
        private void LoadAirportData()
        {
            if (_dataLoaded) return;

            // Build path to CSV
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "airports.csv");

            if (!File.Exists(path))
                throw new FileNotFoundException($"airports.csv not found at: {path}");

            var lines = File.ReadAllLines(path);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = ParseAirportData(line);
                if (parts.Length < 14) continue;

                var type = parts[2];
                if (type != "large_airport" && type != "medium_airport") continue;

                var icao = parts[12]?.Trim();
                var iata = parts[13]?.Trim();
                var city = parts[10]?.Trim();

                if (!double.TryParse(parts[4],
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double lat)) continue;

                if (!double.TryParse(parts[5],
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double lng)) continue;

                if (string.IsNullOrWhiteSpace(icao)) continue;

                var airportInfo = new AirportLatLongData
                {
                    Icao = icao,
                    Iata = iata,
                    City = city,
                    Latitude = lat,
                    Longitude = lng
                };

                _airportDictionary[icao.ToUpper()] = airportInfo;

                if (!string.IsNullOrWhiteSpace(iata))
                    _airportDictionary[iata.ToUpper()] = airportInfo;
            }

            _dataLoaded = true;
        }

        /// <summary>
        /// Gets airport coordinates by airport code.
        /// </summary>
        /// <returns>Latitude and longitude coordinates</returns>
        public (double Latitude, double Longitude)? GetAirportCoordinates(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            code = code.ToUpper().Trim();
            return _airportDictionary.TryGetValue(code, out var airport)
                ? (airport.Latitude, airport.Longitude)
                : null;
        }

        /// <summary>
        /// Gets airport traffic data from database or API.
        /// </summary>
        /// <returns>Airport traffic response data</returns>
        public async Task<Response<AirportTrafficResponse>> GetAirportTrafficData(List<AirportRequest> airports, bool refresh = false, int meetingId = 0)
        {
            // Loads airport data from airports.csv
            LoadAirportData();

            var codes = airports.Select(a => a.Code).ToList();
            var dbData = await _dashboardRepository.GetAirportTrafficCache(codes);
            var currentTime = DateTime.UtcNow;
            var dataExpiry = currentTime.AddHours(Constants.AirportRefreshTimeHour);
            var results = new List<AirportTrafficResponse>();
            var toFetch = new List<AirportRequest>();

            // Use DB data if recent (< 3 hour old)
            foreach (var airport in airports)
            {
                var recentDbRecord = dbData.FirstOrDefault(d => d.Code == airport.Code);
                if (!refresh && recentDbRecord != null && recentDbRecord.LastUpdated > dataExpiry)
                {
                    results.Add(new AirportTrafficResponse
                    {
                        Code = recentDbRecord.Code,
                        CodeType = recentDbRecord.CodeType,
                        City = recentDbRecord.City,
                        Latitude = recentDbRecord.Latitude,
                        Longitude = recentDbRecord.Longitude,
                        DelayIndex = recentDbRecord.DelayIndex
                    });
                }
                else
                {
                    toFetch.Add(airport);
                }
            }

            // Fetch fresh data if needed
            if (toFetch.Any())
            {
                var freshData = await FetchAirportTrafficFromApi(toFetch);
                results.AddRange(freshData);

                // Update cache
                var dbAirportTrafficRecords = freshData.Select(f => new AirportTrafficEntity
                {
                    Code = f.Code,
                    CodeType = f.CodeType,
                    City = f.City,
                    Latitude = f.Latitude,
                    Longitude = f.Longitude,
                    DelayIndex = f.DelayIndex,
                    LastUpdated = currentTime
                }).ToList();

                await _dashboardRepository.SaveAirportTrafficCache(dbAirportTrafficRecords);
                // update airport refresh time
                await _dashboardRepository.UpdateRefreshTime(meetingId, RefreshType.Airport);
            }

            return new Response<AirportTrafficResponse>
            {
                Status = HttpStatusCode.OK,
                Message = "Airport traffic data retrieved successfully",
                Data = results,
                TotalRecords = results.Count,
                status = true
            };
        }

        /// <summary>
        /// Fetches airport traffic data from external API.
        /// </summary>
        /// <returns>Airport traffic details</returns>
        private async Task<List<AirportTrafficResponse>> FetchAirportTrafficFromApi(List<AirportRequest> airports)
        {
            // Get api configuration from appsetting
            var apiKey = _configuration["RapidApi:Key"];
            var host = _configuration["RapidApi:Host"];
            var baseUrl = _configuration["RapidApi:BaseUrl"];
            using var client = new HttpClient();

            // Process all airports in parallel
            var tasks = airports.Select(async airport =>
            {
                // Get airport coordinates and city
                string city = null;
                var coords = GetAirportCoordinates(airport.Code);
                if (!_airportDictionary.TryGetValue(airport.Code.ToUpper(), out var airportData))
                    city = "Unknown";
                else
                    city = airportData.City;

                double? delayIndex = null;
                await _semaphore.WaitAsync(); // API rate limiting 1 request per sec (Radpid api rate limit)
                try
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        $"{baseUrl}/{airport.CodeType}/{airport.Code}/delays"
                    );

                    request.Headers.Add("x-rapidapi-key", apiKey);
                    request.Headers.Add("x-rapidapi-host", host);

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        var json = JsonConvert.DeserializeObject<JObject>(body);
                        var depInfo = json?["departuresDelayInformation"];

                        if (depInfo?["delayIndex"] != null)
                            delayIndex = depInfo["delayIndex"]!.Value<double>();
                    }
                    else
                    {
                        Log.Information("API failed for {Code} with status {Status}", airport.Code, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Log.Information("API error for {Code}: {Error}", airport.Code, ex.Message);
                }
                finally
                {
                    await Task.Delay(1200); // maintain rate limit
                    _semaphore.Release();
                }

                return new AirportTrafficResponse
                {
                    Code = airport.Code,
                    CodeType = airport.CodeType,
                    City = city,
                    Latitude = coords.Value.Latitude,
                    Longitude = coords.Value.Longitude,
                    DelayIndex = delayIndex
                };
            });

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        /// <summary>
        /// Resolves airport code from input value.
        /// </summary>
        /// <returns>Resolved airport code</returns>
        private string ResolveAirportCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (!_dataLoaded)
                LoadAirportData();

            input = input.Trim().ToUpper();

            if (input.Contains(","))
            {
                var parts = input.Split(',');
                var last = parts.Last().Trim();

                if (_airportDictionary.ContainsKey(last))
                    return last;

                input = parts.First().Trim();
            }

            if (_airportDictionary.ContainsKey(input))
                return input;

            if (input.Contains(","))
                input = input.Split(',')[0].Trim();

            var airport = _airportDictionary.Values
                .FirstOrDefault(a =>
                    !string.IsNullOrWhiteSpace(a.City) &&
                    a.City.ToUpper() == input);

            if (airport != null)
                return !string.IsNullOrWhiteSpace(airport.Icao)
                    ? airport.Icao
                    : airport.Iata;

            return null;
        }

        /// <summary>
        /// Gets recent activities for the meeting.
        /// </summary>
        /// <returns>Recent activity list</returns>
        public async Task<IEnumerable<dynamic>> GetRecentActivitiesByMeetingId(int meetingId)
        {
            var activityHistory = await _entityHIstoryService.GetRecentActivitiesByMeetingId(meetingId);
            return activityHistory;
        }

        /// <summary>
        /// Gets weather details for the meeting destination airport.
        /// </summary>
        /// <returns>Weather information list</returns>
        public async Task<List<AirportWeatherDto>> GetWeatherForMeeting(int meetingId, bool forceRefresh = false)
        {
            // Check: meetingId invalid → return empty
            if (meetingId <= 0)
                return new List<AirportWeatherDto>();

            var (topAirport, arrivalDate) = await _dashboardRepository.GetTopAirportAsync(meetingId);

            if (string.IsNullOrEmpty(topAirport) || arrivalDate == null)
                return new List<AirportWeatherDto>();

            AirportWeatherDto weather;

            if (forceRefresh)
            {
                weather = await CallForecastApi(topAirport, arrivalDate.Value);
                await _dashboardRepository.UpdateRefreshTime(meetingId, RefreshType.Weather);
                if (weather != null)
                {
                    await _dashboardRepository.UpsertWeather(weather);
                }
            }
            else
            {
                weather = await GetWeatherWithForecastCache(topAirport, arrivalDate.Value, meetingId);
            }
            // fetch last refresh time 
            var lastRefresh = await _dashboardRepository.GetLastRefreshTimeAgo(meetingId, RefreshType.Weather);
            return weather != null
                ? new List<AirportWeatherDto>
                  {
                      new AirportWeatherDto
                      {
                          AirportCode = weather.AirportCode,
                          Temperature = weather.Temperature,
                          Description = weather.Description,
                          Icon = weather.Icon,
                          LastRefreshTime = lastRefresh
                      }
                  }
                : new List<AirportWeatherDto>();
        }

        /// <summary>
        /// Gets weather data from database or forecast API.
        /// </summary>
        /// <returns>Weather details</returns>
        private async Task<AirportWeatherDto> GetWeatherWithForecastCache(string airportCode, DateTime targetDate,int meetingId)
        {
            var cached = await _dashboardRepository.GetWeather(airportCode);// Check if we have fresh data in cache (less than 1 hour old)
            if (cached != null)
            {
                var diff = DateTime.UtcNow - cached.LastUpdated;
                if (diff.TotalMinutes < 60)
                {
                    return new AirportWeatherDto
                    {
                        AirportCode = cached.AirportCode,
                        Temperature = cached.Temperature,
                        Description = cached.Description,
                        Icon = cached.Icon
                    };
                }
            }

            var getNewWeatherData = await CallForecastApi(airportCode, targetDate); // Call API to get new data if cache is not present
            // update weather refresh time
            await _dashboardRepository.UpdateRefreshTime(meetingId, RefreshType.Weather);
            if (getNewWeatherData != null)
            {
                await _dashboardRepository.UpsertWeather(getNewWeatherData);
            }
            return getNewWeatherData;
        }

        /// <summary>
        /// Fetches weather forecast data from external API.
        /// </summary>
        /// <returns>Weather forecast details</returns>
        private async Task<AirportWeatherDto> CallForecastApi(string airportCode, DateTime targetDate)
        {
            var apiKey = _configuration["OpenWeather:ApiKey"];// Get Api Key From configuration
            var baseUrl = _configuration["OpenWeather:BaseUrl"];// Get Base Url from configuration
            var city = airportCode.Split(',')[0].Trim();
            var url = $"{baseUrl}/forecast?q={city}&appid={apiKey}&units=metric";// Main URL For Weather Forecast API call
            using var http = new HttpClient();
            var res = await http.GetAsync(url);

            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            var list = ((IEnumerable<dynamic>)data.list)
             .Select(x => new
             {
                 Data = x,
                 ForecastTime = DateTime.Parse((string)x.dt_txt)
             });
            // Find the forecast entry closest to the target date (meeting date)
            var closest = list
                .OrderBy(x => Math.Abs((x.ForecastTime - targetDate).TotalSeconds))
                .FirstOrDefault()?.Data;

            if (closest == null) return null;
            return new AirportWeatherDto
            {
                AirportCode = airportCode,
                Temperature = (double?)closest.main.temp,
                Description = (string)closest.weather[0].description,
                Icon = (string)closest.weather[0].icon
            };
        }

        /// <summary>
        /// Gets notification sent and failed counts for the meeting.
        /// </summary>
        /// <returns>Notification count details</returns>
        public async Task<NotificationChannelCountCount> GetNotificationSentCount(int meetingId)
        {
            var data = await _dashboardRepository.GetNotificationSentCount(meetingId);// Get Sent count from database
            if (data == null)
            {
                return new NotificationChannelCountCount
                {
                    MeetingId = meetingId
                };
            }
            data.EmailFailed = data.TotalEmails - data.EmailSent;// Calculate Failed count based on total and sent counts
            data.SmsFailed = data.TotalSms - data.SmsSent;

            return data;
        }
    }
}