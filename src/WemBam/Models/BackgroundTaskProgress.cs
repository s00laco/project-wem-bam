using System;

namespace WemBam.Models
{
    public class BackgroundTaskProgress
    {
        public int ItemsProcessed { get; init; }

        public int? TotalItems { get; init; }

        public string StatusMessage { get; init; } = string.Empty;

        public DateTimeOffset StartedAt { get; init; }

        public TimeSpan Elapsed => DateTimeOffset.UtcNow - StartedAt;

        public double? PercentageComplete
        {
            get
            {
                if (TotalItems is null || TotalItems <= 0)
                {
                    return null;
                }

                return (double)ItemsProcessed / TotalItems.Value * 100.0;
            }
        }
    }
}