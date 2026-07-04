namespace WemBam.Models
{
    public class Source
    {
        public string DisplayName { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public SourceType Type { get; set; }
    }

    public enum SourceType
    {
        Folder,
        File
    }
}