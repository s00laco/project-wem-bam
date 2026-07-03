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

## Multiple Views

**Status:** Future

Allow multiple search tabs, similar to Creation Kit's Object Window or Foobar2000 playlists.

Each tab should maintain its own:

- Search
- Results
- Selection

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

## Tree Navigation

**Status:** Investigation

Investigate an optional tree view for navigating:

- Collections
- Tags
- Favourites
- History

This should remain optional until real-world usage demonstrates a clear benefit over a flat search-driven workflow.

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

---

# Nice-to-Have

Ideas worth remembering but intentionally outside the current roadmap.

- Drag & drop WEM files
- Export search results
- Custom keyboard shortcut configuration
- Search history
- Favourite searches
