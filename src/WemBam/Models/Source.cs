namespace WemBam.Models
{
    public enum SourceType
    {
        Folder,
        File
    }

    public class Source
    {
        public long Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public SourceType Type { get; set; }

        public bool Enabled { get; set; } = true;

        public DateTimeOffset DateAdded { get; set; }
    }
}