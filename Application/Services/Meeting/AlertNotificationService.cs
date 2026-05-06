using Domain.Meeting;
using Application.Interfaces.Email;
using Application.Interfaces.SMS;
using Application.Interfaces.Meeting;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using Domain;
using Domain.Email;
using Web.Models;
using Persistance.Interfaces.Meeting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Amqp.Framing;

namespace Application.Services.Meeting
{
    /// <summary>
    /// Handles email and SMS notifications for flight status changes
    /// </summary>
    public class AlertNotificationService : IAlertNotificationService
    {
        // Dependencies for sending emails, SMS, and reading config
        private readonly IEmailService _emailService;
        private readonly ISmsServices _smsService;
        private readonly IConfiguration _configuration;

        public AlertNotificationService(
            IEmailService emailService,
            ISmsServices smsService,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _smsService = smsService;
            _configuration = configuration;
        }

        /// <summary>
        /// Main method: Sends alerts if status/state changed
        /// </summary>
        public async Task<(bool emailSent, bool smsSent, string emailContent, string smsContent, List<string> selectedEmailStates, List<string> selectedEmailStatuses, List<string> selectedSmsStates, List<string> selectedSmsStatuses, bool sendEmail, bool sendSms, string smsMessageId, string EmailMessageId)> SendAlertsAsync(
            MeetingUser user,
            MeetingDetails meeting,
            string newStatus,
            string newState,
            string oldStatus,
            string oldState,
            FlightStatusDetails flightDetails,
            string htmlFilePath,
            string logoURL, IServiceScope scope)
		{
			bool emailSent = false;
            bool smsSent = false;
            string emailContent = ""; 
            string smsContent = "";
			string smsMessageId = "";
            string EmailMessageId = "";

			//Get configured alert settings (Email states/statuses)
			var meetingUserRepo = scope.ServiceProvider.GetRequiredService<IMeetingUserRepository>();
            var Alertsetting = await meetingUserRepo.GetAllNotificationAlertSettings();

			var effectiveAlert = Alertsetting.Data.FirstOrDefault(x => x.AttendeeId == user.Id);

			if (effectiveAlert == null)
			{
				effectiveAlert = Alertsetting.Data.FirstOrDefault(x =>
					x.EntityType?.ToLower() == user.AttendeeType.ToLower());
			}

			if (effectiveAlert == null)
			{
				effectiveAlert = Alertsetting.Data.FirstOrDefault(x => x.MeetingId == user.MeetingID);
			}
			List<string> selectedEmailStates;
			List<string> selectedEmailStatuses;
			List<string> selectedSmsStates;
			List<string> selectedSmsStatuses;
			bool sendEmail;
			bool sendSms;

            if (effectiveAlert == null)
            {
                // Default values
                selectedEmailStates = new List<string> { AlertStateEnum.Canceled.ToString().ToLower() };
                selectedEmailStatuses = new List<string> { AlertStatusEnum.Delayed.ToString().ToLower() };
                selectedSmsStates = new List<string> { AlertStateEnum.Canceled.ToString().ToLower() };
                selectedSmsStatuses = new List<string> { AlertStatusEnum.Delayed.ToString().ToLower() };
                sendEmail = true;
                sendSms = true;
            }
            else
            {
                selectedEmailStates = effectiveAlert.EmailStates?.Select(e => e.ToString().ToLower()).ToList() ?? new();
                selectedEmailStatuses = effectiveAlert.EmailStatuses?.Select(e => e.ToString().ToLower()).ToList() ?? new();
                selectedSmsStates = effectiveAlert.SmsStates?.Select(e => e.ToString().ToLower()).ToList() ?? new();
                selectedSmsStatuses = effectiveAlert.SmsStatuses?.Select(e => e.ToString().ToLower()).ToList() ?? new();
                sendEmail = effectiveAlert.EmailAlert;
                sendSms = effectiveAlert.SmsAlert;
            }

            // Extract current flight status and state
            var eventStatus = newStatus?.Trim().ToLower() ?? "";

            // Clean the status - remove " by <duration>"
            if (eventStatus.Contains(" by "))
                eventStatus = eventStatus.Split(new[] { " by " }, StringSplitOptions.None)[0].Trim();

            var eventState = newState?.Trim().ToLower() ?? "";

			// Check if email should be sent
			// Send when: status changed + matches config OR state changed + matches config OR delayed status cleared
			bool shouldSendEmail = false;
			if (sendEmail && (oldStatus != newStatus?.ToLower() && selectedEmailStatuses.Any(s => s.Trim().ToLower() == eventStatus)))
			{
				shouldSendEmail = true;
			}
			if (sendEmail && (oldState != newState?.ToLower() && selectedEmailStates.Any(s => s.Trim().ToLower() == eventState)))
			{
				shouldSendEmail = true;
			}

			// Check if SMS should be sent
			bool shouldSendSms = false;
			if (sendSms && (oldStatus != newStatus?.ToLower() && selectedSmsStatuses.Any(s => s.Trim().ToLower() == eventStatus)))
			{
				shouldSendSms = true;
			}
			if (sendSms && (oldState != newState?.ToLower() && selectedSmsStates.Any(s => s.Trim().ToLower() == eventState)))
			{
				shouldSendSms = true;
			}

			//Send email if conditions met
			if (shouldSendEmail)
            {
                var result = await SendEmailAsync(user, meeting, newStatus, newState, flightDetails, htmlFilePath, logoURL);
                emailSent = result.sent;
                emailContent = result.content;
                EmailMessageId = result.EmailMessageId;

			}

            // Send SMS if conditions met
            if (shouldSendSms)
            {
                var result = SendSms(user, meeting, newStatus, newState, flightDetails);
                smsSent = result.sent;
                smsContent = result.content;
                smsMessageId = result.SmsMessageId;


			}

            // Return success status
            return (emailSent, smsSent, emailContent, smsContent, selectedEmailStates, selectedEmailStatuses, selectedSmsStates, selectedSmsStatuses, sendEmail, sendSms, smsMessageId, EmailMessageId);
        }

        /// <summary>
        /// Builds and sends HTML email with flight update details
        /// </summary>
        private async Task<(bool sent, string content, string EmailMessageId)> SendEmailAsync(
            MeetingUser user,
            MeetingDetails meeting,
            string status,
            string state,
            FlightStatusDetails flightDetails,
            string htmlFilePath,
            string logoURL)
        {
            try
            {
                // Check if template file exists
                if (!File.Exists(htmlFilePath))
                    return (false, string.Empty, string.Empty);

                // Read HTML template
                var htmlContent = await File.ReadAllTextAsync(htmlFilePath);

                // Calculate adjusted departure time (scheduled + variation)
                DateTime dt = DateTime.Parse(flightDetails.DepartureTimesScheduledLocal);
                DateTime adjustedDepartureTime = dt.Add(flightDetails.DepartureTimesEstimatedOutGateVariation);
                string depTime = adjustedDepartureTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);

                // Calculate adjusted arrival time (scheduled + variation)
                DateTime arrDt = DateTime.Parse(flightDetails.ArrivalTimesScheduledLocal);
                DateTime adjustedArrivalTime = arrDt.Add(flightDetails.DepartureTimesEstimatedOutGateVariation);
                string arrTime = adjustedArrivalTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);

                // Build email message body with flight details
                var fullName = $"{user.FirstName} {user.LastName}";
                var messageBody = $"The following flight has an update:<br><br>" +
                    $"Attendee: {fullName}<br><br>" +
                    $"Meeting Name: {meeting.MeetingName}<br><br>" +
                    $"Flight State: {state}<br>" +
                    $"Flight Status: {status}<br>" +
                    $"Flight Number: {flightDetails.FlightNumber}<br>" +
                    $"Airline: {user.CarrierName}<br><br>" +
                    $"Departure Time: {depTime} Local Time<br>" +
                    $"Departure Airport: {flightDetails.DepartureAirportIata},{flightDetails.DepartureAirportIcao}<br>" +
                    $"Arrival Time: {arrTime} Local Time<br>" +
                    $"Arrival Airport: {flightDetails.ArrivalAirportIata},{flightDetails.ArrivalAirportIcao}<br>";

                // Replace placeholders in HTML template
                htmlContent = htmlContent
                    .Replace("{{logoURL}}", logoURL)
                    .Replace("{{messageContent}}", messageBody)
                    .Replace("{{currentYear}}", DateTime.Now.Year.ToString());

                if (string.IsNullOrWhiteSpace(user.EmailId))
                    return (false, string.Empty, string.Empty);

                string[] EmailID = user.EmailId.Trim().Split(',');
				var messageIds = new List<string>();
				bool Sent = true;

                foreach (var Email in EmailID)
                {
                    // Create email details object
                    var emailDetails = new EmailDetails
                    {
                        APIKey = _configuration["EmailSenderOptions:apikey"],
                        FromMailNew = _configuration["EmailSenderOptions:FromMailNew"],
                        EmailFromName = "𝘱-value",
                        EmailID = Email.Trim(),
                        FullName = fullName,
                        Subject = $"Flight Status Alert - {status} - {state} - {fullName} - {flightDetails.FlightNumber}",
                        MessageBody = htmlContent
                    };
					string msgId = await _emailService.SendEmailAsyncnew(emailDetails);

					if (!string.IsNullOrEmpty(msgId))
					{
						messageIds.Add(msgId);
					}
					else
					{
						Sent = false;
					}
				}

				string emailMessageId = string.Join(",", messageIds);

				// Send email via email service
				//bool sent = await _emailService.SendEmailAsync(emailDetails);
				return (Sent, messageBody, emailMessageId);
            }
            catch
            {
                // Return false if any error occurs
                return (false, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Builds and sends SMS text message with flight update details
        /// </summary>
        private (bool sent, string content, string SmsMessageId) SendSms(
            MeetingUser user,
            MeetingDetails meeting,
            string status,
            string state,
            FlightStatusDetails flightDetails)
        {
            try
            {
                // Calculate adjusted departure time
                DateTime dt = DateTime.Parse(flightDetails.DepartureTimesScheduledLocal);
                DateTime adjustedDepartureTime = dt.Add(flightDetails.DepartureTimesEstimatedOutGateVariation);
                string depTime = adjustedDepartureTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);

                // Calculate adjusted arrival time
                DateTime arrDt = DateTime.Parse(flightDetails.ArrivalTimesScheduledLocal);
                DateTime adjustedArrivalTime = arrDt.Add(flightDetails.DepartureTimesEstimatedOutGateVariation);
                string arrTime = adjustedArrivalTime.ToString("MM/dd/yyyy - hh:mm tt", CultureInfo.InvariantCulture);

                // Build SMS text content
                var smsContent = $"Flight update for {user.FirstName} {user.LastName}\n" +
                    $"Meeting Name: {meeting.MeetingName}\n" +
                    $"Status: {status}\n" +
                    $"State: {state}\n" +
                    $"Airline: {user.CarrierName}\n" +
                    $"Flight Number: {flightDetails.FlightNumber}\n" +
                    $"Departure Time: {depTime}\n" +
                    $"Arrival Time: {arrTime}\n" +
                    $"Regards, Team 𝘱-value";

                // Return false if no phone number
                if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                    return (false, string.Empty, string.Empty);

                string[] PhoneNumbers = user.PhoneNumber.Trim().Split(',');
				var messageIds = new List<string>();
				bool Sent = true;

                foreach (var PhoneNumber in PhoneNumbers)
                {
					var sid = _smsService.SendSms(PhoneNumber.Trim(), smsContent);
					if (!string.IsNullOrEmpty(sid))
					{
						messageIds.Add(sid);
					}
				}
				string SmsmessageId = string.Join(",", messageIds);

				return (true, smsContent, SmsmessageId);
            }
            catch
            {
                return (false, string.Empty, string.Empty);
            }
        }
    }
}
