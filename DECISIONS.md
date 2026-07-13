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