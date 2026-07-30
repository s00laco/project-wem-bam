using System.Runtime.InteropServices;

namespace WemBam.Services.Audio.Interop
{
    internal static unsafe class NativeMethods
    {
        private const string LibraryName = "libvgmstream";

        #region Version

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint libvgmstream_get_version();

        #endregion

        #region Lifetime

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern libvgmstream_t* libvgmstream_init();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void libvgmstream_free(
            libvgmstream_t* lib);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern libvgmstream_t* libvgmstream_create(
            libstreamfile_t* libsf,
            int subsong,
            libvgmstream_config_t* cfg);

        #endregion

        #region Configuration

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void libvgmstream_setup(
            libvgmstream_t* lib,
            libvgmstream_config_t* cfg);

        #endregion

        #region Stream

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int libvgmstream_open_stream(
            libvgmstream_t* lib,
            libstreamfile_t* libsf,
            int subsong);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void libvgmstream_close_stream(
            libvgmstream_t* lib);

        #endregion

        #region Playback

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int libvgmstream_render(
            libvgmstream_t* lib);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int libvgmstream_fill(
            libvgmstream_t* lib,
            void* buffer,
            int bufferSamples);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern long libvgmstream_get_play_position(
            libvgmstream_t* lib);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void libvgmstream_seek(
            libvgmstream_t* lib,
            long sample);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void libvgmstream_reset(
            libvgmstream_t* lib);

        #endregion

        #region Logging

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void libvgmstream_set_log(
            libvgmstream_loglevel_t level,
            libvgmstream_log_delegate callback);

        #endregion

        #region Extensions

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern byte** libvgmstream_get_extensions(
            int* size);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern byte** libvgmstream_get_common_extensions(
            int* size);

        #endregion

        #region Validation

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool libvgmstream_is_valid(
            byte* filename,
            libvgmstream_valid_t* cfg);

        #endregion

        #region Metadata

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int libvgmstream_get_title(
            libvgmstream_t* lib,
            libvgmstream_title_t* cfg,
            byte* buffer,
            int bufferLength);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int libvgmstream_format_describe(
            libvgmstream_t* lib,
            byte* buffer,
            int bufferLength);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool libvgmstream_is_virtual_filename(
            byte* filename);

        #endregion

        #region Tags

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern libvgmstream_tags_t* libvgmstream_tags_init(
            libstreamfile_t* libsf);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void libvgmstream_tags_find(
            libvgmstream_tags_t* tags,
            byte* targetFilename);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool libvgmstream_tags_next_tag(
            libvgmstream_tags_t* tags);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void libvgmstream_tags_free(
            libvgmstream_tags_t* tags);

        #endregion

        #region StreamFile Helpers

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void libstreamfile_close(
            libstreamfile_t* libsf);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern libstreamfile_t* libstreamfile_open_from_stdio(
            byte* filename);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern libstreamfile_t* libstreamfile_open_from_file(
            IntPtr file,
            byte* filename);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern libstreamfile_t* libstreamfile_open_buffered(
            libstreamfile_t* extLibsf);

        #endregion
    }
}