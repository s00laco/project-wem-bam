using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WemBam.Services.Audio.Interop
{
    internal unsafe sealed class LibVgmStream : IDisposable
    {
        private readonly ManagedStreamFileAdapter _streamFileAdapter;

        private IntPtr _handle;

        private bool _disposed;

        public LibVgmStream(
            Stream stream,
            string fileName)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(fileName);

            _streamFileAdapter = new ManagedStreamFileAdapter(
                stream,
                fileName);

            _handle = (IntPtr)NativeMethods.libvgmstream_create(
                _streamFileAdapter.StreamFile,
                0,
                null);

            if (_handle == IntPtr.Zero)
            {
                _streamFileAdapter.Dispose();

                throw new InvalidOperationException(
                    $"libvgmstream failed to open '{fileName}'.");
            }
        }

        internal IntPtr Handle
        {
            get
            {
                ThrowIfDisposed();

                return _handle;
            }
        }

        internal int Decode(
            Span<short> destination)
        {
            ThrowIfDisposed();

            libvgmstream_t* lib =
                (libvgmstream_t*)_handle;

            fixed (short* buffer = destination)
            {
                int result =
                    NativeMethods.libvgmstream_fill(
                        lib,
                        buffer,
                        destination.Length);

                if (result < 0)
                {
                    throw new InvalidOperationException(
                        "libvgmstream failed to decode audio.");
                }
            }

            if (lib->decoder is null)
            {
                throw new InvalidOperationException(
                    "libvgmstream did not provide decoder state.");
            }

            return lib->decoder->buf_samples;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_handle != IntPtr.Zero)
            {
                NativeMethods.libvgmstream_free(
                    (libvgmstream_t*)_handle);

                _handle = IntPtr.Zero;
            }

            _streamFileAdapter.Dispose();

            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(
                _disposed,
                this);
        }
    }
}