using Application.Interfaces.Meeting;
using Dapper;
using Domain.Meeting;
using Domain.Response;
using Domain.User;
using Persistance;
using Persistance.Interfaces.Meeting;
using Persistance.Services.User;
using System.Data;
using System.Data.Common;
using System.Net;

namespace Application.Services.Meeting
{
    public class MeetingsService(IMeetingsRepository meetingsRepository, DapperContext context) : IMeetingsService
    {
        private readonly IMeetingsRepository meetingsRepository = meetingsRepository;
        private readonly DapperContext _context = context;

        /// <summary>
        /// Add Meetings
        /// </summary>
        /// <param name="meetings"></param>
        /// <returns></returns>
        public async Task<Response<Meetings>> AddMeetingsAsync(Meetings meetings)
        {
            return await this.meetingsRepository.Insert(meetings);
        }

        /// <summary>
        /// Get Meetings Details
        /// </summary>
        /// <param name="meetings"></param>
        /// <returns></returns>
        public async Task<Response<Meetings>> GetMeetingsDetails(Meetings meetings, int selectedMeetingID)
        {
            var filters = new Dictionary<string, object>();

            if (selectedMeetingID > 0)
            {
                filters.Add("Id", selectedMeetingID);
            }
            else
            {
                filters.Add("MeetingName", meetings.MeetingName);
            }

            filters.Add("IsActive", meetings.IsActive);
            filters.Add("IsDeleted", meetings.IsDeleted);
            return await meetingsRepository.GetAllAsync(filters).ConfigureAwait(false);

        }

        /// <summary>
        /// Get All Meetings Details
        /// </summary>
        /// <returns></returns>
        public async Task<Response<MeetingDetails>> GetAllMeetingsDetails()
        {
            return await meetingsRepository.GetAllMeetingsDetails();
        }
        /// <summary>
        /// Get All Archive Meetings Details
        /// </summary>
        /// <returns></returns>
        public async Task<Response<MeetingDetails>> GetAllArchiveMeetingsDetails()
        {
            return await meetingsRepository.GetAllArchiveMeetingsDetails();

        }

        /// <summary>
        /// Get  Meetings Details by id
        /// </summary>
        /// <returns></returns>
        public async Task<Response<MeetingDetails>> GetMeetingById(int id)
        {
            return await meetingsRepository.GetMeetingById(id);
        }

        /// <summary>
        /// update  Meetings Details 
        /// </summary>
        /// <returns></returns>
        /// <param name="meetingDetails"></par
        public async Task<Response<bool>> UpdateMeeting(MeetingDetails meetingDetails)
        {
            return await meetingsRepository.UpdateMeeting(meetingDetails);
        }

    }
}
