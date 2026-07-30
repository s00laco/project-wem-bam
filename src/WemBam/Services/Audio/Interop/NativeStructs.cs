using System;
using System.Runtime.InteropServices;

namespace WemBam.Services.Audio.Interop
{
	// Mirrors the public libvgmstream API structures.
	// This file intentionally contains no managed wrappers or helper functionality.

	[StructLayout(LayoutKind.Sequential)]
	internal struct libstreamfile_t
	{
		public IntPtr user_data;

        public IntPtr read;
        public IntPtr get_size;
        public IntPtr get_name;
        public IntPtr open;
        public IntPtr close;
    }

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct libvgmstream_format_t
	{
		public int channels;
		public int sample_rate;
		public libvgmstream_sfmt_t sample_format;
		public int sample_size;

		public uint channel_layout;

		public int subsong_index;
		public int subsong_count;

		public int input_channels;

		public long stream_samples;
		public long loop_start;
		public long loop_end;

		[MarshalAs(UnmanagedType.I1)]
		public bool loop_flag;

		[MarshalAs(UnmanagedType.I1)]
		public bool play_forever;

		public long play_samples;

		public int stream_bitrate;

        public fixed byte codec_name[128];

        public fixed byte layout_name[128];

        public fixed byte meta_name[128];

        public fixed byte stream_name[256];

        public int format_id;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct libvgmstream_decoder_t
	{
		public IntPtr buf;

		public int buf_samples;
		public int buf_bytes;

		[MarshalAs(UnmanagedType.I1)]
		public bool done;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct libvgmstream_t
	{
		public IntPtr priv;

		// Native type is:
		// const libvgmstream_format_t*
		// C# interop intentionally ignores the native const qualifier.
		public libvgmstream_format_t* format;

		public libvgmstream_decoder_t* decoder;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct libvgmstream_config_t
	{
		[MarshalAs(UnmanagedType.I1)]
		public bool disable_config_override;

		[MarshalAs(UnmanagedType.I1)]
		public bool allow_play_forever;

		[MarshalAs(UnmanagedType.I1)]
		public bool play_forever;

		[MarshalAs(UnmanagedType.I1)]
		public bool ignore_loop;

		[MarshalAs(UnmanagedType.I1)]
		public bool force_loop;

		[MarshalAs(UnmanagedType.I1)]
		public bool really_force_loop;

		[MarshalAs(UnmanagedType.I1)]
		public bool ignore_fade;

		public double loop_count;
		public double fade_time;
		public double fade_delay;

		public int stereo_track;
		public int auto_downmix_channels;

		public libvgmstream_sfmt_t force_sfmt;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct libvgmstream_valid_t
	{
		[MarshalAs(UnmanagedType.I1)]
		public bool is_extension;

		[MarshalAs(UnmanagedType.I1)]
		public bool skip_standard;

		[MarshalAs(UnmanagedType.I1)]
		public bool reject_extensionless;

		[MarshalAs(UnmanagedType.I1)]
		public bool accept_unknown;

		[MarshalAs(UnmanagedType.I1)]
		public bool accept_common;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct libvgmstream_title_t
	{
		[MarshalAs(UnmanagedType.I1)]
		public bool force_title;

		[MarshalAs(UnmanagedType.I1)]
		public bool subsong_range;

		[MarshalAs(UnmanagedType.I1)]
		public bool remove_extension;

		[MarshalAs(UnmanagedType.I1)]
		public bool remove_archive;

		public IntPtr filename;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct libvgmstream_tags_t
	{
		public IntPtr priv;

		public IntPtr key;
		public IntPtr val;
	}
}