using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace JakePerry.Threading.Tasks
{
    /// <summary>
    /// Simple utility class that allows code to yield once a time threshold has elapsed.
    /// </summary>
    internal sealed class TimeYielder
    {
        private readonly long m_yieldThreshold;
        private long m_timestamp;

        private static long Timestamp => Stopwatch.GetTimestamp();

        /// <summary>
        /// Indicates whether the time threshold has been reached.
        /// </summary>
        public bool WantsToYield => Timestamp - m_timestamp > m_yieldThreshold;

        /// <summary>
        /// Create a new instance with a given time threshold.
        /// </summary>
        /// <param name="threshold">
        /// Time threshold in ticks.
        /// </param>
        public TimeYielder(long threshold)
        {
            m_yieldThreshold = threshold;
            m_timestamp = Timestamp;
        }

        /// <param name="threshold">
        /// Time threshold.
        /// </param>
        /// <inheritdoc cref="TimeYielder(long)"/>
        public TimeYielder(TimeSpan threshold) : this(threshold.Ticks) { }

        /// <summary>
        /// Yields, then updates the internal timestamp upon continuation.
        /// </summary>
        public async Task Yield()
        {
            await Task.Yield();
            m_timestamp = Timestamp;
        }

        /// <summary>
        /// Yields only if the time threshold has been reached.
        /// </summary>
        /// <returns>
        /// Invokes and returns <see cref="Yield"/> if the threshold has been reached;
        /// Otherwise, returns a completed task.
        /// </returns>
        public Task YieldOptional()
        {
            return WantsToYield ? Yield() : Task.CompletedTask;
        }
    }
}
