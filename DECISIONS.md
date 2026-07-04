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