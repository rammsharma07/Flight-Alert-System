using Application.Interfaces.History;
using ClosedXML.Excel;
using Domain.History;
using Domain.Response;
using Persistance.Interfaces.History;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Services.History
{
    public class NotificationHistoryService(INotificationHistory repo) : INotificationHistoryService
    {
        private readonly INotificationHistory _repo = repo;

        public async Task<Response<NotificationHistory>> GetNotificationHistory(NotifQueryDto dto)
        {
            return await _repo.GetNotificationHistory(dto);
        }

        public async Task<List<string>> GetDistinctMeetingsAsync()
        {
            return await _repo.GetDistinctMeetingsAsync();
        }

        public async Task<(byte[] FileData, int rowCount)> ExportNotificationsAsync(NotifQueryDto dto)
        {
            dto.IsExport = true;

            var result = await GetNotificationHistory(dto);

            var rows = result.Data.Select(item => new
            {
                MeetingName = item.MeetingName ?? "-",
                Attendee = item.UserName ?? "-",
                FlightNumber = item.FlightNumber ?? "-",
                DepartureDateTime = item.DepartureDateTime?.ToString("dd MMM yyyy, hh:mm tt") ?? "-",
                NotificationTrigger = item.SkipReason ?? "-",
                Source = FormatSource(item.Trigger ?? "-"),
                EmailSent = item.EmailSent == true ? "Yes" : "No",
                SmsSent = item.SmsSent == true ? "Yes" : "No",
                OldStatus = FormatFlightLabel(item.StatusFrom ?? "-"),
                NewStatus = FormatFlightLabel(item.StatusTo ?? "-"),
                OldState = FormatFlightLabel(item.StateFrom ?? "-"),
                NewState = FormatFlightLabel(item.StateTo ?? "-"),
                EmailSentAlertSetting = item.EmailAlert == true ? "ON" : "OFF",
                SmsSentAlertSetting = item.SmsAlert == true ? "ON" : "OFF",
                EmailStatusAlertSetting = item.EmailStatus ?? "-",
                EmailStateAlertSetting = item.EmailState ?? "-",
                SmsStatusAlertSetting = item.SmsStatus ?? "-",
                SmsStateAlertSetting = item.SmsState ?? "-"
            }).ToList();

            int rowCount = rows.Count;
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Notifications");

            var table = ws.Cell(1, 1).InsertTable(rows);
            table.Theme = XLTableTheme.None;
            table.ShowHeaderRow = true;
            table.HeadersRow().Style.Font.Bold = true;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            return (stream.ToArray(), rowCount);
        }

        private string FormatSource(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "-";

            text = text.Replace("_", " ");
            text = Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");

            return text;
        }

        private string FormatFlightLabel(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "-";

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "ontime", "OnTime" },
        { "ingate", "InGate" },
        { "outgate", "OutGate" },
        { "landed", "Landed" },
        { "inair", "InAir" },
        { "canceled", "Cancelled" },
        { "delayed", "Delayed" },
        { "scheduled", "Scheduled" },
        { "early", "Early" }
    };

            var cleaned = text.Replace(" ", "").Replace("_", "").ToLower();

            if (map.ContainsKey(cleaned))
                return map[cleaned];

            return text;
        }

        /// <summary>
        /// SaveExport Service 
        /// </summary>
        /// <param name="historyType"></param>
        /// <param name="fileName"></param>
        /// <param name="scope"></param>
        /// <param name="recordCount"></param>
        /// <param name="meetingNames"></param>
        /// <param name="filtersJson"></param>
        /// <param name="userId"></param>
        /// <param name="dataSummary"></param>
        /// <returns></returns>
        public async Task SaveExport(
          string historyType,
          string fileName,
          string scope,
          int recordCount,
          string meetingNames,
          string filtersJson,
          int? userId,
          string dataSummary)
        {
            var entity = new ExportHistory
            {
                HistoryType = historyType,
                FileName = fileName,
                Scope = scope,
                RecordCount = recordCount,
                MeetingsName = meetingNames,
                Filters = filtersJson,
                DataSummary = dataSummary,
                UserId = userId,
                ExportedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
        }

        /// <summary>
        /// GetPagedExportHistory
        /// </summary>
        /// <param name="search"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public async Task<(List<ExportHistory> Data, int TotalCount)>
            GetPagedExportHistory(string search, int start, int length, string sortColumn, string sortDirection)
        {
            return await _repo.GetPagedAsync(search, start, length, sortColumn ,sortDirection);
        }

        /// <summary>
        /// GetMeetingNameById
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public async Task<string> GetMeetingNameById(int meetingId)
        {
            return await _repo.GetMeetingNameById(meetingId);
        }
    }
}
