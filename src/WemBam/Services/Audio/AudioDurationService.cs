using System;
using System.IO;
using WemBam.Contracts;
using WemBam.Logging;
using WemBam.Models;
using WemBam.Services.Audio.Interop;

namespace WemBam.Services.Audio
{
    internal static class AudioDurationService
    {
        public static int? GetDuration(
            AudioStreamRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                IAudioStreamProvider streamProvider =
                    AudioStreamProviderFactory.Create(request);

                using Stream stream =
                    streamProvider.OpenStream(request);

                using LibVgmStream decoder =
                    new(
                        stream,
                        request.AssetPath);

                int sampleRate = decoder.SampleRate;
                long streamSamples = decoder.StreamSamples;

                if (sampleRate <= 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid sample rate: {sampleRate}.");
                }

                if (streamSamples < 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid stream sample count: {streamSamples}.");
                }

                return (int)(streamSamples / (double)sampleRate);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    $"Failed to determine duration for '{request.AssetPath}'.");

                return null;
            }
        }
    }
}