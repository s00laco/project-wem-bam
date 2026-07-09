using System;

namespace WemBam.Services
{
    public sealed class IndexStatusManager
    {
        public static IndexStatusManager Instance { get; } = new();

        private IndexStatusManager()
        {
            Status = "Not indexed";
        }

        public string Status { get; private set; }

        public DateTimeOffset? LastIndexed { get; private set; }

        public int IndexedFileCount { get; private set; }

        public void SetIndexing()
        {
            Status = "Indexing";
        }

        public void SetUpToDate(
            int indexedFileCount)
        {
            Status = "Up to date";
            LastIndexed = DateTimeOffset.UtcNow;
            IndexedFileCount = indexedFileCount;
        }

        public void SetPartiallyIndexed(
            int indexedFileCount)
        {
            Status = "Partially indexed";
            LastIndexed = DateTimeOffset.UtcNow;
            IndexedFileCount = indexedFileCount;
        }

        public void SetOutOfDate()
        {
            Status = "Out of date";
        }

        public void PreserveCurrentState()
        {
            // Intentionally does nothing.
            // Used when an indexing operation is cancelled
            // or fails so the previous successful state
            // remains unchanged.
        }
    }
}