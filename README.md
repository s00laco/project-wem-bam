# Wem Bam (Working Title)

> A fast, offline WEM browser and audio discovery tool for Bethesda game modders.

## Overview

Wem Bam is a desktop application designed to solve a simple problem:

Creation Kit does not provide a practical way to preview audio while building mods.

Finding the right ambient sound, machinery noise, room tone or sound effect often involves repeatedly loading the game simply to hear what an event sounds like.

Wem Bam allows mod authors to browse, search and preview Wwise WEM audio files quickly without launching the game.

The project is deliberately focused on discovering and organising WEM files, not authoring or editing Wwise projects.

## Philosophy

The primary goal is to make audio discovery fast.

Everything else exists to support that goal.

This means:

- Fast browsing
- Instant audio preview
- Powerful search
- Easy organisation
- Easy rediscovery of interesting sounds

The application should remain lightweight, responsive and easy to use.

## Intended Users

Wem Bam is primarily designed for Starfield modders working with Bethesda's Wwise audio pipeline.

The application is intentionally focused on Wwise-based game audio rather than attempting to support every Bethesda title or audio format.

Future support for additional Wwise-based games may be explored if appropriate.

## Core Features

### Browse

Browse indexed WEM files from one or more user-defined sources.

### Search

Search using:

- WEM filename
- Path
- Notes
- Tags
- Collections

Search results should use weighted scoring rather than simple text matching.

### Preview

Preview audio instantly.

Playback should begin with minimal delay while navigating search results.

### Organise

Users can organise files using:

- Notes
- Tags
- Collections
- Favourites
- History

None of this modifies the original game files.

## Data Sources

Users choose where the application searches.

Supported source types include:

- folders
- subfolders
- individual files
- BA2 archives (audio)

The application does not automatically scan a user's computer for game installations.

## Design Goals

- Fast startup
- Fast indexing
- Responsive UI
- Minimal installation
- Portable database
- No SQL Server required
- Open source
- Beginner-friendly

## Technology

Current planned stack:

- C#
- .NET 8
- WPF
- SQLite
- GitHub
- Visual Studio 2022

## Project Status

Early development.

The application is currently being built from the ground up.

## Non-Goals

Wem Bam is not intended to replace Wwise or the Creation Kit.

It does not aim to author audio projects, edit Wwise sound banks, or replace existing modding tools.

Its purpose is to make discovering, previewing and organising game audio significantly faster.