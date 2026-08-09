# Native Debugging Log

## Objective

Determine the cause of the native failure occurring during first audio playback.

---

## Session 1 — Native Debugging Established

### Environment

- Built a Debug version of `libvgmstream.dll` from the frozen r2117 source.
- Copied the Debug DLL and matching PDB into Wem Bam's output directory.
- Enabled mixed managed/native debugging.
- Verified that Visual Studio successfully loaded symbols for `libvgmstream.dll`.

### Breakpoint

Function breakpoint:

```
libvgmstream_fill
```

### Observations

- The function breakpoint was successfully hit.
- Visual Studio automatically opened the native source file (`api_decode_play.c`).
- The managed-to-native transition was confirmed by the call stack:

```
VgmStreamWaveProvider.Read()
    |
    v
LibVgmStream.Decode()
    |
    v
libvgmstream_fill()
```

- After stepping over the initial parameter validation, the incoming arguments were observed to be valid:
  - `buf_samples = 4096`
  - `lib` was a valid pointer.
  - `decoder` was a valid pointer.

- The decoder state at entry was consistent with an initial decode:
  - `decoder->buf_samples = 0`
  - `decoder->buf_bytes = 0`
  - `decoder->done = false`

### Conclusions

- The managed/native transition is functioning correctly.
- The initial state at entry to `libvgmstream_fill()` appears valid.
- No evidence of an ABI or marshaling issue was observed during entry to the function.

### Next Step

Step through `libvgmstream_fill()` to observe the first decode operation and determine where execution first deviates from the expected path.

---

## 2026-08-02 — Session 1

### Managed ? Native Boundary Verified

#### Objective

Verify that execution successfully crosses the managed/native boundary and enters the native libvgmstream decoder.

#### Observations

A mixed managed/native debugging session was performed using the Debug build of `libvgmstream.dll` with matching symbols loaded.

The following call chain was observed:

```
SettingsWindow.PlayWemButton_Click()
    |
    v
VgmStreamWaveProvider.Read()
    |
    v
LibVgmStream.Decode()
    |
    v
libvgmstream_fill()
    |
    v
libvgmstream_render()
```

The function breakpoint on `libvgmstream_fill()` was successfully hit.

Stepping into `libvgmstream_render()` confirmed that execution progressed beyond the managed/native boundary and entered the native decoding implementation.

#### Notes

Values shown for local variables that had not yet been initialized (for example `err`) were recognised as uninitialized stack contents and were not treated as meaningful observations.

The session ended after confirming entry into `libvgmstream_render()`. No conclusions were drawn regarding the behaviour of the decoder beyond this point.

#### Outcome

Confirmed:

- native symbols loaded correctly
- function breakpoint resolved correctly
- managed-to-native transition verified
- execution entered `libvgmstream_render()`

The next investigation will continue from within `libvgmstream_render()`.

---

## 2026-08-02 — Session 2

### Core Decode Routine Reached

#### Objective

Continue tracing the native execution path beyond `libvgmstream_render()`.

#### Observations

Execution was stepped through `libvgmstream_render()`.

The setup and buffer initialization completed successfully.

Execution then entered:

```
render_main(sbuf_t* sbuf, VGMSTREAM* vgmstream)
```

The observed native call chain is now:

```
LibVgmStream.Decode()
    |
    v
libvgmstream_fill()
    |
    v
libvgmstream_render()
    |
    v
render_main()
```

#### Outcome

Confirmed that execution reaches the core rendering routine without error.

No failures have been observed within the managed/native boundary or the initial native playback setup.

The investigation will continue by tracing execution within `render_main()`.

---

## 2026-08-02 — Session 3

### Layout Dispatch Reached

#### Objective

Continue tracing execution beyond the core render routine.

#### Observations

Execution was stepped from `render_main()` into:

```
render_layout(sbuf_t* sbuf, VGMSTREAM* vgmstream)
```

The observed native call chain is now:

```
LibVgmStream.Decode()
    |
    v
libvgmstream_fill()
    |
    v
libvgmstream_render()
    |
    v
render_main()
    |
    v
render_layout()
```

`render_layout()` is responsible for selecting the decoding strategy based on the stream's layout type before delegating to the appropriate renderer.

### Layout Selection

Within `render_layout()`, the decoder selected:

```c
case layout_none:
    render_vgmstream_flat(sbuf, vgmstream);
```

This confirms that the current WEM file is decoded using the `layout_none` rendering path rather than one of the interleaved or blocked layout implementations.

The investigation will therefore continue within `render_vgmstream_flat()`.

#### Outcome

Confirmed that execution progresses beyond the core render routine and reaches the layout dispatch stage without error.

The next stage of investigation will identify which layout renderer is selected for the current WEM file.

### Flat Renderer Selected

Stepping into the selected layout handler entered:

```text
render_vgmstream_flat(sbuf_t* sdst, VGMSTREAM* vgmstream)
```

The observed decode path is now:

```
LibVgmStream.Decode()
    |
    v
libvgmstream_fill()
    |
    v
libvgmstream_render()
    |
    v
render_main()
    |
    v
render_layout()
    |
    v
render_vgmstream_flat()
```

`render_vgmstream_flat()` is responsible for decoding flat (non-interleaved) streams. The implementation repeatedly determines how many samples should be decoded and delegates the actual decoding work to `decode_vgmstream()`.

The next stage of investigation will continue into `decode_vgmstream()`.

### Codec Dispatch Reached

Stepping through the flat renderer entered:

```text
decode_vgmstream(sbuf_t* sdst, VGMSTREAM* vgmstream, int samples_to_do)
```

`decode_vgmstream()` dispatches decoding based on the stream's coding type.

The observed decode path is now:

```
LibVgmStream.Decode()
    |
    v
libvgmstream_fill()
    |
    v
libvgmstream_render()
    |
    v
render_main()
    |
    v
render_layout()
    |
    v
render_vgmstream_flat()
    |
    v
decode_vgmstream()
```

The next stage of investigation will determine which codec implementation is selected by the `coding_type` switch.

### Codec Dispatch Result

Within `decode_vgmstream()`, execution did not enter one of the explicit codec-specific cases.

Instead, decoding followed the generic path:

```c
default: {
    sbuf_t stmp = *sdst;
    stmp.samples = stmp.filled + samples_to_do;
    decode_frames(&stmp, vgmstream, samples_to_do);
}
```

This indicates that the current stream is decoded via the shared `decode_frames()` implementation rather than a dedicated codec-specific dispatch case.

The investigation will continue within `decode_frames()`.

## Generic Frame Decoder Reached


### Observation

Stepping through the native decoder has confirmed the execution path from the managed/native boundary into the renderer.

The observed call chain is currently:

```text
LibVgmStream.Decode()
-> libvgmstream_fill()
-> libvgmstream_render()
-> render_main()
-> render_layout()
-> render_vgmstream_flat()
-> decode_vgmstream()
-> decode_frames()
```

### Result

- Execution is successfully entering the native decoder.
- The decoder is progressing through the expected high-level rendering pipeline.
- At this stage there has been no crash or early exit within the renderer.
- The investigation has now reached `decode_frames()`, where frame decoding begins.

### Next Step

Continue stepping into `decode_frames()` to determine where the decoder begins reading or decoding frame data, and identify the first point at which invalid state or corruption appears.

##  `decode_frames()`

### Observation

On entry to `decode_frames()`, the local variable `codec_info` contains the value:

```text
0xffffffff0000006b
```

Expanding the structure in the debugger shows that none of its members can be read. Every function pointer (`sample_type`, `get_sample_type`, `decode_frame`, etc.) reports **"Unable to read memory."**

### Result

`codec_info` does not appear to reference a valid `codec_info_t` structure at this point in execution.

This suggests the failure may originate before any codec-specific decoding function is entered.

### Next Step

Determine where `codec_info` is assigned and identify the source of the invalid pointer.

## Observation: VGMSTREAM state at codec_get_info()

Stopped inside:

```
codec_get_info(VGMSTREAM* v)
```

Inspection of the incoming `VGMSTREAM` revealed internally inconsistent state.

Representative values observed:

- channels = 0
- sample_rate = 0
- coding_type = coding_SILENCE
- layout_type = 1984951568
- meta_type = meta_SGXD
- stream_name contained unreadable/garbled text
- numerous offset/block-size fields contained implausibly large values

Assessment:

The observed values do not describe a coherent decoded audio stream.

At this stage it is not yet known whether this indicates:

- debugger interpretation issues,
- an invalid/stale pointer,
- or memory corruption occurring before `codec_get_info()`.

This is the first strong indication that the native investigation has reached suspicious runtime state.

### Pointer Origin Investigation

Successfully identified the canonical decoder parser and initial `VGMSTREAM*` instance.

- Successful parser: `init_vgmstream_wwise`
- Canonical pointer observed immediately after parser return:
  - `0x000001CE106A52C0`

Attempting to continue execution from this early breakpoint consistently resulted in a fatal CLR termination before reaching `libvgmstream_fill`.

Observed failure:

- Native access violation:
  - `0xC0000005`
  - Read address: `0xFFFFFFFFFFFFFFFF`
- Followed by:
  - `System.ExecutionEngineException`

This behaviour was **not** observed during previous investigations which successfully stepped through the full decode pipeline (`libvgmstream_fill` ? `codec_get_info()`).

Current assessment:

- The pointer origin has been successfully identified.
- The early-breakpoint execution path appears to destabilize the debugging session and is likely a debugger/runtime interaction rather than evidence of corruption within libvgmstream itself.

Recommendation:

Resume pointer tracing from the previously stable `libvgmstream_fill` breakpoint rather than from parser creation.

---

---

## 2026-08-04 — Session 4

### VGMSTREAM Pointer Trace Completed

#### Objective

Determine whether the `VGMSTREAM*` being inspected throughout the native playback pipeline represents the same decoder instance, or whether pointer identity changes before decoding begins.

#### Observations

A systematic pointer trace was performed through the native decode pipeline.

Rather than inspecting decoder fields immediately after entering each function, the debugger was allowed to advance one or two instructions before observations were recorded. This was necessary because Visual Studio repeatedly displayed transient uninitialized or stale values immediately after entering native functions.

After the debugger state settled, the same `VGMSTREAM*` was observed throughout the decode path.

The confirmed execution path is now:

```text
LibVgmStream.Decode()
    |
    v
libvgmstream_fill()
    |
    v
libvgmstream_render()
    |
    v
render_main()
    |
    v
render_layout()
    |
    v
render_vgmstream_flat()
```

At each stage, the settled `VGMSTREAM` contained consistent metadata including:

- channels = 2
- sample_rate = 48000
- num_samples = 184908
- coding_type = coding_VORBIS_custom
- layout_type = layout_none
- meta_type = meta_WWISE_RIFF

During the investigation, Visual Studio repeatedly displayed temporary values such as:

- `0xCCCCCCCCCCCCCCCC`
- unreadable pointers
- impossible sample counts
- invalid metadata
- inaccessible memory

These values consistently resolved after one or two debugger steps and were determined to be transient debugger artefacts rather than persistent runtime state.

#### Conclusions

The investigation found no evidence that the playback pipeline switches to a different `VGMSTREAM` object prior to decoding.

The earlier suspicion that pointer identity was changing appears to have been caused by transient debugger state immediately after entering native functions.

Pointer tracing is therefore considered complete.

#### Next Step

Continue tracing execution beyond `render_vgmstream_flat()` into the decoding implementation.

The investigation will now focus on identifying the first genuine runtime failure, unexpected return path, or error condition responsible for the playback failure rather than continuing to verify `VGMSTREAM` identity.

---

## 2026-08-09 — Session 5

### Native Output Format Investigation

#### Objective

Determine whether the native decoder output format matched the format expected by the managed playback pipeline.

#### Findings

Investigation of the libvgmstream source established that:

- libvgmstream does not default to PCM16 when no output format is specified.
- When `force_sfmt` is not supplied, the library preserves the codec's native output format subject to its documented compatibility conversions.
- The current `coding_VORBIS_custom` codec reports `SFMT_FLT`.
- `SFMT_FLT` has a sample size of 4 bytes.
- `decoder->buf_bytes` is calculated using the selected output sample size.

The managed playback pipeline was supplying `short` buffers and interpreting the decoder output as PCM16.

#### Resolution

Wem Bam was changed to explicitly request:

LIBVGMSTREAM_SFMT_PCM16

Runtime validation confirmed:

- `force_sfmt = LIBVGMSTREAM_SFMT_PCM16`
- `format->sample_format = PCM16`
- `format->sample_size = 2`
- `priv->buf.sample_size = 2`

The native decoder subsequently produced PCM16 output as expected.

---

### Managed Buffer Boundary Investigation

#### Objective

Determine whether the managed buffer passed to libvgmstream correctly represented the native API's sample-count and channel semantics; as testing produced "stuccato" audio playback with obvious
microsecond gaps between audio samples within the same playback.

#### Findings

The libvgmstream API uses `buf_samples` as a per-channel sample/frame count, while the managed `short[]` represents interleaved PCM values.

For stereo PCM16:

1 libvgmstream sample
    = 2 PCM16 channel values
    = 2 shorts

The existing managed boundary initially passed the per-channel sample count directly as the `Span<short>` length, exposing only half of the underlying buffer capacity.

The boundary was corrected so that:

libvgmstream samples
    × channels
    = Span<short> length

Runtime validation subsequently confirmed:

requestedSamples = 4096
Decode Span      = 8192 shorts
buf_samples      = 4096
buf_bytes        = 16384
decodedSamples   = 8192
bytesDecoded     = 16384

The same behaviour was observed across successive decode calls.

---

### NAudio Playback Buffer Investigation

#### Objective

Determine the cause of the remaining staccato playback after the native decode and managed buffer boundaries had been corrected.

#### Findings

The NAudio `IWaveProvider.Read()` contract is byte-oriented:

count  = requested byte count
return = bytes actually written

Runtime observation showed that NAudio was requesting:

28800 bytes

while a single libvgmstream decode produced:

16384 bytes

The existing `Read()` implementation performed one decode and returned the 16384 available bytes.

Investigation of NAudio's playback implementation established that the playback buffer is filled by a single `Read()` call. When fewer bytes are returned, the unused portion of the playback buffer is zero-filled rather than requesting another `Read()` to fill the remainder.

Therefore a 28800-byte playback request receiving only 16384 bytes resulted in:

16384 bytes decoded audio
12416 bytes silence

This behaviour directly accounted for the observed staccato playback.

#### Resolution

`VgmStreamWaveProvider.Read()` was changed to continue decoding and copying data until:

- the requested NAudio byte count has been satisfied, or
- decoding produces zero bytes.

Each decode iteration respects the available sample-buffer capacity and copies only the amount that fits in the remaining NAudio destination.

---

### Final Validation

The corrected playback path was tested under the debugger and with the executable running normally outside Visual Studio.

The previous `ExecutionEngineException` was no longer reproduced.

Playback was then tested with multiple WEM files.

All tested files played continuously and correctly.

#### Conclusion

The investigation identified two independent playback boundary issues:

1. libvgmstream was producing its native output format (`SFMT_FLT`) while the managed playback path expected PCM16.
2. After PCM16 output was established, `VgmStreamWaveProvider.Read()` returned partial NAudio buffers instead of continuing to decode until the requested playback buffer was filled.

Both issues have now been corrected and validated through playback of multiple WEM files.