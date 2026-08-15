namespace WemBam.Models
{
    public class AudioAssetSource
    {
        public long Id { get; set; }

        public long AudioAssetId { get; set; }

        public long SourceId { get; set; }

        public string? ContainerPath { get; set; }

        public string AssetPath { get; set; } = string.Empty;

        public string? ContentHash { get; set; }
    }
}