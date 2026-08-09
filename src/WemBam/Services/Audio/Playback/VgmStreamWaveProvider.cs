using System;
using NAudio.Wave;
using WemBam.Services.Audio.Interop;

namespace WemBam.Services.Audio.Playback
{
    /// <summary>
    /// Adapts a <see cref="LibVgmStream"/> decoder to NAudio's
    /// <see cref="IWaveProvider"/> interface.
    /// </summary>
    internal sealed class VgmStreamWaveProvider : IWaveProvider
    {
        private readonly LibVgmStream _decoder;

        private readonly short[] _sampleBuffer;

        public VgmStreamWaveProvider(
            LibVgmStream decoder)
        {
            ArgumentNullException.ThrowIfNull(decoder);

            _decoder = decoder;

            WaveFormat = new WaveFormat(
                _decoder.SampleRate,
                16,
                _decoder.Channels);

            _sampleBuffer = new short[4096 * _decoder.Channels];
        }

        public WaveFormat WaveFormat
        {
            get;
        }

        public int Read(
            byte[] buffer,
            int offset,
            int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(offset));
            }

            if (count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count));
            }

            int bytesWritten = 0;

            while (bytesWritten < count)
            {
                int remainingBytes =
                    count - bytesWritten;

                int requestedSamples =
                    Math.Min(
                        remainingBytes /
                            (_decoder.Channels * sizeof(short)),
                        _sampleBuffer.Length / _decoder.Channels);

                if (requestedSamples == 0)
                {
                    break;
                }

                int decodedSamples =
                    _decoder.Decode(
                        _sampleBuffer.AsSpan(
                            0,
                            requestedSamples * _decoder.Channels));

                int bytesDecoded =
                    decodedSamples * sizeof(short);

                if (bytesDecoded == 0)
                {
                    break;
                }

                int bytesToCopy =
                    Math.Min(
                        bytesDecoded,
                        remainingBytes);

                Buffer.BlockCopy(
                    _sampleBuffer,
                    0,
                    buffer,
                    offset + bytesWritten,
                    bytesToCopy);

                bytesWritten += bytesToCopy;
            }

            return bytesWritten;
        }
    }
}