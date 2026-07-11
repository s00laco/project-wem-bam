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