using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WemBam.Services.Audio.Interop
{
    /// <summary>
    /// Adapts a managed <see cref="Stream"/> to libvgmstream's
    /// <see cref="libstreamfile_t"/> callback interface.
    /// </summary>
    ///
    /// <remarks>
    /// This class owns the supplied stream for its lifetime.
    /// Disposing the adapter also disposes the underlying stream.
    /// </remarks>
    internal unsafe sealed class ManagedStreamFileAdapter : IDisposable
    {
        private readonly Stream _stream;

        private readonly GCHandle _thisHandle;
        private readonly GCHandle _filenameHandle;

        private readonly byte[] _filenameBytes;

        private readonly libstreamfile_read_delegate _readDelegate;
        private readonly libstreamfile_get_size_delegate _getSizeDelegate;
        private readonly libstreamfile_get_name_delegate _getNameDelegate;
        private readonly libstreamfile_open_delegate _openDelegate;
        private readonly libstreamfile_close_delegate _closeDelegate;

        private readonly libstreamfile_t* _streamFile;

        private bool _disposed;

        public ManagedStreamFileAdapter(
            Stream stream,
            string filename)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(filename);

            if (!stream.CanRead)
            {
                throw new ArgumentException(
                    "Stream must be readable.",
                    nameof(stream));
            }

            if (!stream.CanSeek)
            {
                throw new ArgumentException(
                    "Stream must be seekable.",
                    nameof(stream));
            }

            _stream = stream;

            _filenameBytes =
                Encoding.UTF8.GetBytes(filename + '\0');

            _filenameHandle =
                GCHandle.Alloc(
                    _filenameBytes,
                    GCHandleType.Pinned);

            _thisHandle =
                GCHandle.Alloc(this);

            _readDelegate = Read;
            _getSizeDelegate = GetSize;
            _getNameDelegate = GetName;
            _openDelegate = Open;
            _closeDelegate = Close;

            _streamFile =
                (libstreamfile_t*)Marshal.AllocHGlobal(
                    sizeof(libstreamfile_t));

            *_streamFile = new libstreamfile_t
            {
                user_data =
                    GCHandle.ToIntPtr(_thisHandle),

                read =
                    Marshal.GetFunctionPointerForDelegate(
                        _readDelegate),

                get_size =
                    Marshal.GetFunctionPointerForDelegate(
                        _getSizeDelegate),

                get_name =
                    Marshal.GetFunctionPointerForDelegate(
                        _getNameDelegate),

                open =
                    Marshal.GetFunctionPointerForDelegate(
                        _openDelegate),

                close =
                    Marshal.GetFunctionPointerForDelegate(
                        _closeDelegate)
            };
        }

        public libstreamfile_t* StreamFile
        {
            get
            {
                ThrowIfDisposed();
                return _streamFile;
            }
        }

        private static ManagedStreamFileAdapter FromUserData(
            IntPtr userData)
        {
            GCHandle handle =
                GCHandle.FromIntPtr(userData);

            return (ManagedStreamFileAdapter)handle.Target!;
        }

        private static int Read(
            IntPtr userData,
            byte* destination,
            long offset,
            int length)
        {
            ManagedStreamFileAdapter adapter =
                FromUserData(userData);

            if (length <= 0)
            {
                return 0;
            }

            lock (adapter._stream)
            {
                adapter._stream.Position = offset;

                Span<byte> destinationSpan =
                    new(destination, length);

                return adapter._stream.Read(destinationSpan);
            }
        }

        private static long GetSize(
            IntPtr userData)
        {
            return FromUserData(userData)
                ._stream.Length;
        }

        private static byte* GetName(
            IntPtr userData)
        {
            ManagedStreamFileAdapter adapter =
                FromUserData(userData);

            return (byte*)adapter
                ._filenameHandle
                .AddrOfPinnedObject();
        }

        private static libstreamfile_t* Open(
            IntPtr userData,
            byte* filename)
        {
            ManagedStreamFileAdapter adapter =
                FromUserData(userData);

            // NOTE:
            // libvgmstream may request a reopened streamfile.
            //
            // For the current milestone the existing streamfile
            // instance is returned. If future testing determines
            // that independent reopen semantics are required,
            // this callback can be expanded to construct a new
            // adapter over an equivalent managed stream.

            return adapter._streamFile;
        }

        private static void Close(
            libstreamfile_t* streamFile)
        {
            // Intentionally empty.
            //
            // Resource ownership is managed by the adapter's
            // Dispose() implementation rather than by the native
            // callback.
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _stream.Dispose();

            if (_thisHandle.IsAllocated)
            {
                _thisHandle.Free();
            }

            if (_filenameHandle.IsAllocated)
            {
                _filenameHandle.Free();
            }

            if (_streamFile != null)
            {
                Marshal.FreeHGlobal(
                    (IntPtr)_streamFile);
            }
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(
                _disposed,
                this);
        }
    }
}