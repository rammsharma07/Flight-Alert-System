using Domain.Meeting;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;

namespace Application.Interfaces.Meeting
{
    /// <summary>
    /// Service for sending email and SMS alerts for flight status changes
    /// </summary>
    public interface IAlertNotificationService
    {
		/// <summary>
		/// Sends email and SMS alerts based on flight status/state changes
		/// </summary>
		/// <param name="user">Meeting user details</param>
		/// <param name="meeting">Meeting details</param>
		/// <param name="newStatus">New flight status</param>
		/// <param name="newState">New flight state</param>
		/// <param name="oldStatus">Previous flight status</param>
		/// <param name="oldState">Previous flight state</param>
		/// <param name="flightDetails">Current flight information</param>
		/// <returns>Tuple indicating email and SMS send success</returns>
		Task<(bool emailSent, bool smsSent, string emailContent, string smsContent, List<string> selectedEmailStates, List<string> selectedEmailStatuses, List<string> selectedSmsStates, List<string> selectedSmsStatuses, bool sendEmail, bool sendSms, string smsMessageId, string EmailMessageId)> SendAlertsAsync(
			 MeetingUser user,
			 MeetingDetails meeting,
			 string newStatus,
			 string newState,
			 string oldStatus,
			 string oldState,
			 FlightStatusDetails flightDetails,
			 string htmlFilePath,
			 string logoURL, IServiceScope scope);

	}
}
