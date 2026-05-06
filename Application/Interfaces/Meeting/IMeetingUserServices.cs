using Domain.EntityHistory;
using Domain.Meeting;
using Domain.Response;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Meeting
{
    public interface IMeetingUserServices
    {
        /// <summary>
        /// Add Meeting User
        /// </summary>
        /// <param name="meetingUser"></param>
        /// <returns></returns>
        Task<Response<MeetingUser>> AddMeetingUserAsync(MeetingUser meetingUser);

        /// <summary>
        /// Add Meeting Details
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        Task<Response<MeetingUser>> AddMeetingDetailsAsync(IFormFile formFile, int userId, int selectedMeetingID);

        /// <summary>
        /// Get All Meetings User Details
        /// </summary>
        /// <param name="meetingID"></param>
        /// <returns></returns>
        Task<Response<MeetingUserDetails>> GetAllMeetingsUserDetails(int meetingID);

        /// <summary>
        /// Get Attendees 
        /// </summary>
        /// <param name="meetingID"></param>
        /// <param name="searchValue"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        Task<Response<MeetingUserDetails>> GetAllMeetingsUserDetailsPaged(int meetingID, string searchValue, int start, int length, Dictionary<string, string> filters, string sortColumn, string sortDirection);

        /// <summary>
        /// Get Meetings User Details
        /// </summary>
        /// <param name="meetingID"></param>
        /// <returns></returns>g
        Task<Response<MeetingUserDetails>> GetMeetingsUserDetails(int meetingID);
        /// <summary>
        /// Get All Archive Meetings User Details
        /// </summary>
        /// <param name="meetingID"></param>
        /// <returns></returns>
        Task<Response<MeetingUserDetails>> GetAllArchiveMeetingsUserDetails(int meetingID);
        /// <summary>
        /// Get All Meetings User Details of FlightDelay
        /// </summary>
        /// <param name="meetingID"></param>
        /// <returns></returns>
        Task<Response<MeetingUserDetails>> GetAllMeetingsUserDetailsFlightDelay(int meetingID);

        /// <summary>
        /// GetMeeting User By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response<MeetingUser>> GetMeetingUserById(int id);

        /// <summary>
        /// Update Meeting User Data
        /// </summary>
        /// <param name="meetingUser"></param>
        /// <returns></returns>
        Task<Response<MeetingUser>> UpdateMeetingUserData(MeetingUser meetingUser);

        /// <summary>
        /// Delete Meeting User Data
        /// </summary>
        /// <param name="meetingUser"></param>
        /// <returns></returns>
        Task<Response<MeetingUser>> DeleteMeetingUserData(MeetingUser meetingUser);

        /// <summary>
        /// Get All Meeting User Data
        /// </summary>
        /// <param name="commaSeparatedIds"></param>
        /// <returns></returns>
        Task<Response<MeetingUser>> GetAllMeetingUserData(string commaSeparatedIds);

        /// <summary>
        /// Get All Meeting Id With User Data
        /// </summary>
        /// <param name="commaSeparatedIds"></param>
        /// <returns></returns>

        Task<Response<MeetingUser>> GetAllMeetingIdWithUserData(string commaSeparatedIds, bool MeetingFlag);

        /// <summary>
        /// Bulk Send Alert Mail
        /// </summary>
        /// <param name="htmlFilePath"></param>
        /// <param name="logoURL"></param>
        /// <param name="meetingUser"></param>
        /// <returns></returns>
        Task BulkSendAlertMail(string htmlFilePath, string logoURL, List<MeetingUser> listMeetingUser);

        /// <summary>
        /// Bulk Send Alert Mail with event hub
        /// </summary>
        /// <param name="htmlFilePath"></param>
        /// <param name="logoURL"></param>
        /// <param name="meetingUser"></param>
        /// <returns></returns>
        Task BulkSendAlertMailEventHub(string htmlFilePath, string logoURL, string Trigger, string meetingId, bool MeetingFlag);


        /// <summary>
        /// Create Alert
        /// </summary>
        /// <returns></returns>
        Task<Response<MeetingUser>> createAlertAsync();

        /// <summary>
        /// createAlertAsync
        /// </summary>
        /// <param name="MeetingId"></param>
        /// <returns></returns>
		Task<Response<MeetingUser>> createAlertAsync(List<int> MeetingId);
        /// <summary>
        /// Update Alert
        /// </summary>
        /// <param name="meetingUser"></param>
        /// <param name="AlertID"></param>
        /// <returns></returns>
        Task<Response<MeetingUser>> UpdateAlertAsync(MeetingUser meetingUser, string AlertID);

        /// <summary>
        /// Get All meeting IDS for web job 
        /// </summary>
        /// <returns></returns>
        Task<Response<MeetingUser>> GetAllMeetingIdWithUserDataWebJob(int Hours);
        /// <summary>
        /// Get All meeting IDS for web job every 15 min
        /// </summary>
        /// <returns></returns>
        Task<Response<MeetingUser>> GetAllMeetingIdWithUserDataFasWebJob(int Hours);
        /// <summary>
        /// Exception store in Database 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="FunctionName"></param>
        /// <returns></returns>
        Task Exceptionsend(Exception ex, string FunctionName);
        /// <summary>
        /// Send Excaption Email
        /// </summary>
        /// <param name="htmlFilePath"></param>
        /// <param name="logoURL"></param>
        /// <param name="ex"></param>
        /// <param name="FunctionName"></param>
        /// <returns></returns>
        Task ExceptionSendMail(string htmlFilePath, string logoURL, Exception ex, string FunctionName);
        /// <summary>
        ///  Move data into ArchiveTable
        /// </summary>
        /// <returns></returns>
        Task MoveDataintoArchiveTable();
        /// <summary>
        /// move data in archive Table by ID
        /// </summary>
        /// <param name="MeetingID"></param>
        /// <returns></returns>
        Task MoveDataintoArchiveTable(int MeetingID);

        /// <summary>
        /// GetallAlertsbyMeetingID
        /// </summary>
        /// <param name="MeetingID"></param>
        /// <returns></returns>
        Task<List<string>> GetallAlertsbyMeetingID(int MeetingID);

        /// <summary>
        ///  UpdateAlertsettingAsyn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> UpdateAlertsettingAsync(SaveAlertSettingsRequest request);

        /// <summary>
        /// NotificationAlertsettingAsync
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> NotificationAlertsettingAsync(SaveAlertSettingsRequest model, int userId);

        /// <summary>
        /// MultipleNotificationAlertsettingAsync
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> MultipleNotificationAlertsettingAsync(SaveAlertSettingsRequest model, int userId);


        /// <summary>
        /// Get oldNotificationAlertsetting
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Dictionary<int, AlertSettingOldValueDto>> oldNotificationAlertsettingAsync(SaveAlertSettingsRequest model);

		/// <summary>
		/// GetOldAlertSettingForAttendeeTypeAsync
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		Task<AlertSettingOldValueDto?> GetOldAlertSettingForAttendeeTypeAsync(SaveAlertSettingsRequest model);


		/// <summary>
		/// MultipleNotificationAlertsettingForTypes
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		Task<bool> MultipleNotificationAlertsettingForTypes(SaveAlertSettingsRequest model, int userId);



        Task<Response<int>> InsertAlertSettingAuditHistory(
        AlertSettingHistoryAuditDto dto);

        Task<bool> InsertAlertSettingFieldHistoryChanges(
            List<AlertSettingFieldHistoryChangeDto> list);
        /// <summary>
        /// GetAllMeetingNotificationDetails
        /// </summary>
        /// <param name="meetingID"></param>
        /// <returns></returns>
        Task<Response<SaveAlertSettingsRequest>> GetAllMeetingNotificationDetails(int meetingID);

        /// <summary>
        /// GetAllAttendeeNotificationDetails
        /// </summary>
        /// <param name="meetingID"></param>
        /// <returns></returns>
        Task<Response<SaveAlertSettingsRequest>> GetAllAttendeeNotificationDetails(int meetingID);

        /// <summary>
        ///  GetAlertNotificationByattendeeId
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        Task<Response<SaveAlertSettingsRequest>> GetAlertNotificationByattendeeId(int ID);

        /// <summary>
        /// GetAlertNotificationAlertSettings
        /// </summary>
        /// <returns></returns>
        Task<Response<SaveAlertSettingsRequest>> GetAlertNotificationAlertSettings();

        /// <summary>
        /// InsertEventHubUserLogAsync
        /// </summary>
        /// <returns></returns>
        Task<Response<int>> InsertEventHubUserLogAsync(EventHubUserLog logEntry);

        /// <summary>
        /// GetAllMeetingUsersPaginated
        /// </summary>
        /// <returns></returns>
        Task<Response<MeetingUserDetails>> GetAllMeetingUsersPaginated(AttendeeSearchRequest request);

        /// <summary>
        /// GetAllAttendeeNameType
        /// </summary>
        /// <returns></returns>
        Task<Response<AttendeeFilterData>> GetAllAttendeeNameType(int meetingID);

        Task<Response<SaveAlertSettingsRequest>> GetAttendeeNotificationDetails(List<int> attendeeIds);

        Task<object> GetFilterOptions(int meetingId);

		/// <summary>
		/// UpdateActivityMeetingsettingAsync
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		Task UpdateActivityMeetingsettingAsync(ActivityMeetingDetails model);

		/// <summary>
		///  UpdateEmailStatus
		/// </summary>
		/// <param name="status"></param>
		/// <param name="messageId"></param>
		/// <returns></returns>
		Task UpdateEmailStatus(string status, string messageId);

		/// <summary>
		/// UpdateSmsStatus
		/// </summary>
		/// <param name="status"></param>
		/// <param name="messageId"></param>
		/// <returns></returns>
		Task UpdateSmsStatus(string status, string messageId);


	}
}
