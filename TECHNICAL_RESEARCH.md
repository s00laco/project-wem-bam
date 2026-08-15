# Technical Research

This document records the outcomes of technical investigations performed during the development of Wem Bam.

Unlike `DECISIONS.md`, this document records technical findings rather than project decisions. The purpose is to preserve evidence gathered through experiments, proof-of-concepts and library investigations so that future architectural decisions can be based on established facts rather than repeating previous research.

---

# Mutagen + libvgmstream Investigation

## Objective

Investigate whether Wem Bam can index and eventually play WEM files contained within Starfield BA2 archives without extracting temporary files to disk.

## Investigation Summary

The investigation examined:

- Mutagen.Bethesda
- Starfield Wwizard
- libvgmstream
- existing projects using libvgmstream
- a standalone sandbox application built specifically to validate runtime behaviour

## Confirmed Findings

### Mutagen

Confirmed:

- Starfield BA2 archives can be opened using `Archive.CreateReader()`.
- Archive entries are exposed as `IArchiveFile`.
- `IArchiveFile.AsStream()` returns a normal .NET `Stream`.
- The returned stream is:
  - readable
  - seekable
  - exposes `Length`
  - exposes `Position`
  - supports random seeking
  - returns the expected file bytes.

Mutagen therefore exposes archive contents without requiring extraction to temporary files.

### libvgmstream

Confirmed:

- libvgmstream is designed around `libstreamfile_t`.
- `libstreamfile_t` is intended to be implemented by consumers of the library.
- The abstraction requires callbacks such as:
  - read
  - get_size
  - get_name
  - open
  - close
- Existing production software implements custom `libstreamfile_t` wrappers around arbitrary seekable data sources rather than filesystem files.
- The library documentation explicitly supports custom implementations.

### Starfield Wwizard

Confirmed:

- Wwizard currently writes temporary `.wem` and `.wav` files before playback.
- This is an implementation choice rather than a known limitation of either Mutagen or libvgmstream.

## Conclusions

Current evidence indicates there are no known architectural barriers to implementing direct playback of BA2-contained WEM files without temporary extraction.

Mutagen already exposes archive entries as fully seekable .NET streams, and libvgmstream is designed to consume consumer-implemented stream adapters rather than requiring physical files.

Remaining work appears to be implementation effort rather than feasibility research.

## Impact on Wem Bam

The investigation supports an architecture based on direct streaming rather than temporary extraction.

The expected playback pipeline is:

BA2 Archive
-> Mutagen Stream
-> libstreamfile_t Adapter
-> libvgmstream
-> PCM Audio
-> Audio Output

No project decision has been made at this stage. The investigation provides evidence that will inform future architecture and implementation decisions.

---

# Mutagen Runtime Compatibility

## Objective

Determine the appropriate Mutagen.Bethesda version for integration into Wem Bam.

## Findings

Investigation determined:

- Mutagen.Bethesda 0.53.1 targets .NET 8.
- Mutagen.Bethesda 0.54.x and later target .NET 9.
- The BA2 archive APIs validated during the standalone feasibility investigation are available in 0.53.1.

## Conclusion

Wem Bam will integrate Mutagen.Bethesda 0.53.1 while the project targets .NET 8.

A future migration to a newer Mutagen release can be considered alongside a planned upgrade to the next LTS .NET release.

---

# Mutagen Stream Lifetime Investigation

## Objective

Determine whether a stream returned by `IArchiveFile.AsStream()` remains usable after the originating `Ba2Reader` is no longer referenced by application code.

## Investigation

A sandbox experiment performed the following sequence:

1. Opened a BA2 archive.
2. Located a known WEM entry.
3. Obtained a stream using `IArchiveFile.AsStream()`.
4. Verified the stream supported:
   - `Length`
   - reading
   - seeking
5. Allowed the `Ba2Reader` to go out of scope.
6. Requested a full garbage collection.
7. Repeated the same stream operations.

## Findings

The stream continued to function correctly after the `Ba2Reader` had gone out of scope and a full garbage collection had been requested.

The following operations all continued to succeed without exception:

- querying `Length`
- seeking
- reading

### Additional API Findings

During integration into Wem Bam it was confirmed that:

- `Archive.CreateReader(...)` returns `IArchiveReader`.
- `IArchiveReader` does not implement `IDisposable`.

Consequently, the archive reader cannot be explicitly disposed by application code.

## Conclusion

Although this experiment cannot conclusively prove that the `Ba2Reader` had been reclaimed by the runtime, it provides strong evidence that callers do not need to retain a reference to the originating `Ba2Reader` in order for the stream returned by `IArchiveFile.AsStream()` to remain usable.

This investigation found no evidence that the usability of the returned stream depends on application code retaining a reference to the originating `Ba2Reader`.

---

# libvgmstream Integration Investigation

## Objective

Determine the recommended approach for integrating libvgmstream into a .NET 8 WPF application.

## Findings

Investigation determined:

- No actively maintained official .NET wrapper for libvgmstream was identified.
- P/Invoke is the recommended interoperability mechanism.
- libvgmstream functions solely as an audio decoder.
- Decoded output is PCM audio.
- A custom `libstreamfile_t` implementation remains the appropriate mechanism for presenting a managed `System.IO.Stream` to libvgmstream.
- A separate playback component is required to render decoded PCM audio.
- NAudio is an appropriate playback library for this purpose.
- The required native interop surface is expected to remain small, focusing primarily on decoder lifetime, metadata queries, PCM decoding and cleanup.

## Conclusion

The investigation found no remaining architectural barriers to embedded playback.

The recommended playback pipeline is:

System.IO.Stream
→ ManagedStreamFileAdapter
→ libvgmstream
→ PCM
→ NAudio
→ Windows audio device

---

# libvgmstream Public API Investigation

## Objective

Determine the public native API exposed by libvgmstream r2117 and how it relates to the existing internal headers.

## Findings

Investigation of the r2117 source release confirmed that libvgmstream exposes a dedicated public API.

The public API is defined by:

- `libvgmstream.h`
- `libvgmstream_streamfile.h`

The public API exposes functions for:

- library initialisation and shutdown
- stream opening and closing
- playback configuration
- PCM rendering
- seeking
- metadata queries

The public stream interface is represented by `libstreamfile_t`, which defines callbacks for:

- read
- get_size
- get_name
- open
- close

The internal header `vgmstream.h` includes documentation indicating that consumers should migrate to the public API defined by `libvgmstream.h`.

The internal headers (`vgmstream.h` and `streamfile.h`) remain part of the source distribution and describe the decoder's internal implementation.

---
# Native libvgmstream Build Investigation (r2117)

## Objective

Determine whether the frozen `vgmstream` r2117 source can produce the native shared library required for external consumers, and identify the runtime artefacts produced by the official CMake build.

## Investigation

A clean build was performed from the frozen r2117 source using the project's CMake build system with shared libraries enabled.

The investigation examined:

- whether `libvgmstream.dll` was produced
- which additional runtime artefacts were generated
- the native runtime dependencies of the resulting shared library

The completed build was then inspected using `dumpbin /DEPENDENTS` and the build output directories were searched to identify all generated DLLs.

The distributed Windows x64 package documentation (`Usage.md`) was also reviewed to determine the expected deployment layout for native codec libraries. :contentReference[oaicite:0]{index=0}

## Findings

The shared-library build successfully produced:

- `libvgmstream.dll`
- `libvgmstream.lib`
- `libvgmstream.exp`

Inspection of the generated build output confirmed that `libvgmstream.dll` was the **only DLL produced** by the CMake build.

No codec DLLs were generated.

The upstream `Usage.md` documentation states that Windows deployments require the following companion DLLs to be supplied alongside `libvgmstream.dll`, rather than being produced by the shared-library build: :contentReference[oaicite:0]{index=0}

- `libvorbis.dll`
- `libmpg123-0.dll`
- `libg719_decode.dll`
- `avcodec-vgmstream-59.dll`
- `avformat-vgmstream-59.dll`
- `avutil-vgmstream-57.dll`
- `swresample-vgmstream-4.dll`
- `libatrac9.dll`
- `libcelt-0061.dll`
- `libcelt-0110.dll`
- `libspeex-1.dll`

`dumpbin /DEPENDENTS` confirmed direct imports for:

- `libmpg123-0.dll`
- `libvorbis.dll`
- `libg719_decode.dll`
- `libatrac9.dll`
- `libcelt-0061.dll`
- `libcelt-0110.dll`
- `libspeex-1.dll`

along with the expected Microsoft Visual C++ runtime and Windows system libraries.

## Conclusion

The frozen r2117 source successfully builds the public `libvgmstream` shared library.

The shared-library build does **not** produce the codec DLLs required by `libvgmstream.dll`. This behaviour is consistent with the upstream Windows distribution, whose documentation specifies that these codec DLLs are supplied separately. :contentReference[oaicite:2]{index=2}

---

---

# libvgmstream Output Format and Buffer Contract Investigation

## Objective

Determine how libvgmstream selects its output sample format and establish the meaning and sizing requirements of the `libvgmstream_fill()` buffer parameters.

## Findings

Investigation of the libvgmstream source and public API established that output format selection is controlled by the `force_sfmt` configuration value.

When an explicit output format is supplied, libvgmstream maps the public format value to the corresponding internal sample format.

When `force_sfmt` is not supplied, libvgmstream does not default to PCM16. Instead, it derives the output format from the codec's native sample type, subject to the documented compatibility conversions.

For the investigated `coding_VORBIS_custom` decoder:

    coding_VORBIS_custom
    |
    v
    codec_get_info()
    |
    v
    sample_type = SFMT_FLT
    |
    v
    mixing_get_input_sample_type()
    |
    v
    mixing_get_output_sample_type()
    |
    v
    sfmt_get_sample_size(SFMT_FLT)
    |
    v
    sample_size = 4

`SFMT_FLT` therefore uses a 4-byte sample size.

The `libvgmstream_fill()` public API defines `buf_samples` as the requested number of samples to copy and requires the caller's output buffer to be large enough for:

    buf_samples × channels × sample_size

The implementation copies samples until either the requested sample count has been reached or no further output is available.

Before returning, the decoder state records:

    decoder->buf_samples = buf_copied

and:

    decoder->buf_bytes =
        buf_copied × sample_size × channels

`decoder->buf_bytes` therefore represents the number of decoded output bytes produced for the selected output sample format.

## Official API Examples

The official libvgmstream API examples were also examined.

The PCM16 example explicitly requests:

    LIBVGMSTREAM_SFMT_PCM16

and sizes its output buffer using the PCM16 element size and channel count.

The general command-line implementation sizes its output buffer using:

    sample_buffer_size × format->sample_size × format->channels

rather than assuming a fixed sample size.

The examples use `decoder->buf_bytes` directly as the authoritative number of decoded output bytes.

## Conclusion

The investigation confirms that:

- libvgmstream does not implicitly default to PCM16 when no output format is specified.
- The selected output sample format determines `sample_size`.
- `SFMT_FLT` has a sample size of 4 bytes.
- `libvgmstream_fill()` requires an output buffer sized according to `buf_samples × channels × sample_size`.
- `decoder->buf_samples` records the number of samples copied.
- `decoder->buf_bytes` records the corresponding number of output bytes.
- The official examples size buffers according to the selected sample format and channel count rather than assuming a fixed output element size.

---

# NAudio IWaveProvider Read and Playback Buffer Investigation

## Objective

Determine how NAudio consumes data supplied by an `IWaveProvider` and establish the behaviour when a provider returns fewer bytes than the requested `count`.

## Findings

The `IWaveProvider.Read()` interface is byte-oriented:

    Read(byte[] buffer, int offset, int count)

The `count` parameter represents the number of bytes requested for the current read operation.

The return value represents the number of bytes actually written to the supplied buffer.

NAudio's `WaveProvider16` converts the byte-oriented request into a sample-oriented request for derived providers and expects the provider to supply the requested amount of audio data when available.

NAudio's `BufferedWaveProvider` defaults to `ReadFully = true`. When fewer bytes are available than requested, it fills the remainder of the destination buffer with zeroes and returns the full requested count.

The WinMM playback implementation was also examined.

When a playback buffer is refilled, `WaveOutBuffer.OnDone()` performs a single `Read()` call using the complete playback buffer size.

If the provider returns fewer bytes than the requested buffer size, the implementation does not perform another `Read()` to fill the remainder. Instead, the unused portion of the playback buffer is zero-filled before the complete buffer is submitted to the audio device.

Therefore, for a playback request of:

    28800 bytes

a provider returning:

    16384 bytes

leaves:

    28800 - 16384 = 12416 bytes

which are submitted as silence.

## Conclusion

The investigation confirms that NAudio playback providers must account for the full requested playback buffer when additional audio data is available.

A provider that returns fewer bytes than requested can cause the unused portion of the playback buffer to be filled with silence by the playback implementation.

This behaviour is distinct from the `IWaveProvider` interface itself, which permits a read to return fewer bytes. The practical behaviour depends on how the consuming NAudio output implementation handles the returned byte count.

For continuous generated or decoded audio, the provider therefore needs to continue supplying data within the same `Read()` operation when additional audio is available.

---

# Starfield Wwise Event Metadata Investigation

## Objective

Determine whether Starfield's `SoundBanksInfo.json` can provide meaningful Wwise Event metadata that can be associated with Wem Bam's indexed WEM audio assets.

## Investigation

The complete `SoundBanksInfo.json` dataset was examined to determine the relationship between Wwise Events and their referenced streamed files.

The investigation focused on:

- Wwise Event IDs
- Event Names
- Event Object Paths
- Event Duration Types
- Event Duration Min/Max values
- Referenced streamed-file IDs
- Referenced streamed-file paths
- The relationship between streamed-file IDs and generated WEM filenames
- The cardinality of the WEM-to-Event relationship

## Findings

The `SoundBanksInfo.json` structure provides Wwise Event metadata including:

- `Id`
- `Name`
- `ObjectPath`
- `GUID`
- `DurationType`
- `DurationMin`
- `DurationMax`
- `ReferencedStreamedFiles`

Each `ReferencedStreamedFiles` entry provides:

- `Id`
- `Language`
- `ShortName`
- `Path`

The `ShortName` and source audio references represent the original Wwise source audio, which is generally a `.wav` file.

The `ReferencedStreamedFiles.Id` corresponds to the generated WEM file ID used by Wem Bam to identify the associated audio asset.

The Event's own `Id` is a separate identifier and does not correspond to the WEM filename.

The complete dataset confirmed that the relationship between WEM files and Wwise Events is many-to-many.

A single Wwise Event can reference multiple WEM files.

A single WEM file can be referenced by multiple Wwise Events.

Therefore, Event metadata cannot be represented as a single property directly on an `AudioAsset`.

The Event metadata also provides useful information that is not represented by the WEM file itself. In particular, `DurationType`, `DurationMin`, and `DurationMax` describe the Event's expected playback behaviour.

For example, an Event with:

- `DurationType = OneShot`
- `DurationMin = 10.058301`
- `DurationMax = 21.457775`

provides an Event-level duration range that is distinct from the duration of any individual referenced WEM.

## Conclusion

`SoundBanksInfo.json` provides a reliable source of Wwise Event metadata that can be associated with Wem Bam's indexed WEM assets through the streamed-file ID.

The confirmed many-to-many relationship requires the Event metadata and WEM/Event associations to be represented separately from the `AudioAssets` records.

The metadata source should remain separate from normal audio indexing. WEM discovery populates the audio asset data, while `SoundBanksInfo.json` can be imported separately and used to populate Wwise Event metadata and the relationships between Events and indexed audio assets.

The resulting relationship must support traversal in both directions:

AudioAsset → Wwise Events

Wwise Event → AudioAssets

This provides the foundation for searching by Event metadata and for browsing other WEM assets associated with the same Wwise Event.