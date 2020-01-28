using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Adia.TaskRateLimiter.Tests
{
    public class Tests
    {
        [Test]
        [MaxTime(2000)]
        public async Task RunFunctions_ShouldExecuteAtMostNRequestsInGivenPeriod()
        {
            var counter = 0;

            Func<Task> action = () => {
                Interlocked.Increment(ref counter);
                Console.WriteLine("Running");
                return Task.CompletedTask;
            };

            var tasks = new List<Task>();

            var throttler = new RateLimiter(2, TimeSpan.FromSeconds(0.5));

            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));

            counter.ShouldBe(2);
            Console.WriteLine("Working...");

            await Task.Delay(TimeSpan.FromSeconds(0.75));

            counter.ShouldBe(4);

            await Task.WhenAll(tasks);

            counter.ShouldBe(5);
        }

        [Test]
        public async Task RunActions_ShouldExecuteAtMostNRequestsInGivenPeriod()
        {
            var stopwatch = new Stopwatch();

            Func<Task<int>> action = () => Task.FromResult((int)stopwatch.ElapsedMilliseconds);

            var throttler = new RateLimiter(2, TimeSpan.FromMilliseconds(250));
            stopwatch.Start();

            var tasks = new List<Task<int>>();
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));

            await Task.WhenAll(tasks);

            var results = tasks.Select(t => t.Result).ToList();

            results[0].ShouldBeLessThan(250);
            results[1].ShouldBeLessThan(250);
            results[2].ShouldBeGreaterThan(240); // less than 250 as delays in .NET are not exact
            results[3].ShouldBeGreaterThan(240);
            results[4].ShouldBeGreaterThan(480);
        }

        [Test]
        [MaxTime(2000)]
        public async Task RunFunctions_ShouldNotFailWhenTasksThrowExceptionsAsynchronously()
        {
            var counter = 0;

            Func<Task> action = async () => {
                Interlocked.Increment(ref counter);
                await Task.Delay(10);
                throw new Exception("Boom");
            };

            var tasks = new List<Task>();

            var throttler = new RateLimiter(2, TimeSpan.FromSeconds(0.5));

            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));
            tasks.Add(throttler.Run(action));

            try
            {
                await Task.WhenAll(tasks);
            }
            catch
            {
                counter.ShouldBe(5);
            }
        }
    }
}
