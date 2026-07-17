namespace WemBam.Models
{
    public class AudioAsset
    {
        public long SourceId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileExtension { get; set; } = string.Empty;

        public string? ContainerPath { get; set; }

        public string AssetPath { get; set; } = string.Empty;

        public int? Duration { get; set; }
    }
}