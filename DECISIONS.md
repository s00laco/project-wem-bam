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