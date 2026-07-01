# Architecture

This document describes the overall architecture of Wem Bam and the principles that guide its design.

The goal is to create an application that remains fast, maintainable and easy to extend as new features are added.

---

# Design Philosophy

Wem Bam exists to help modders discover and preview game audio as quickly as possible.
The architecture should avoid assumptions that permanently tie the application to Starfield, 
while remaining focused on Wwise-based game audio.

Every architectural decision should support that goal.

The application should favour:

- simplicity
- responsiveness
- maintainability
- extensibility

over unnecessary complexity.

---

# Core Principles

## Single Responsibility

Each major component should have one clearly defined responsibility.

Examples:

- UI displays information.
- Database stores information.
- Indexer discovers files.
- Audio Player plays audio.
- Search Engine finds results.

Each component should do one job well.

---

## User-Controlled Sources

Users explicitly choose what is indexed.

The application does not automatically search the user's computer for installed games.

Supported sources include:

- folders
- subfolders
- individual files
- BA2 archives

---

## User Knowledge is First-Class Data

Automatically indexed metadata is only part of the application's value.

User-created information is equally important.

Examples include:

- notes
- tags
- collections
- favourites
- history
- CK references

This information should remain separate from indexed game data.

---

# High-Level Components

The application consists of several logical systems.

## User Interface

Responsible for:

- displaying information
- playback controls
- search
- navigation
- settings

The UI should never perform heavy processing directly.

---

## Source Manager

Responsible for:

- managing indexed locations
- monitoring enabled sources
- refreshing sources

---

## Indexing Engine

Responsible for:

- scanning sources
- reading BA2 archives
- indexing WEM files
- extracting metadata

The indexer should support incremental updates wherever possible.

---

## Search Engine

Responsible for:

- indexing searchable content
- weighted searching
- ranking results

Search behaviour should be configurable without changing application code.

---

## Audio Playback

Responsible only for:

- loading audio
- playback
- seeking
- volume
- playback state

---

## Database

SQLite stores:

- indexed metadata
- user settings
- notes
- tags
- collections
- favourites
- history

No external database server is required.

---

# Separation of Data

The application maintains two distinct kinds of information.

## Indexed Data

Generated automatically.

Examples:

- filename
- path
- file ID
- event name
- archive source

Indexed data may be regenerated at any time.

---

## User Data

Created by the user.

Examples:

- notes
- tags
- collections
- favourites
- CK references

User data must never be overwritten during re-indexing.

---

# Performance

The application should prioritise responsiveness.

Goals include:

- fast startup
- responsive UI
- incremental indexing
- efficient searching
- minimal memory usage

Lengthy operations should run in the background wherever practical.

---

# Extensibility

The application should avoid assumptions that limit future expansion.

Examples include:

- multiple Bethesda games
- additional metadata providers
- future plugins
- additional archive formats

New features should be additive rather than requiring major rewrites.

---

# Error Handling

Failures should be isolated wherever possible.

Examples:

- one corrupt BA2 should not stop indexing other sources
- one invalid WEM should not terminate playback
- missing metadata should not prevent browsing

Meaningful log messages should be provided to help diagnose problems.

---

# Guiding Principle

The architecture should always support the core purpose of the application:

Find.

Preview.

Organise.

Game audio quickly.
