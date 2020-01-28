# Adia.TaskRateLimiter

![](https://github.com/adia-technology/Adia.TaskRateLimiter/workflows/Build%20and%20test/badge.svg)

This is a utility to limit the rate of calls to any provided function. When the function is called too frequently, TaskRateLimiter queues it and schedules its execution for a later time

## Installation

Add the package to your project from Visual Studio Package Manager or from the command line, using .NET CLI:

`dotnet add package Adia.TaskRateLimiter`

## Usage

First, create and configure an instance of TaskRateLimiter.
The following example creates an instance configured to allow at most 3 calls per second.

```c#
using Adia.TaskRateLimiter;
var rateLimiter = new RateLimiter(3, TimeSpan.FromSeconds(1));
```

Then, provide your function to the `Run` method of the `rateLimiter`:

```c#

Func<Task> myCode = () =>
{
    //...
}

await rateLimiter.Run(myCode);
```

If `Run` gets called 4 times in a row without any delay, the last execution will wait a second before completing.

```c#
await rateLimiter.Run(myCode); //
await rateLimiter.Run(myCode); //
await rateLimiter.Run(myCode); // these 3 will complete without delays
await rateLimiter.Run(myCode); // this one will wait
```

> The time to wait is counted from the end of the first call.
