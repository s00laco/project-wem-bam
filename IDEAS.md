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
