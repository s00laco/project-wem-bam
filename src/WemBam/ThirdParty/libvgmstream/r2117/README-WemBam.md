# Wem Bam Notes

This directory contains the native libvgmstream runtime used by Wem Bam.

## Version

The project is pinned to **libvgmstream r2117**.

Future upgrades should be treated as explicit project milestones rather than automatically tracking upstream releases.

## Runtime

The `runtime/x64` directory contains the native libraries required by Wem Bam at runtime.

### libvgmstream.dll

`libvgmstream.dll` was built from the official frozen r2117 source using the project's CMake build system.

The shared library is **not** included in the official Windows release package and must therefore be built separately.

The build procedure and investigation findings are documented in `TECHNICAL_RESEARCH.md`.

### Companion DLLs

The remaining runtime DLLs are the official companion libraries distributed with the upstream r2117 Windows release package.

These libraries are required by libvgmstream at runtime and are copied alongside `libvgmstream.dll` during the Wem Bam build.

## Licensing

The original upstream documentation and licence files are preserved in this directory:

- `README.md`
- `Usage.md`
- `COPYING`

Refer to those files for upstream documentation, licensing terms and usage information.

## Repository Policy

The native runtime is committed to the repository as a third-party dependency.

Contributors are **not** required to build libvgmstream locally in order to build or run Wem Bam.

The project file automatically copies the required native runtime into the application's output directory during each build.