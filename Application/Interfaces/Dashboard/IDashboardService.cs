using Domain.DashboardEntities;
using Domain.Response;

namespace Application.Interfaces.Dashboard
{
    public interface IDashboardService
    {
        /// <summary>
        /// Fetch all meeting names.
        /// </summary>
        /// <returns>List of MeetingName</returns>
        Task<List<MeetingName>> GetAllMeetingNames();

        /// <summary>
        /// Get dashboard overview for a meeting.
        /// </summary>
        /// <returns>Tiles count and summary data</returns>
        Task<DashboardOverviewDto> GetDashboardOverview(int meetingId);

        /// <summary>
        /// Get attendee types for a meeting.
        /// </summary>
        /// <returns>List of attendee types</returns>
        Task<List<AttendeeTypeDto>> GetAttendeeType(int meetingId);

        /// <summary>
        /// Get paginated passenger flight info for a meeting.
        /// </summary>
        /// <returns>Passenger flight details with pagination</returns>
        Task<PassengerFlightResponseDto> GetPassengerFlights(int meetingId,int page, int pageSize);

        /// <summary>
        /// Get airport traffic data for a meeting, optionally refresh.
        /// </summary>
        /// <returns>Airport delay and traffic info</returns>
        Task<AirportTrafficResponseDto> GetAirportTrafficByMeeting(int meetingId, bool refresh = false);

        /// <summary>
        /// Get meeting notification alert summary.
        /// </summary>
        /// <returns>Summary of alert settings</returns>
        Task<NotificationAlertSummaryDto> GetMeetingAlertSummary(int meetingId);

        /// <summary>
        /// Get count of notifications sent and failed for a meeting.
        /// </summary>
        /// <returns>Notification counts by channel</returns>
        Task<NotificationChannelCountCount> GetNotificationSentCount(int meetingId);

        /// <summary>
        /// Get notification history for a meeting.
        /// </summary>
        /// <returns>List of sent notifications</returns>
        Task<List<NotificationHistoryDto>> GetNotificationHistory(int meetingId);

        /// <summary>
        /// Get airport traffic for specified airports, optionally refresh.
        /// </summary>
        /// <returns>Airport traffic data</returns>
        Task<Response<AirportTrafficResponse>> GetAirportTrafficData(List<AirportRequest> airports, bool refresh = false, int meetingId = 0);

        /// <summary>
        /// Get recent activities for a meeting.
        /// </summary>
        /// <returns>Activity logs</returns>
        Task<IEnumerable<dynamic>> GetRecentActivitiesByMeetingId(int meetingId);

        /// <summary>
        /// Get weather data for airports in a meeting, optionally force refresh.
        /// </summary>
        /// <returns>Weather info for destination airports</returns>
        Task<List<AirportWeatherDto>> GetWeatherForMeeting(int meetingId, bool forceRefresh = false);
    }
}
