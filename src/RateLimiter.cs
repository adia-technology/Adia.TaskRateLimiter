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
            try
            {
                await _semaphore.WaitAsync();
                Console.WriteLine("Lock aquired. Resource count: " + _semaphore.CurrentCount);
                await action();
            }
            finally
            {
                Console.WriteLine("Scheduling release of a semaphore");
                _ = Task.Delay(_timePeriod).ContinueWith(_ => {
                    _semaphore.Release();
                    Console.WriteLine("Semaphore released. Resource count: " + _semaphore.CurrentCount);
                });
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
            try
            {
                await _semaphore.WaitAsync();
                return await action();
            }
            finally
            {
                _ = Task.Delay(_timePeriod).ContinueWith(delayTask => { _semaphore.Release(); });
            }
        }
    }
}
