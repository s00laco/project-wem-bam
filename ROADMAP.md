# Roadmap

This roadmap outlines the planned evolution of Wem Bam.

Features are grouped into logical milestones rather than strict release versions. Priorities may change as development progresses.

---

# Phase 1 – Foundation (MVP)

Build a fast, reliable WEM browser capable of indexing, searching and previewing audio.

## Core Application

- Modern desktop UI
- Application settings
- Logging
- Persistent user preferences
- Window state persistence

## Source Management

- Add source folders
- Add BA2 archives
- Enable/disable sources
- Remove sources
- Refresh sources

The application never automatically searches for game installations. Users choose exactly what is indexed.

## Audio Indexing

- Index WEM files
- Index BA2 audio contents
- Store metadata in SQLite
- Fast incremental re-indexing

## Audio Playback

- Instant preview
- Stop / Pause
- Seek bar
- Volume control
- Keyboard shortcuts

## Search

Search by:

- filename
- path
- event name
- file ID

Weighted search ranking should prioritize the most relevant results.

---

# Phase 2 – Organisation

Help users build their own audio library.

## Notes

Add personal notes to any sound.

## Tags

Create custom tags.

Provide useful built-in tags such as:

- Found in CK
- Ambient
- Music
- Voice
- UI
- Weapon
- Creature

Users can create unlimited additional tags.

## Collections

Create named collections of sounds.

Examples:

- Reactor Sounds
- My Favourite Ambience
- Horror
- Ship Interior
- Sci-Fi Machinery

## Favourites

Quick access to commonly used sounds.

## History

Automatically record recently previewed sounds.

---

# Phase 3 – Creation Kit Workflow

Improve the workflow for Bethesda mod authors.

## CK Reference

Allow users to manually record:

- CK Form Name
- CK Form Type
- Editor ID (optional)

This information is user-managed and complements personal notes.

## Event Information

Display available event information where known.

## Copy Tools

Quickly copy useful values such as:

- Event Name
- WEM Filename
- File ID
- Path

---

# Phase 4 – Power Features

## Advanced Search

- configurable search weighting
- wildcard search
- filters
- saved searches

## Batch Operations

- export lists
- batch tagging
- batch collection management

## UI Improvements

- multiple layouts
- resizable panels
- custom columns

---

# Phase 5 – Future Investigation

Ideas worth exploring after the core application is mature.

- Automatic CK relationship discovery
- Automatic event-to-form mapping
- Support for additional Bethesda games
- Plugin architecture
- Waveform display
- Spectrogram view
- Audio comparison tools

These items are exploratory and are not guaranteed.

---

# Guiding Principle

Every feature should answer one question:

> Does this make finding, previewing or organising game audio faster?

If not, it probably belongs in another tool.
