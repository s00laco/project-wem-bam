namespace WemBam.Models
{
    public class AudioStreamRequest
    {
        public SourceType SourceType { get; init; }

        public string? ContainerPath { get; init; }

        public string AssetPath { get; init; } = string.Empty;
    }
}