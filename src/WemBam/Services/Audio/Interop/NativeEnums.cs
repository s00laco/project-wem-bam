namespace WemBam.Services.Audio.Interop
{
    /// <summary>
    /// Available decoded sample formats.
    /// Mirrors libvgmstream_sfmt_t.
    /// </summary>
    internal enum libvgmstream_sfmt_t
    {
        LIBVGMSTREAM_SFMT_PCM16 = 1,
        LIBVGMSTREAM_SFMT_PCM24 = 2,
        LIBVGMSTREAM_SFMT_PCM32 = 3,
        LIBVGMSTREAM_SFMT_FLOAT = 4,
    }

    /// <summary>
    /// Logging levels used by libvgmstream.
    /// Mirrors libvgmstream_loglevel_t.
    /// </summary>
    internal enum libvgmstream_loglevel_t
    {
        LIBVGMSTREAM_LOG_LEVEL_ALL = 0,
        LIBVGMSTREAM_LOG_LEVEL_DEBUG = 20,
        LIBVGMSTREAM_LOG_LEVEL_INFO = 30,
        LIBVGMSTREAM_LOG_LEVEL_NONE = 100,
    }
}