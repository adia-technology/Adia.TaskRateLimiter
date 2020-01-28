using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adia.TaskRateLimiter
{
    /// <summary>
    /// Utility to limit the call rate of functions.
    /// </summary>
    public sealed class RateLimiter
    {
        private readonly TimeSpan _timePeriod;

        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Creates a new instance of RateLimiter.
        /// </summary>
        /// <param name="maxOccurrences">Maximum number of function calls within the provided timePeriod.</param>
        /// <param name="timePeriod">Time period to wait after the call quota is exceeded.</param>
        public RateLimiter(int maxOccurrences, TimeSpan timePeriod)
        {
            if (maxOccurrences < 1)
            {
                throw new ArgumentException("maxOccurrences must be greater or equal to 1", nameof(maxOccurrences));
            }

            if (timePeriod <= TimeSpan.Zero)
            {
                throw new ArgumentException("timePeriod must be greater than 0", nameof(timePeriod));
            }

            _semaphore = new SemaphoreSlim(maxOccurrences);
            _timePeriod = timePeriod;
        }

        /// <summary>
        /// Runs the provided function ensuring that subsequent calls occur no more often than configured.
        /// If the call rate is too high, the function will be queued and run later.
        /// </summary>
        /// <param name="action">Function to run.</param>
        /// <returns>Whatever the function to run returns.</returns>
        public async Task Run(Func<Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                await action().ConfigureAwait(false);
            }
            finally
            {
                _ = Task.Delay(_timePeriod)
                    .ContinueWith(_ => _semaphore.Release(), TaskScheduler.Default);
            }
        }

        /// <summary>
        /// Runs the provided function ensuring that subsequent calls occur no more often than configured.
        /// If the call rate is too high, the function will be queued and run later.
        /// </summary>
        /// <param name="action">Function to run.</param>
        /// <returns>Whatever the function to run returns.</returns>
        public async Task<T> Run<T>(Func<Task<T>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return await action().ConfigureAwait(false);
            }
            finally
            {
                _ = Task.Delay(_timePeriod)
                    .ContinueWith(delayTask => _semaphore.Release(), TaskScheduler.Default);
            }
        }
    }
}
