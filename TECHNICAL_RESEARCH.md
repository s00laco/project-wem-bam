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