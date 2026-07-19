using WemBam.Contracts;
using WemBam.Models;

namespace WemBam.Services.Audio
{
    public static class AudioStreamProviderFactory
    {
        public static IAudioStreamProvider Create(
            AudioStreamRequest request)
        {
            return request.ContainerPath switch
            {
                null => new LooseFileAudioStreamProvider(),
                _ => new BA2AudioStreamProvider()
            };
        }
    }
}