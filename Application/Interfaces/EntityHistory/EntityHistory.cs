using Domain.EntityHistory;
using Domain.Meeting;
using Domain.Response;

namespace Application.Interfaces.EntityHistory
{
	public interface IEntityHistoryService
	{
		/// <summary>
		/// Save Entity History
		/// </summary>
		/// <param name="history"></param>
		/// <returns></returns>
		Task<Response<object>> SaveHistory(EntityHistoryDto history);

		/// <summary>
		/// Insert Entity Audit History 
		/// </summary>
		/// <param name="history"></param>
		/// <returns></returns>
		Task<Response<int>> InsertEntityAuditHistory(EntityHistoryAuditDto history);

		/// <summary>
		/// InsertEntityFieldHistoryChanges
		/// </summary>
		/// <param name="histories"></param>
		/// <returns></returns>
		Task<Response<object>> InsertEntityFieldHistoryChanges(List<EntityFieldHistoryChangeDto> histories);

		/// <summary>
		/// GetEntityHistoryPaged
		/// </summary>
		/// <param name="searchValue"></param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <param name="filters"></param>
		/// <returns></returns>

		Task<PagedResponse<EntityHistoryGridDto>> GetEntityHistoryPaged(string searchValue, int start, int length, Dictionary<string, string> filters);

		/// <summary>
		/// GetEntityHistoryDetails
		/// </summary>
		/// <param name="entityHistoryId"></param>
		/// <returns></returns>
		Task<List<EntityFieldHistoryChangeDto>> GetEntityHistoryDetails(int entityHistoryId);

		/// <summary>
		/// GetAttendeeHistoryByMeeting
		/// </summary>
		/// <param name="meetingId"></param>
		/// <returns></returns>
		Task<List<EntityHistoryGridDto>> GetAttendeeHistoryByMeeting(int meetingId);

		/// <summary>
		/// GetMeetingHistoryPaged
		/// </summary>
		/// <param name="searchValue"></param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <param name="filterDate"></param>
		/// <param name="filterModule"></param>
		/// <param name="filterAction"></param>
		/// <returns></returns>
		Task<PagedResponse<EntityHistoryGridDto>> GetMeetingHistoryPaged(string searchValue, int start, int length, string filterDate, string filterModule, string filterAction, int meetingId);

		/// <summary>
		/// GetAlertSettingHistoryDetails(
		/// </summary>
		/// <param name="alertHistoryId"></param>
		/// <returns></returns>
		Task<List<EntityFieldHistoryChangeDto>> GetAlertSettingHistoryDetails(int entityHistoryId);

		/// <summary>
		/// GetAlertSettingHistoryByAttendee
		/// </summary>
		/// <param name="attendeeId"></param>
		/// <returns></returns>
		Task<List<EntityHistoryGridDto>> GetAlertSettingHistoryByAttendee(int attendeeId);

		/// <summary>
		/// GetAlertSettingHistoryByMeeting
		/// </summary>
		/// <param name="meetingId"></param>
		/// <returns></returns>
		Task<List<EntityHistoryGridDto>> GetAlertSettingHistoryByMeeting(int meetingId);


		/// <summary>
		/// GetAllMeetingsAsync
		/// </summary>
		/// <param name="meetingId"></param>
		/// <returns></returns>
		Task<List<MeetingLookupDto>> GetAllMeetingsAsync(int meetingId);


		/// <summary>
		/// GetAttendeetypeAlertSettingHistoryByMeeting
		/// </summary>
		/// <param name="meetingId"></param>
		/// <returns></returns>
		Task<List<EntityHistoryGridDto>> GetAttendeetypeAlertSettingHistoryByMeeting(int meetingId);

        Task<IEnumerable<dynamic>> GetRecentActivitiesByMeetingId(int meetingId);
        Task<List<string>> GetMeetingNamesByIds(List<int> meetingIds);
    }
}
