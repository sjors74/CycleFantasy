namespace CycleManager.Domain.Models
{
    [Obsolete("This enum is deprecated. Newer version in Domain/Enums")]
    public enum ScrapeStatus
    {
        Pending = 0,
        Partial = 1,
        Completed = 2,
        Failed = 3,
        Skipped = 4
    }
}
