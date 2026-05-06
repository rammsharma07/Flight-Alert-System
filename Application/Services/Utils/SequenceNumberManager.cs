using Application.Interfaces.Utils;

namespace Application.Services.Utils
{
    /// <summary>
    /// Manages message deduplication using in-memory HashSet
    /// Prevents duplicate message processing in distributed systems
    /// </summary>
    public class SequenceNumberManager : ISequenceNumberManager
    {
        // In-memory storage for processed message IDs
        private readonly HashSet<string> _processedMessageIds;

        // Maximum cache size before clearing (prevents memory overflow)
        private readonly int _maxSize = 10000;

        public SequenceNumberManager()
        {
            // Step 1: Initialize empty HashSet for fast lookups
            _processedMessageIds = new HashSet<string>();
        }

        /// <summary>
        /// Checks if message was already processed
        /// </summary>
        /// <param name="messageId">Unique message identifier</param>
        /// <returns>True if already processed, false otherwise</returns>
        public bool IsProcessed(string messageId)
        {
            // Step 1: Validate input - reject null/empty IDs
            if (string.IsNullOrEmpty(messageId))
                return false;

            // Step 2: Check if ID exists in HashSet (O(1) lookup)
            return _processedMessageIds.Contains(messageId);
        }

        /// <summary>
        /// Marks message as processed to prevent reprocessing
        /// </summary>
        /// <param name="messageId">Unique message identifier</param>
        public void MarkProcessed(string messageId)
        {
            // Step 1: Validate input - ignore null/empty IDs
            if (string.IsNullOrEmpty(messageId))
                return;

            // Step 2: Check cache size limit to prevent memory overflow
            if (_processedMessageIds.Count >= _maxSize)
            {
                // Step 3: Clear entire cache when max size reached
                // Note: This resets deduplication - consider using LRU cache for production
                _processedMessageIds.Clear();
            }

            // Step 4: Add message ID to processed set
            _processedMessageIds.Add(messageId);
        }

        /// <summary>
        /// Gets count of processed messages in memory
        /// </summary>
        /// <returns>Number of tracked message IDs</returns>
        public int GetProcessedCount()
        {
            // Step 1: Return current count of tracked IDs
            return _processedMessageIds.Count;
        }
    }
}
