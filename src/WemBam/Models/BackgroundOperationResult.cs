namespace WemBam.Models
{
    public class BackgroundOperationResult
    {
        public BackgroundTaskState State { get; init; }

        public int ItemsProcessed { get; init; }

        public int WarningCount { get; init; }

        public string? Message { get; init; }

        public static BackgroundOperationResult Completed(
            int itemsProcessed,
            string? message = null)
        {
            return new BackgroundOperationResult
            {
                State = BackgroundTaskState.Completed,
                ItemsProcessed = itemsProcessed,
                WarningCount = 0,
                Message = message
            };
        }

        public static BackgroundOperationResult CompletedWithWarnings(
            int itemsProcessed,
            int warningCount,
            string? message = null)
        {
            return new BackgroundOperationResult
            {
                State = BackgroundTaskState.CompletedWithWarnings,
                ItemsProcessed = itemsProcessed,
                WarningCount = warningCount,
                Message = message
            };
        }

        public static BackgroundOperationResult Cancelled(
            int itemsProcessed = 0,
            int warningCount = 0,
            string? message = null)
        {
            return new BackgroundOperationResult
            {
                State = BackgroundTaskState.Cancelled,
                ItemsProcessed = itemsProcessed,
                WarningCount = warningCount,
                Message = message
            };
        }

        public static BackgroundOperationResult Failed(
            string? message = null)
        {
            return new BackgroundOperationResult
            {
                State = BackgroundTaskState.Failed,
                Message = message
            };
        }
    }
}