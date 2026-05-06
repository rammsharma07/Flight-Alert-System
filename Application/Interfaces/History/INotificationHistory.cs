using Domain.History;
using Domain.Response;

namespace Application.Interfaces.History
{
    public interface INotificationHistoryService
    {
        Task<Response<NotificationHistory>> GetNotificationHistory(NotifQueryDto dto);
        Task<List<string>> GetDistinctMeetingsAsync();  
        Task<(byte[] FileData, int rowCount)> ExportNotificationsAsync(NotifQueryDto dto);
        Task SaveExport(string historyType, string fileName, string scope, int recordCount, string meetingNames, string filtersJson, int? userId, string dataSummary);
        Task<(List<ExportHistory> Data, int TotalCount)> GetPagedExportHistory(string search, int start, int length, string sortColumn, string sortDirection);
        Task<string> GetMeetingNameById(int meetingId);
    }
}
