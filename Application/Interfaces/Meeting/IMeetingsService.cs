using Domain.Meeting;
using Domain.Response;

namespace Application.Interfaces.Meeting
{
    public interface IMeetingsService
    {
        /// <summary>
        /// Add Meetings
        /// </summary>
        /// <param name="meetings"></param>
        /// <returns></returns>
        Task<Response<Meetings>> AddMeetingsAsync(Meetings meetings);

        /// <summary>
        /// Get Meetings Details
        /// </summary>
        /// <param name="meetings"></param>
        /// <returns></returns>
        Task<Response<Meetings>> GetMeetingsDetails(Meetings meetings, int selectedMeetingID);

        /// <summary>
        /// Get All Meetings Details
        /// </summary>
        /// <returns></returns>
        Task<Response<MeetingDetails>> GetAllMeetingsDetails();
        /// <summary>
        /// Get All Archive Meetings Details
        /// </summary>
        /// <returns></returns>
        Task<Response<MeetingDetails>> GetAllArchiveMeetingsDetails();

        /// <summary>
        /// Get  Meetings Details by id
        /// </summary>
        /// <returns></returns>
        Task<Response<MeetingDetails>> GetMeetingById(int id);

        /// <summary>
        /// update  Meetings Details 
        /// </summary>
        /// <returns></returns>
        /// <param name="meetingDetails"></param>
        Task<Response<bool>> UpdateMeeting(MeetingDetails meetingDetails);
    }
}

