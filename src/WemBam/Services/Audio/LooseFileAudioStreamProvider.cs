using System.IO;
using WemBam.Contracts;
using WemBam.Models;

namespace WemBam.Services.Audio
{
    public class LooseFileAudioStreamProvider : IAudioStreamProvider
    {
        public Stream OpenStream(
            AudioStreamRequest request)
        {
            return new FileStream(
                request.AssetPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
        }
    }
}