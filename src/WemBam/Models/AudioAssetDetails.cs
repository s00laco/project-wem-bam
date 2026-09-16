using System;
using System.Collections.Generic;

namespace WemBam.Models
{
    public class AudioAssetDetails
    {
        public AudioAsset AudioAsset { get; set; } = new();

        public string WwisePath { get; set; } = string.Empty;

        public IReadOnlyList<AudioAssetSource> Sources { get; set; } =
            Array.Empty<AudioAssetSource>();

        public IReadOnlyList<WwiseEvent> WwiseEvents { get; set; } =
            Array.Empty<WwiseEvent>();

        public IReadOnlyList<AudioCategory> Categories { get; set; } =
            Array.Empty<AudioCategory>();

        public bool IsLooped { get; set; }
    }
}