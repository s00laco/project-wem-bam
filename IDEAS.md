# Ideas

This document captures ideas, observations and possible future enhancements.

Items in this document are **not commitments** and should not be interpreted as part of the roadmap unless they are promoted into `ROADMAP.md`.

The goal is simply to avoid losing good ideas while keeping the roadmap focused.

---

# User Experience

## Playback Menu

**Status:** Under Consideration

### Observation

Playback controls are permanently visible in the main window.

### Idea

Remove the Playback menu entirely.

### Reason

Playback is a frequent action that is faster via toolbar buttons or keyboard shortcuts than navigating menus.

---

## Search

**Status:** Future

### Idea

Replace the Search button with live searching as the user types.

---

## Window Layout

**Status:** Future

### Ideas

- Remember window layout
- Remember panel sizes
- Remember column widths
- Remember sort order

---

## Filter Tabs

**Status:** Future

Support multiple filter tabs, similar to multiple Object Window tabs in the Creation Kit or playlists in Foobar2000.

Each tab maintains its own independent:

- Selected Collection
- Filter text
- Results list
- Selected sound
- Sort order

The first tab is created automatically but behaves exactly like any other tab.

Users should be able to:

- Create tabs
- Close tabs
- Rename tabs directly by clicking the tab title

Example workflow:

Tab 1:
Favourites filtered by "reactor"

Tab 2:
All Sounds filtered by "alarm"

Tab 3:
Industrial collection filtered by "steam"

Switching tabs restores the entire working context instantly.

Tabs should persist between sessions.

Reopening Wem Bam should restore the user's previous workspaces, including:

- Open tabs
- Tab names
- Selected Collection
- Filter text
- Results
- Selected sound

---

## Workspace Layout

**Status:** Future

Remember the user's preferred application layout between sessions.

Examples include:

- Window size and position
- Panel sizes
- Splitter positions
- Column widths
- Column visibility
- Sort order

The application should restore the layout automatically when reopened.

---

# Playback

## Keyboard Shortcuts

**Status:** Future

Suggested defaults:

- Space → Play / Pause
- Enter → Play selected
- Ctrl+Enter → Play collection
- Left / Right → Previous / Next
- Ctrl+L → Toggle looping

---

## Playback Modes

**Status:** Future

Support:

- Play single sound
- Queue playback
- Loop single sound
- Loop queue
- Shuffle queue

---

## Playback Timeline

**Status:** Future

Display:

- Elapsed time
- Remaining time
- Total duration
- Scrubbable playback position

---

## Waveform Display

**Status:** Investigation

Display a waveform above the playback timeline to provide a visual representation of the audio.

Potential benefits:

- Identify silence
- Spot transient sounds
- Scrub to specific sections more accurately

---

## Audio Thumbnails in Search Results

**Status:** Future

Display a compact visual summary of each audio asset directly within the main search results list.

Rather than requiring users to play multiple files to identify the one they want, a small audio thumbnail would provide an at-a-glance indication of the overall shape and energy of the audio.

Examples include:

- Short transient sounds (footsteps, gunshots, explosions)
- Continuous ambience
- Dialogue with pauses
- Music with repeating patterns

The thumbnail does not need to be a full-resolution waveform. A simplified representation that captures the overall character of the audio would provide the greatest benefit while remaining fast to render.

Potential implementation:

- Generate the audio thumbnail during indexing alongside other derived metadata (such as duration).
- Store the thumbnail representation in the database.
- Display the thumbnail in the main asset list.
- Reuse the same underlying data for larger waveform displays elsewhere in the application.

Benefits:

- Faster visual identification of audio assets.
- Fewer files need to be previewed before finding the desired sound.
- Makes large audio libraries easier to browse.
- Complements the planned detailed waveform display in the playback panel.

---

# Search

## Predictive Search

**Status:** Future

Update results while typing.

---

## Search Ranking

**Status:** Future

Allow weighting of:

- Event Name
- Generated WEM filename
- User Tags
- Collections
- Notes
- Object Path

---

## Search Behaviour

**Status:** Future

Investigate options such as:

- Token-based searching
- Wildcards
- Fuzzy matching
- Adjustable ranking

---

## Filters

**Status:** Future

Optional filters to narrow search results.

Possible examples:

- Source
- Collection
- Tags
- File Type
- Archive

---

# Collections

## Collections

**Status:** Future

Collections represent user projects rather than simple favourites.

Examples:

- Industrial
- Reactors
- Horror
- UI
- Current Mod

Events may belong to multiple collections.

---

## Smart Collections

**Status:** Investigation

Collections generated automatically from rules or searches.

---

# Tags

## Tag Management

**Status:** Future

Support:

- Unlimited tags
- Multiple tags per event
- Easy editing
- Search by tags

---

# Notes

## Rich Notes

**Status:** Future

Support:

- Multi-line notes
- Auto-save
- Searchable notes

---

# Library

## Collection Browser

**Status:** Investigation

Replace the current Sources panel with a Collection Browser.

The Collection Browser represents logical groups of sounds rather than physical storage locations.
The Collection Browser is the application's primary navigation mechanism.

Built-in collections may include:

- All Sounds
- Favourites

User-created collections appear beneath these.

Selecting any collection immediately filters the Results list. Additional text entered into the Filter field further narrows the currently selected collection.

Collections should never be "opened" in a separate window. Selecting a collection simply changes the active view.

Collection management should be available via both toolbar buttons and right-click context menus.

Common actions include:

- New Collection
- Rename Collection
- Delete Collection
- Duplicate Collection

Future collections should remain fully user-configurable, including the ability to remove default collections if desired.

---

## Backup & Restore

**Status:** Future

Allow users to back up and restore their complete Wem Bam library.

The backup should preserve all user-created information, including:

- Sources
- Notes
- Tags
- Collections
- Favourites
- History
- Settings

The primary workflow should allow users to:

- Create a backup
- Restore a backup
- Choose the backup location

The implementation should not require users to manually locate or copy the SQLite database.

---

## Creation Kit References

**Status:** Investigation

Allow users to record confirmed Creation Kit information.

Possible fields:

- Form Name
- Form Type
- Comments

This information should always remain user-editable.

---

# Integration

## Foobar2000

**Status:** Planned

Continue using Foobar2000 for playback rather than embedding an audio player.

---

## Wwise

**Status:** Investigation

Investigate possible integration with Wwise in the future.

---

# Interface Polish

**Status:** Future

Ideas include:

- Dark mode
- Resizable panels
- Adjustable columns
- Column visibility
- Progress indicators
- Better status information

## Status Feedback

**Status:** Future

Provide subtle visual feedback in the status bar when background operations complete.

Examples include:

- Soft green fade for successful completion.
- Soft amber fade for completion with warnings.
- Soft red fade for failed operations.

The animation should be brief, non-intrusive and automatically return to the normal status bar appearance after a few seconds.

This feedback supplements logging and progress reporting without interrupting the user's workflow.

---

# Diagnostics

## Log Viewer

**Status:** Future

Provide a built-in log viewer within Settings (likely under the Advanced section).

The log viewer should:

- Display available log files.
- Allow users to read log files directly within Wem Bam.
- Support copying log contents for troubleshooting and bug reports.

The log viewer should be read-only and should not allow editing of log files.

---

# Indexing

## Optimise Duplicate File Discovery

**Status:** Implemented (2026-07)

### Original Idea

Currently duplicate file paths may be discovered when sources overlap (for example, indexing both a parent folder and one of its subfolders).

Future improvement:

- Detect duplicate file paths during discovery rather than relying on the database to reject them.
- Maintain the UNIQUE constraint on the database as a safety net.
- Consider using a `HashSet<string>` during indexing so each unique file is processed only once.

### Outcome

Implemented.

Folder indexing now deduplicates discovered file paths using a `HashSet<string>` before persistence. The database UNIQUE constraint remains as a defensive safety net.

The original motivation has therefore been addressed.

## Warn about overlapping folder sources

Warn when one configured source is already contained within another configured source.

Possible behaviour:

- Highlight redundant sources.
- Explain why they overlap.
- Allow the user to remove the redundant source.

This is a UX improvement only and should not be required for correct indexing.

## Batch SQLite writes during indexing

Current implementation opens a connection and writes one file at a time.

Future improvement:

- Use batched transactions during indexing.
- Commit periodically (for example every 500 or 1000 files).

Benefits:

- Significantly improved indexing performance.
- Reduced disk I/O.
- Limited data loss if indexing is interrupted compared with a single large transaction.

## Verify index after startup

**Status:** Future

Currently Wem Bam remembers that an index exists and restores the last successful indexing information when the application starts.

Future improvement:

Before reporting the index as "Up to date", check whether the configured sources are still the same as when the index was created.

If the sources have changed (for example, a source was added, removed or modified), automatically mark the index as "Out of date" so the user knows it should be rebuilt.

This should avoid incorrectly reporting an index as current after the source configuration has changed.

## Source Guidance

**Status:** Future

Help users understand how different source types are indexed.

Possible ideas:

- Display a tooltip or help text explaining that folder sources index only loose audio files.
- Explain that BA2 archives must be added explicitly using **Add File...** to have their contents indexed.
- Consider adding the same explanation to the future Help documentation.

The goal is to make indexing behaviour predictable and reduce confusion for new users.

---

# Nice-to-Have

Ideas worth remembering but intentionally outside the current roadmap.

- Drag & drop WEM files
- Export search results
- Custom keyboard shortcut configuration
- Search history
- Favourite searches
