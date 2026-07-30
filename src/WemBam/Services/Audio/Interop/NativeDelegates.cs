using System;
using System.Runtime.InteropServices;

namespace WemBam.Services.Audio.Interop
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate int libstreamfile_read_delegate(
        IntPtr user_data,
        byte* dst,
        long offset,
        int length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long libstreamfile_get_size_delegate(
        IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate byte* libstreamfile_get_name_delegate(
        IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate libstreamfile_t* libstreamfile_open_delegate(
        IntPtr user_data,
        byte* filename);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void libstreamfile_close_delegate(
        libstreamfile_t* libsf);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void libvgmstream_log_delegate(
        int level,
        byte* message);
}