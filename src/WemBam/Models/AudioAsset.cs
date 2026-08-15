namespace WemBam.Models
{
    public class AudioAsset
    {
        public long Id { get; set; }

        public string? FileId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileExtension { get; set; } = string.Empty;

        public int? Duration { get; set; }

        public long? DefaultSourceId { get; set; }
    }
}