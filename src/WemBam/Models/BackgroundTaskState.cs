namespace WemBam.Models
{
    public enum BackgroundTaskState
    {
        Idle,
        Running,
        Cancelling,
        Completed,
        CompletedWithWarnings,
        Cancelled,
        Failed
    }
}