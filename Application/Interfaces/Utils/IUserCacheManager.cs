using Domain.Meeting;

namespace Application.Interfaces.Utils
{
    /// <summary>
    /// Service for managing MeetingUser cache with alert ID indexing
    /// </summary>
    public interface IUserCacheManager
    {
        /// <summary>
        /// Retrieves cached users by alert ID
        /// </summary>
        /// <param name="alertId">Alert identifier</param>
        /// <returns>List of users or null if not cached</returns>
        List<MeetingUser>? GetUsersByAlertId(string alertId);

        /// <summary>
        /// Stores users in cache with alert ID key
        /// </summary>
        /// <param name="alertId">Alert identifier</param>
        /// <param name="users">List of users to cache</param>
        void SetUsersByAlertId(string alertId, List<MeetingUser> users);

        /// <summary>
        /// Updates specific user in cached alert list
        /// </summary>
        /// <param name="alertId">Alert identifier</param>
        /// <param name="updatedUser">User with updated data</param>
        void UpdateUserInCache(string alertId, MeetingUser updatedUser);

        /// <summary>
        /// Removes alert from cache
        /// </summary>
        /// <param name="alertId">Alert identifier to remove</param>
        void InvalidateAlertId(string alertId);

        /// <summary>
        /// Gets total number of cached entries
        /// </summary>
        /// <returns>Cache entry count</returns>
        int GetCacheSize();
    }
}
