using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;
using Domain.Meeting;
using Domain.Response;
using Domain.Response.EventHub;
using Persistance.Interfaces.Meeting;
using Domain;
using Application.Interfaces.Utils;
using Application.Interfaces.Meeting;
using Serilog;
using System.Globalization;

namespace Application.Services.Meeting
{
    /// <summary>
    /// Real-time background service for processing Azure EventHub flight status events
    /// Monitors flight updates and sends email/SMS alerts to meeting attendees
    /// </summary>
    public class RealtimeEventHubService : BackgroundService
    {
        #region Private Fields

        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISequenceNumberManager _sequenceManager;
        private EventHubConsumerClient? _consumerClient;

        #endregion

        public RealtimeEventHubService(IConfiguration configuration,IServiceProvider serviceProvider,ISequenceNumberManager sequenceManager)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _sequenceManager = sequenceManager;
        }

        #region Service Lifecycle

        /// <summary>
        /// Main execution method - runs continuously in background
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            #region Serilog
            Log.Information("EventHub Service Started at {StartTime:yyyy-MM-dd HH:mm:ss} UTC", DateTime.UtcNow);
            #endregion

            //  Create EventHub consumer client
            _consumerClient = new EventHubConsumerClient(
                EventHubConsumerClient.DefaultConsumerGroupName,
                _configuration["OAGApiSettings:EventHubConnectionString"],
                _configuration["OAGApiSettings:EventHubName"]
            );

            try
            {
                // Read events continuously from EventHub
                await foreach (PartitionEvent partitionEvent in _consumerClient.ReadEventsAsync(startReadingAtEarliestEvent: false,cancellationToken: stoppingToken))
                {
                    if (partitionEvent.Data == null) continue;

                    try
                    {
                        await ProcessEventAsync(partitionEvent);
                    }
                    catch (Exception ex)
                    {
                        Log.Information(ex, "Error processing event: {ErrorMessage}", ex.Message);
                    }
                }
            }
            catch (TaskCanceledException) 
            {
                Log.Information("EventHub Service Stopped at {StopTime:yyyy-MM-dd HH:mm:ss} UTC", DateTime.UtcNow);
            }
            finally 
            { 
                await _consumerClient.DisposeAsync(); 
            }
        }

        #endregion

        #region Event Processing
        /// <summary>
        /// Processes a single EventHub event
        /// </summary>
        private async Task ProcessEventAsync(PartitionEvent partitionEvent)
        {
            var eventBody = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
            var eventData = JsonConvert.DeserializeObject<EventHubResponse>(eventBody);

            if (eventData?.AlertId == null || eventData.MessageId == null) return;
            if (_sequenceManager.IsProcessed(eventData.MessageId)) return;

            _sequenceManager.MarkProcessed(eventData.MessageId);

            var sequenceNumber = partitionEvent.Data.SequenceNumber;
            var flightDetails = EventHubResponseMapper.MapToUserFlightStatus(eventData);

            using var scope = _serviceProvider.CreateScope();

            // Update database with flight details
            await UpdateDatabaseAsync(flightDetails, sequenceNumber, scope);
        }
        #endregion

        #region Database Update Logic
        /// <summary>
        /// Updates database with flight status and sends alerts
        /// </summary>
        private async Task UpdateDatabaseAsync(FlightStatusDetails eventHub,long sequenceNumber,IServiceScope scope)
        {
            var meetingUserRepo = scope.ServiceProvider.GetRequiredService<IMeetingUserRepository>();
            var meetingUserService = scope.ServiceProvider.GetRequiredService<IMeetingUserServices>();
            var usersResponse = await meetingUserRepo.GetUsersByAlertIdAsync(eventHub.AlertId);

            if (usersResponse?.Data == null || !usersResponse.Data.Any()) return;

            var meetingsRepo = scope.ServiceProvider.GetRequiredService<IMeetingsRepository>();
            var alertService = scope.ServiceProvider.GetRequiredService<IAlertNotificationService>();

            List<MeetingUser> users = usersResponse.Data.ToList();

            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.LastEventOffset) && user.LastEventOffset == eventHub.MessageId)
                {
                    await LogSkipAsync(meetingUserService, meetingsRepo, eventHub, user, "Duplicate event - MessageId already processed");
                    continue;  // Skip duplicate
                }

                if (sequenceNumber <= (user.LastSequenceNumber ?? 0))
                {
                    await LogSkipAsync(meetingUserService, meetingsRepo, eventHub, user, "Old sequence number");
                    continue;
                }

                if (DateTime.TryParse(eventHub.DepartureTimesScheduledLocal, out var eventDepTime))
                {
                    var timeDiff = Math.Abs((eventDepTime - user.DepartureDateTime).TotalMinutes);
                    if (timeDiff > 5)
                    {
                        await LogSkipAsync(meetingUserService, meetingsRepo, eventHub, user, $"Time mismatch : {eventHub.DepartureTimesScheduledLocal}. Difference: {timeDiff} minutes.");
                        continue;
                    }
                }
                // If status is null add old value in new status
                var newStatus = !string.IsNullOrWhiteSpace(eventHub.DepartureTimesEstimatedOutGateTimeliness)
                            ? eventHub.DepartureTimesEstimatedOutGateTimeliness
                            : (!string.IsNullOrEmpty(user.Status) ? user.Status : string.Empty);

                if (newStatus.ToLower() == Constants.Delayed.ToLower())
                {
                    newStatus = eventHub.DepartureTimesEstimatedOutGateTimeliness + " by " +
                        eventHub.DepartureTimesEstimatedOutGateVariation;
                }
                else if (newStatus.ToLower() == Constants.Early.ToLower())
                {
                    eventHub.DepartureTimesEstimatedOutGateVariation = TimeSpan.Zero;
                }

                // If state is null add old value in new state
                var newState = !string.IsNullOrWhiteSpace(eventHub.State) ? eventHub.State : (!string.IsNullOrEmpty(user.state) ? user.state : string.Empty);
                var currentStatus = (user.Status ?? string.Empty).ToLower();
                var currentState = (user.state ?? string.Empty).ToLower();


                if (newStatus.ToLower() == currentStatus && newState.ToLower() == currentState)
                {
                    await LogSkipAsync(meetingUserService, meetingsRepo, eventHub, user, "No status/state change");
                    continue;
                }

                var changeMessage = BuildChangeMessage(currentStatus, newStatus, currentState, newState);

                var meetingsResponse = await meetingsRepo.GetAllMeetingDetailsId(user.MeetingID.ToString());
                var meeting = meetingsResponse.Data.FirstOrDefault();

                if (meeting == null)
                {
                    await LogSkipAsync(meetingUserService, meetingsRepo, eventHub, user, "Meeting not found");
                    continue;
                }

                bool emailSent = false;
                bool smsSent = false;

                string htmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "StatusAlertsTemplate.html");
                string logoURL = $"{_configuration["BaseUrl"]}/assets/images/logo-with-text.png";

                var alertResult = await alertService.SendAlertsAsync(
                    user, meeting, newStatus, newState, currentStatus, currentState, eventHub, htmlFilePath, logoURL, scope);

                emailSent = alertResult.emailSent;
                smsSent = alertResult.smsSent; 

                DateTime serverLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);

                var updateFields = new Dictionary<string, object>
                {
                    { "Status", newStatus },
                    { "State", newState },
                    { "LastSequenceNumber", sequenceNumber },
                    { "LastEventOffset", eventHub.MessageId },
                    { "LastSyncDateTime", serverLocalTime.ToString("dd/MM/yyyy, hh:mm:ss tt", CultureInfo.InvariantCulture) },
                    { "LastSyncDateTimeUtc", DateTime.UtcNow.ToString("dd/MM/yyyy, hh:mm:ss tt", CultureInfo.InvariantCulture)},
                    { "EmailSend", emailSent || user.EmailSend == true },
                    { "SmsSend", smsSent || user.SmsSend == true },
                    { "ModifiedDate", DateTime.UtcNow },
                    { "SmsMessageId", alertResult.smsMessageId},
					{ "EmailMessageId", alertResult.EmailMessageId}
				};

                // Execute partial update
                await meetingUserRepo.PartialUpdate(updateFields, "Id", new MeetingUser { Id = user.Id });

                
                await meetingsRepo.NotificationLog(
                    "RealtimeEventHub",
                    currentStatus,
                    newStatus.ToLower(),
                    currentState,
                    newState.ToLower(),
                    user.DepartureFlightNumber,
                    eventHub.DepartureTimesScheduledLocal,
                    eventHub.ArrivalTimesScheduledLocal,
                    user.CarrierCode,
                    "RealTime",
                    alertResult.emailContent,
                    alertResult.smsContent,
                    emailSent,
                    smsSent
                );

                // Log successful insert 
                await meetingUserService.InsertEventHubUserLogAsync(new EventHubUserLog
                {
                    AlertId = eventHub.AlertId,
                    UserId = user.Id,
                    UserName = $"{user.FirstName} {user.LastName}",
                    FlightNumber = user.DepartureFlightNumber,
                    DepartureDateTime = user.DepartureDateTime,
                    IsSkipped = false,
                    StatusFrom = currentStatus,
                    StatusTo = newStatus.ToLower(),
                    StateFrom = currentState,
                    StateTo = newState.ToLower(),
                    EmailSent = alertResult.emailSent,
                    SmsSent = alertResult.smsSent,
                    Emailalert = alertResult.sendEmail,
                    Smsalert = alertResult.sendSms,
                    EmailStatus = alertResult.selectedEmailStatuses,
                    EmailState = alertResult.selectedEmailStates,
                    SmsStatus = alertResult.selectedSmsStatuses,
                    SmsState = alertResult.selectedSmsStates,
                    EmailContent = alertResult.emailContent,
                    SmsContent = alertResult.smsContent,
                    CarrierCode = user.CarrierCode,
                    CarrierName = user.CarrierName,
                    MeetingID = user.MeetingID,
                    MeetingName = meeting.MeetingName,
                    Trigger = "RealTime",
                    SkipReason = changeMessage,
                    EmailMessageId = alertResult.EmailMessageId,
                    SmsMessageId = alertResult.smsMessageId,

				});

            }
        }
        #endregion

        // Helper method to insert skipped users
        private async Task LogSkipAsync(IMeetingUserServices service, IMeetingsRepository meetingRepository, FlightStatusDetails eventHub, MeetingUser user, string reason)
        {
            var meetingsResponse = await meetingRepository.GetAllMeetingDetailsId(user.MeetingID.ToString());
            var meeting = meetingsResponse?.Data?.FirstOrDefault();

            await service.InsertEventHubUserLogAsync(new EventHubUserLog
            {
                AlertId = eventHub.AlertId,
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                FlightNumber = user.DepartureFlightNumber,
                DepartureDateTime = user.DepartureDateTime,
                IsSkipped = true,
                SkipReason = reason,
                EmailSent = false,
                SmsSent = false,
                MeetingID = user.MeetingID,
                MeetingName = meeting?.MeetingName,
                CarrierCode = user.CarrierCode,
                CarrierName = user.CarrierName,
                Trigger = "RealTime",
            });
        }

        // Remove Extra Space helper method
        private string NormalizeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return string.Join(" ", value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        // Status and state changed message
        private string BuildChangeMessage(string currentStatus, string newStatus, string currentState, string newState)
        {
            currentStatus = NormalizeText(currentStatus);
            newStatus = NormalizeText(newStatus);
            currentState = NormalizeText(currentState);
            newState = NormalizeText(newState);

            bool hasCurrentStatus = !string.IsNullOrWhiteSpace(currentStatus);
            bool hasCurrentState = !string.IsNullOrWhiteSpace(currentState);

            bool hasNewStatus = !string.IsNullOrWhiteSpace(newStatus);
            bool hasNewState = !string.IsNullOrWhiteSpace(newState);

            bool statusChanged = hasNewStatus &&
                                 (!hasCurrentStatus || !string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase));

            bool stateChanged = hasNewState &&
                                (!hasCurrentState || !string.Equals(currentState, newState, StringComparison.OrdinalIgnoreCase));

            // BOTH status and state changed message
            if (statusChanged && stateChanged)
            {
                if (!hasCurrentStatus && !hasCurrentState)
                    return string.Format(Constants.FlightStatusAndStateChangedTo, newStatus, newState);

                return string.Format(Constants.FlightStatusAndStateChangedFromTo,
                    currentStatus, currentState, newStatus, newState);
            }

            // STATUS only changed
            if (statusChanged)
            {
                if (!hasCurrentStatus)
                    return string.Format(Constants.FlightStatusChangedTo, newStatus);

                return string.Format(Constants.FlightStatusChangedFromTo, currentStatus, newStatus);
            }

            // STATE only changed
            if (stateChanged)
            {
                if (!hasCurrentState)
                    return string.Format(Constants.FlightStateChangedTo, newState);

                return string.Format(Constants.FlightStateChangedFromTo, currentState, newState);
            }

            return Constants.NoStatusStateChange;
        }

    }
}
