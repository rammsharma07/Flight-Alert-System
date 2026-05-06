namespace Application.Interfaces.Utils
{
    /// <summary>
    /// Service for tracking and preventing duplicate message processing
    /// </summary>
    public interface ISequenceNumberManager
    {
        /// <summary>
        /// Checks if message was already processed
        /// </summary>
        /// <param name="messageId">Unique message identifier</param>
        /// <returns>True if already processed, false otherwise</returns>
        bool IsProcessed(string messageId);

        /// <summary>
        /// Marks message as processed to prevent reprocessing
        /// </summary>
        /// <param name="messageId">Unique message identifier</param>
        void MarkProcessed(string messageId);

        /// <summary>
        /// Gets count of processed messages in memory
        /// </summary>
        /// <returns>Number of tracked message IDs</returns>
        int GetProcessedCount();
    }
}
