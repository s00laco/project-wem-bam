using System.IO;
using WemBam.Models;

namespace WemBam.Contracts
{
    public interface IAudioStreamProvider
    {
        Stream OpenStream(AudioStreamRequest request);
    }
}