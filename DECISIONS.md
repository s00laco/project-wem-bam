# Decisions

This document records important architectural and product decisions.

The purpose is to document *why* decisions were made so they are not revisited repeatedly without good reason.

---

## 2026-06-29

### Manual Source Management

**Decision**

Users manually choose which folders and BA2 archives are indexed.

**Reason**

Provides complete user control and avoids unnecessary scanning of the user's computer.

---

## 2026-06-29

### Focus on WEM Discovery

**Decision**

The application is designed to help users discover, preview and organise WEM audio rather than extract original source WAV files.

**Reason**

The primary workflow problem is finding useful audio quickly, not recovering original assets.

---

## 2026-07-01

### User Knowledge is First-Class Data

**Decision**

User-created information is stored separately from indexed metadata.

Examples include:

- Notes
- Tags
- Collections
- Favourites
- History
- CK References

**Reason**

User annotations should survive re-indexing and become part of the user's personal audio library.

---

## 2026-07-01

### Starfield Project Scope

**Decision**

Wem Bam focuses on Starfield's Wwise audio pipeline.

Future support for other Wwise-based games may be considered, but the application will not attempt to become a universal game asset browser.

**Reason**

Maintaining a clear project scope helps keep development focused on solving the workflow problems that inspired the project.

## 2026-07-01

### Third-Party Tools

**Decision**

Where practical, Wem Bam will integrate with well-established third-party tools and libraries rather than reimplementing their functionality.

Examples include:

- vgmstream for audio decoding and playback
- Mutagen.Bethesda for BA2 archive support

**Reason**

This allows the project to focus development effort on its core purpose: helping users discover, preview and organise game audio.

Established tools are generally more mature, better tested and actively maintained by their respective communities.

## 2026-07-04

### Source Lifetime

**Decision**

Source management is owned by a dedicated `SourceManager` service rather than by individual windows.

**Reason**

Source data represents application state rather than user interface state.

Keeping the source list independent of the Settings window allows multiple windows and future features to share the same data while keeping responsibilities clearly separated.

It also provides a clean transition to persistent SQLite storage later without requiring changes to the user interface.

---

## 2026-07-04

### Unified File Sources

**Decision**

Wem Bam treats supported audio-related files as generic file sources rather than creating separate workflows for BA2 archives and individual audio files.

Initially supported file types include:

- BA2
- BNK
- WEM
- WAV

Additional file types may be added in the future.

**Reason**

The application is intended to help users organise and discover audio regardless of where it originated.

Using a single **Add File...** workflow keeps the interface simple while allowing future expansion.

---

## 2026-07-04

### Duplicate Source Handling

**Decision**

When multiple sources are added, duplicate sources are skipped and reported using a single summary message.

The operation continues processing all selected sources rather than stopping at the first duplicate.

**Reason**

Users will often add multiple files or archives at once.

Summarising the results provides useful feedback without interrupting the workflow with unnecessary message boxes.

---

## 2026-07-05

### Timestamp Storage

**Decision**

All timestamps stored in the SQLite database use UTC Unix time in milliseconds.

Database timestamps are stored as INTEGER values.

Outside the persistence layer, the application uses `DateTimeOffset`.

The persistence layer is responsible for converting between the database representation and application objects.

**Reason**

Using UTC Unix timestamps provides a timezone-independent storage format that sorts efficiently and avoids daylight saving ambiguities.

Keeping Unix timestamps confined to the persistence layer prevents storage implementation details from leaking into the rest of the application.

---

## 2026-07-05

### Log Retention

**Decision**

Wem Bam automatically retains the 30 most recent log files.

Older log files are deleted during application startup before a new session log is created.

**Reason**

This keeps disk usage bounded while preserving recent diagnostic information.

Retention is based on the number of application sessions rather than elapsed time, ensuring consistent behaviour regardless of how frequently the application is used.

---

## 2026-07-05

### Background Task Framework

**Decision**

All long-running operations execute through a single BackgroundTaskManager.

The BackgroundTaskManager is responsible for:

- Starting operations.
- Tracking progress.
- Reporting elapsed time.
- Handling cancellation.
- Logging operation lifecycle events.
- Reporting completion or failure.

The BackgroundTaskManager does not perform domain-specific work.

Individual operations (such as folder indexing or BA2 indexing) are responsible only for performing their own work and reporting progress back to the manager.

**Reason**

This keeps operational concerns separate from business logic, provides a consistent user experience across all long-running operations, and avoids duplicated infrastructure throughout the application.

---

### Single Background Operation

**Decision**

Only one background operation may execute at any time.

While an operation is running:

- The user interface remains responsive.
- Additional long-running operations cannot be started.
- The currently running operation may be cancelled.

**Reason**

Wem Bam is intended to remain simple and predictable.

Supporting multiple concurrent operations or operation queues would add significant complexity with little practical benefit for the application's intended scope.

---

### Background Operation Behaviour

**Decision**

Background operations are processed in small interruptible batches.

Between batches the application:

- Updates progress.
- Checks for cancellation.
- Continues only if appropriate.

Cancellation should feel immediate to the user while always leaving the application in a consistent state.

**Reason**

This provides responsive cancellation while allowing each operation to choose sensible batch sizes appropriate to the work being performed.

---

### Background Operation Outcomes

**Decision**

Every background operation ends in exactly one of the following states:

- Completed
- Completed with warnings
- Cancelled
- Failed

Recoverable errors are logged and skipped where appropriate.

Fatal errors terminate the operation cleanly.

**Reason**

This provides predictable behaviour for users and establishes a consistent lifecycle for every long-running operation in the application.

---

## 2026-07-13

### Canonical Indexed Audio Asset

Decision

The indexing engine produces a canonical discovered audio asset model before persistence.

DatabaseManager persists audio assets rather than interpreting filesystem paths directly.

Reason

Loose WEM files and BA2-contained WEM files represent the same logical concept: a playable audio asset.

Representing discovered assets using a common model keeps the indexing engine independent of storage format and avoids introducing archive-specific logic into the persistence layer.

This allows new source types to be added while preserving a consistent indexing pipeline.

---

## 2026-07-18
(added days later after the original decision was made to correct a gap here in the decision record)
### Canonical Audio Location Model

**Decision**

Every indexed `AudioAsset` represents the location of a discovered audio asset using `ContainerPath` and `AssetPath`.

For loose WEM files:

- `ContainerPath` is `null`.
- `AssetPath` stores the full path to the WEM file.

For archive-based assets:

- `ContainerPath` stores the path to the archive.
- `AssetPath` stores the asset's path within that archive.

**Reason**

Loose files and archived files are physically stored differently.

Rather than forcing both storage mechanisms into an identical representation, the canonical model reflects each storage type naturally while providing a consistent interface for the rest of the application.

This allows playback, indexing and future archive formats to operate on a common `AudioAsset` model without requiring artificial or redundant path representations.

---

## 2026-07-13

### Explicit Archive Indexing

**Decision**

Folder sources index only loose audio files.

Archive contents are indexed only when the archive itself is explicitly added as a source.

Initially this applies to BA2 archives.

**Reason**

Users explicitly control which content is indexed.

Automatically indexing every archive discovered within a folder could result in unexpectedly scanning hundreds of unrelated archives, significantly increasing indexing time and producing unwanted results.

Treating archives as explicit sources keeps indexing predictable, aligns with the application's user-controlled source philosophy, and cleanly separates loose file indexing from archive indexing.

This also allows dedicated archive indexing operations to evolve independently while continuing to produce the same canonical `AudioAsset` model.

---

---

## 2026-07-14

### Single Indexing Session

**Decision**

A user-initiated indexing operation represents a single logical indexing session.

A dedicated `IndexSourcesOperation` coordinates the indexing session.

Individual discovery operations execute within that session.

Current discovery operations include:

- folder discovery
- BA2 archive discovery

**Reason**

Users initiate indexing as a single action and should experience a single background operation with:

- one progress lifecycle
- one cancellation token
- one elapsed timer
- one completion event

This preserves the existing user experience while allowing additional discovery mechanisms to be introduced without changing how indexing is initiated or managed.

---

## 2026-07-14

### Separation of Session Orchestration and Asset Discovery

**Decision**

Session-level responsibilities belong to `IndexSourcesOperation`.

These responsibilities include:

- clearing previously indexed assets
- coordinating discovery operations
- aggregating progress
- producing the final indexing result

Individual discovery operations are responsible only for discovering audio assets from their respective source types and 
passing canonical `AudioAsset` instances into the common indexing pipeline for persistence.

**Reason**

Separating session orchestration from asset discovery keeps each class focused on a single responsibility.

This allows new discovery mechanisms (for example BA2, BNK or future archive formats) to be added without changing the overall indexing workflow.

It also preserves the canonical indexing pipeline whereby every discovery mechanism produces the same `AudioAsset` model before persistence.

---

## 2026-07-18

### Stream-Based Audio Access

**Decision**

Audio playback and future audio processing obtain audio data through `IAudioStreamProvider` rather than accessing storage directly.

Audio stream providers are responsible only for exposing a readable `Stream` for a requested audio asset.

Different storage mechanisms (such as loose WEM files or BA2 archives) provide independent implementations of the same interface.

**Reason**

Separating audio access from playback allows the remainder of the application to operate on streams rather than physical storage.

This keeps playback independent of filesystem and archive formats, preserves single responsibility, and allows additional storage mechanisms to be introduced without changing playback code.

---

## 2026-07-20

### Playback Architecture

**Decision**

Embedded audio playback is implemented as a dedicated playback subsystem composed of small, single-purpose components.

The playback subsystem consists of:

- `PlaybackService`
- `VgmStreamDecoder`
- `ManagedStreamFileAdapter`
- `NAudioPlayer`

`PlaybackService` coordinates playback but does not perform audio decoding or audio output directly.

`VgmStreamDecoder` owns interaction with libvgmstream.

`ManagedStreamFileAdapter` adapts a managed `System.IO.Stream` to the callback interface required by libvgmstream.

`NAudioPlayer` is responsible only for rendering decoded PCM audio to the operating system audio device.

**Reason**

This preserves the project's single responsibility principle by separating playback orchestration, native decoder integration, stream adaptation and audio output into independent components.

It also allows the playback pipeline to remain independent of how audio assets are physically stored while keeping native interoperability isolated from the remainder of the application.

---

---

## 2026-07-20

### Frozen libvgmstream Dependency

**Decision**

Wem Bam targets **libvgmstream r2117**.

The project's native interoperability layer is implemented against libvgmstream's **public API**.

The canonical public headers are:

- `libvgmstream.h`
- `libvgmstream_streamfile.h`

These headers are stored within the repository under:

`ThirdParty/libvgmstream/r2117/`

The internal headers (`vgmstream.h` and `streamfile.h`) are retained for reference only and are not considered part of Wem Bam's native interoperability contract.

Future upgrades to libvgmstream will be treated as explicit dependency upgrade milestones rather than automatically tracking newer upstream releases.

**Reason**

Generating the managed interoperability layer against the library's supported public API minimises coupling to implementation details, reduces future maintenance effort, and provides a stable interoperability boundary.

---

## 2026-07-30

### Native Interoperability Strategy

**Decision**

Wem Bam's native interoperability layer is implemented as a direct ABI translation of libvgmstream's public API.

The interop layer is composed of:

- `NativeEnums`
- `NativeStructs`
- `NativeDelegates`
- `NativeMethods`

These types exist solely to mirror the native API and must not contain wrapper logic, resource ownership, string conversion or other managed convenience functionality.

Higher-level behaviour belongs in managed wrapper classes such as `LibVgmStream` and `ManagedStreamFileAdapter`.

**Reason**

Maintaining a thin ABI layer keeps native interoperability isolated, simplifies future library upgrades, and provides a stable foundation for the managed playback subsystem.

---

## 2026-08-01

### Vendored Native Runtime

**Decision**

Wem Bam vendors the native runtime required by the frozen libvgmstream r2117 dependency.

The native runtime consists of:

- `libvgmstream.dll`
- the required companion native libraries

These binaries are committed as third-party dependencies within the repository and are automatically copied to the application's output directory during the .NET build.

Contributors are not required to build libvgmstream locally in order to build or run Wem Bam.

**Reason**

Vendoring the native runtime provides a reproducible development environment, ensures all contributors use the same tested native library version, and keeps the build process focused on the .NET application rather than requiring additional native build tooling.

---

## 2026-08-09

### Explicit PCM16 Playback Output

**Decision**

Wem Bam will explicitly configure libvgmstream to produce PCM16 output for playback.

**Reason**

libvgmstream does not default to PCM16 when no output format is specified. It preserves the decoder's native output format in that case, and the Starfield WEM format investigated during playback naturally produces `SFMT_FLT`.

Wem Bam's existing playback pipeline is designed around PCM16 data and NAudio's PCM wave format. Explicitly requesting PCM16 establishes a consistent output format at the libvgmstream boundary and allows the existing managed playback pipeline to consume the decoded data correctly.

No runtime output-format selection or support for alternative output formats is required for the current playback architecture.

---

## 2026-08-09

### Complete NAudio Playback Reads

**Decision**

`VgmStreamWaveProvider.Read()` will continue decoding and copying audio until the requested NAudio byte count has been satisfied or decoding produces no further data.

**Reason**

A single libvgmstream decode operation may produce fewer bytes than the NAudio playback buffer requests.

NAudio's playback implementations may submit the entire playback buffer after a single `Read()` call and zero-fill any portion not supplied by the provider. Returning a partial decode while more audio is available therefore introduces silence into continuous playback.

The provider must therefore bridge the difference between NAudio's requested byte count and the amount produced by each individual libvgmstream decode operation.

This keeps the NAudio boundary byte-oriented while allowing libvgmstream to continue operating in its sample/frame-oriented decode model.

---

## 2026-08-10

### Wwise Event Metadata and Relationships

**Decision**

Wem Bam will store Wwise Event metadata separately from `AudioAssets` and represent the relationship between WEM audio assets and Wwise Events as a many-to-many relationship.

Wwise Event metadata will be imported from Starfield's `SoundBanksInfo.json`.

The metadata model will retain the following Wwise Event information:

- Wwise Event ID
- Event Name
- Object Path
- Duration Type
- Duration Min
- Duration Max

WEM audio assets will retain their own File ID as explicit metadata.

The WEM-to-Event association will be stored in a dedicated relationship table, allowing:

- an AudioAsset to be associated with multiple Wwise Events;
- a Wwise Event to reference multiple AudioAssets;
- relationships to be traversed in either direction.

`SoundBanksInfo.json` will be configured and imported separately from normal audio indexing. It will be presented to the user alongside audio source configuration because it provides metadata used to give indexed WEM assets meaningful Wwise Event information.

The imported Wwise metadata remains indexed data and is kept separate from user-created information such as notes, tags, collections and favourites.

**Reason**

Analysis of the complete Starfield `SoundBanksInfo.json` confirmed that the relationship between streamed WEM files and Wwise Events is genuinely many-to-many.

Flattening Event information directly onto `AudioAssets` would therefore misrepresent the source data and prevent Wem Bam from correctly representing multiple Event associations.

A dedicated relationship preserves the actual Wwise structure while supporting future search, display and navigation between related audio assets and Events.

The separate metadata import also keeps Wwise metadata acquisition independent from normal audio discovery and indexing, allowing either process to be updated without unnecessarily repeating the other.

---

## 2026-08-10

### Wwise Metadata Import and AudioAsset Relationship Model

**Decision**

Wem Bam will treat Starfield Wwise metadata and indexed audio assets as two independently populated datasets that are connected through the shared WEM File ID.

During normal WEM audio indexing, the numeric File ID will be extracted from the WEM filename and stored in `AudioAssets.FileId`.

When `SoundBanksInfo.json` is imported, the WEM File ID supplied by each `ReferencedStreamedFiles.Id` will be stored as part of the Wwise Event relationship data.

The metadata import will not require the corresponding `AudioAsset` to already exist and will not perform a lookup against `AudioAssets` during import. The two datasets are allowed to be populated in either order.

Wwise Event metadata will be stored in the dedicated `WwiseEvents` table.

The many-to-many relationship between WEM audio and Wwise Events will be stored in the dedicated `AudioAssetToWwiseEvents` relationship table using:

- `FileId`
- `WwiseEventId`

The relationship table will therefore reference the shared WEM File ID rather than the internal `AudioAssets.Id`.

The relationship between indexed audio and Wwise metadata will be resolved when the data is queried, using the shared File ID.

`SoundBanksInfo.json` import will remain a separate operation from normal audio indexing.

Audio indexing will populate and update `AudioAssets` without modifying Wwise Event metadata.

Wwise metadata import will populate and update `WwiseEvents` and `AudioAssetToWwiseEvents` without modifying or creating `AudioAssets`.

Re-importing Wwise metadata will replace the previously imported Wwise Event metadata and relationship data with the newly imported dataset. Audio indexing remains independent of this process.

**Reason**

The WEM filename provides the numeric File ID used by Starfield's Wwise metadata. Wem Bam therefore obtains the File ID during audio discovery, while `SoundBanksInfo.json` independently provides the same identifier through `ReferencedStreamedFiles.Id`.

Requiring metadata import to find or validate an existing `AudioAsset` would unnecessarily couple two independent indexing processes and would make the result dependent on which data had been imported first.

Using the shared File ID allows the audio and metadata datasets to remain independent while still supporting joins when searching, displaying, or navigating between audio assets and Wwise Events.

This also preserves the confirmed many-to-many relationship between WEM files and Wwise Events without requiring an `AudioAsset` record to exist when the metadata is imported.

---

## 2026-08-14

### Logical Audio Assets and Multiple Physical Sources

**Decision**

Wem Bam will treat each WEM File ID as a single logical audio asset, regardless of how many physical copies of that WEM are discovered.

Physical occurrences of an audio asset will be stored separately from the logical `AudioAsset` record, allowing a single audio asset to have multiple physical sources.

Each physical source will retain the information required to locate and access that occurrence, including its source, container and asset path.

Where the same WEM File ID is discovered in both a BA2 archive and as a loose WEM file:

- The BA2 occurrence is considered the canonical game audio source because it originates from the shipped game.
- A loose WEM occurrence is considered an override or additional source.
- If no BA2 occurrence exists for a File ID, a loose WEM occurrence may serve as the default source.
- Multiple physical sources must remain available even when they contain identical audio.
- Physical sources containing different audio under the same File ID must also remain associated with the same logical audio asset and be distinguishable from one another.

The database will therefore retain enough information to determine whether multiple physical sources contain the same or different audio content.

The default playback source will be stored as part of the underlying asset/source data rather than being determined solely by the UI.

The initial default source will be the canonical BA2 occurrence when one exists. Where no BA2 occurrence exists, an available loose WEM occurrence will become the default.

The user will be able to change which physical source is designated as the default playback source. The UI for changing this selection will be implemented separately from the underlying database support.

User-created information such as notes, tags, collections and favourites remains separate from indexed audio and source data.

**Reason**

The same WEM File ID can legitimately be discovered in multiple physical locations, such as a shipped BA2 archive and a loose modded WEM.

Treating each occurrence as a separate logical audio asset would cause the same sound to appear multiple times in the user's library and would make later organisation and user metadata unnecessarily difficult.

Keeping one logical asset with multiple physical sources allows Wem Bam to represent the relationship between the shipped game audio and modded or extracted copies without losing either occurrence.

A loose WEM may intentionally replace the shipped BA2 version, so different audio content under the same File ID must not be discarded or merged into an indistinguishable copy.

The physical source that is played is therefore a property of the logical audio asset and can be changed by the user without creating another logical asset.

---

## 2026-09-04

### Derived Audio Categories from WEM Paths

**Decision**

Wem Bam will derive audio category information from the folder structure and nomenclature encoded in WEM paths.

The leading WEM path component provides a short Bethesda/Wwise category code, for example:

- `AMB` = Ambience
- `SFX` = Sound Effects
- `WPN` = Weapon
- `ITM` = Item
- `UI` = User Interface
- `VEH` = Vehicle
- `MUS` = Music

These derived categories are application-level metadata and are distinct from user-created Tags. They are derived on demand when an individual audio asset is displayed and are not persisted in the database or stored as separate indexed metadata.

The full WEM path will continue to be retained and displayed as source metadata. The derived category information is an additional interpretation of that existing path rather than a replacement for it.

The conversion from short code to human-readable category name will use a central application-owned mapping of known Bethesda/Wwise category codes to their display names.

The mapping will be deterministic and shared by any feature that consumes these derived categories. It must not be duplicated independently across UI components or search/filter implementations.

The approved category mappings are educated interpretations based on research into Starfield's WEM paths and naming conventions. They are not assumed to be definitively documented by Bethesda/Wwise.

Unknown or unrecognised category codes must remain representable using their original raw value rather than causing display failure.

The category hierarchy will initially be treated as derived information associated with the individual AudioAsset being displayed. Wem Bam will not create a separate user-editable category hierarchy in this stage.

**Reason**

Starfield's WEM paths contain meaningful nomenclature that provides useful information about the nature of an audio asset.

Displaying the full WEM path already exposes this information, but interpreting established category codes allows Wem Bam to present the same information more clearly.

The interpretation is performed only for the individual audio asset being viewed, rather than being calculated for the entire audio library. This keeps the implementation simple while avoiding unnecessary storage and indexing of derived information.

Keeping the category mapping separate from user Tags preserves an important distinction:

- Derived categories describe information inferred from the game's existing audio structure.
- User Tags describe information assigned by the Wem Bam user.

A central mapping also provides a single place to maintain the Bethesda/Wwise terminology as additional category codes are identified or existing interpretations are refined.

The original WEM path remains the authoritative source information, while the derived category is a convenience representation of that information.