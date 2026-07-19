using System;
using System.IO;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Archives;
using Noggog;
using WemBam.Contracts;
using WemBam.Models;

namespace WemBam.Services.Audio
{
    public class BA2AudioStreamProvider : IAudioStreamProvider
    {
        public Stream OpenStream(
            AudioStreamRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ContainerPath))
            {
                throw new ArgumentException(
                    "ContainerPath is required for BA2 audio assets.",
                    nameof(request.ContainerPath));
            }

            IArchiveReader archive =
                Archive.CreateReader(
                    GameRelease.Starfield,
                    new FilePath(request.ContainerPath));

            IArchiveFile file = archive.Files.FirstOrDefault(
                file => string.Equals(
                    file.Path,
                    request.AssetPath,
                    StringComparison.OrdinalIgnoreCase))
                ?? throw new FileNotFoundException(
                    $"Asset '{request.AssetPath}' was not found in archive '{request.ContainerPath}'.");

            return file.AsStream();
        }
    }
}