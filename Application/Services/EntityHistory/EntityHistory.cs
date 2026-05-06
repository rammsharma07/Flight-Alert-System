using Application.Interfaces.EntityHistory;
using Persistance.Interfaces.EntityHistory;
using Domain.Response;
using Domain.EntityHistory;
using Domain.Meeting;

namespace Application.Services.EntityHistory
{
	public class EntityHistoryService(IEntityHistoryRepository entityHistoryRepository) : IEntityHistoryService
	{
		private readonly IEntityHistoryRepository _entityHistoryRepository = entityHistoryRepository;

		/// <summary>
		/// Save Entity History
		/// </summary>
		/// <param name="history"></param>
		/// <returns></returns>
		public async Task<Response<object>> SaveHistory(EntityHistoryDto history)
		{
			return await _entityHistoryRepository.InsertEntityHistory(history);
		}

		/// <summary>
		/// insert Entity Audit History 
		/// </summary>
		/// <param name="history"></param>
		/// <returns></returns>
		public async Task<Response<int>> InsertEntityAuditHistory(EntityHistoryAuditDto history)
		{
			return await _entityHistoryRepository.InsertEntityAuditHistory(history);
		}

		/// <summary>
		/// Insert Entity Field History Changes 
		/// </summary>
		/// <param name="histories"></param>
		/// <returns></returns>
		public async Task<Response<object>> InsertEntityFieldHistoryChanges(List<EntityFieldHistoryChangeDto> histories)
		{
			return await _entityHistoryRepository
						 .InsertEntityFieldHistoryChanges(histories);
		}

		/// <summary>
		/// GetMeetingHistoryPaged
		/// </summary>
		/// <param name="searchValue"></param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <param name="filterDate"></param>
		/// <param name="filterAction"></param>
		/// <returns></returns>
		public async Task<PagedResponse<EntityHistoryGridDto>> GetMeetingHistoryPaged(string searchValue, int start, int length, string filterDate, string filterModule, string filterAction, int meetingId)
		{
			return await _entityHistoryRepository.GetMeetingHistoryPaged(searchValue, start, length, filterDate, filterModule, filterAction, meetingId);
		}


		/// <summary>
		/// GetAttendeeHistoryByMeeting
		/// </summary>
		/// <param name="meetingId"></param>
		/// <returns></returns>
		public async Task<List<EntityHistoryGridDto>> GetAttendeeHistoryByMeeting(int meetingId)
		{
			return await _entityHistoryRepository.GetAttendeeHistoryByMeeting(meetingId);
		}

		/// <summary>
		/// GetEntityHistoryPaged
		/// </summary>
		/// <param name="searchValue"></param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public async Task<PagedResponse<EntityHistoryGridDto>> GetEntityHistoryPaged(string searchValue, int start, int length, Dictionary<string, string> filters)
		{
			return await _entityHistoryRepository.GetEntityHistoryPaged(
				searchValue, start, length, filters);
		}

		/// <summary>
		/// GetEntityHistoryDetails
		/// </summary>
		/// <param name="entityHistoryId"></param>
		/// <returns></returns>
		public async Task<List<EntityFieldHistoryChangeDto>> GetEntityHistoryDetails(int entityHistoryId)
		{
			return await _entityHistoryRepository.GetEntityHistoryDetails(entityHistoryId);
		}

		/// <summary>
		/// GetAlertSettingHistoryByAttendee
		/// </summary>
		/// <param name="attendeeId"></param>
		/// <returns></returns>
		public async Task<List<EntityHistoryGridDto>> GetAlertSettingHistoryByAttendee(int attendeeId)
		{
			return await _entityHistoryRepository.GetAlertSettingHistoryByAttendee(attendeeId);
		}

		/// <summary>
		/// GetAlertSettingHistoryDetails
		/// </summary>
		/// <param name="alertHistoryId"></param>
		/// <returns></returns>
		public async Task<List<EntityFieldHistoryChangeDto>> GetAlertSettingHistoryDetails(int entityHistoryId)
		{
			return await _entityHistoryRepository.GetallAlertSettingHistoryDetails(entityHistoryId);
		}


		/// <summary>
		/// GetAlertSettingHistoryByMeeting
		/// </summary>
		/// <param name="meetingId"></param>
		/// <returns></returns>
		public async Task<List<EntityHistoryGridDto>> GetAlertSettingHistoryByMeeting(int meetingId)
		{
			return await _entityHistoryRepository.GetAlertSettingHistoryByMeeting(meetingId);
		}

		public async Task<List<MeetingLookupDto>> GetAllMeetingsAsync(int meetingId)
		{
			return await _entityHistoryRepository.GetAllMeetingsAsync(meetingId);
		}


		/// <summary>
		/// GetAttendeetypeAlertSettingHistoryByMeeting
		/// </summary>
		/// <param name="meetingId"></param>
		/// <returns></returns>
		public async Task<List<EntityHistoryGridDto>> GetAttendeetypeAlertSettingHistoryByMeeting(int meetingId)
		{
			return await _entityHistoryRepository.GetAttendeeTypeAlertSettingHistoryByMeeting(meetingId);
		}

        public async Task<IEnumerable<dynamic>> GetRecentActivitiesByMeetingId(int meetingId)
        {
            var meetingHistory = await _entityHistoryRepository.GetRecentActivitiesByMeetingId(meetingId);
            return meetingHistory;
        }

        public async Task<List<string>> GetMeetingNamesByIds(List<int> meetingIds)
        {
            return await _entityHistoryRepository.GetMeetingNamesByIds(meetingIds);
        }
    }
}
