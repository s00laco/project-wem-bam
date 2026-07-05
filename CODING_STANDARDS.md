# Coding Standards

These standards exist to keep Wem Bam maintainable, readable and consistent.

## General Principles

- Prefer simple solutions over clever ones.
- Prioritise readability.
- Avoid unnecessary complexity.
- Follow the established architecture.

## Naming

- Use clear, descriptive names.
- Avoid abbreviations unless they are widely understood.
- Classes, methods and properties should clearly describe their purpose.

## Single Responsibility

Each class should have one clearly defined responsibility.

Avoid large classes that perform multiple unrelated tasks.

## Comments

Comment why something is done, not what the code is doing.

Code should generally explain itself through good naming.

## Error Handling

Handle failures gracefully.

Where appropriate:

- log useful information
- present clear messages to the user
- avoid application crashes

## Time and Date Handling

All timestamps stored in the SQLite database must use UTC Unix time in milliseconds.

Rules:

- Store timestamps in SQLite as `INTEGER`.
- Store only UTC timestamps.
- Never store local time in the database.
- Use `DateTimeOffset` throughout the application.
- Convert between Unix time and `DateTimeOffset` only within the persistence layer.
- Convert to the user's local time only when displaying dates in the user interface.

This keeps storage implementation details isolated from the rest of the application and avoids timezone and daylight saving issues.

## User Experience

Performance and responsiveness are priorities.

Long-running operations should not block the UI.

## Background Operations

All long-running operations must execute through the BackgroundTaskManager.

Long-running operations must not be started directly from the user interface.

Background operations must:

- Report progress through the BackgroundTaskManager.
- Support cooperative cancellation.
- Process work in small interruptible batches.
- Leave the application in a consistent state if cancelled.
- Log significant lifecycle events using the Logger.

The BackgroundTaskManager coordinates execution only.

Domain-specific work remains the responsibility of the operation being executed.

## Future Development

New features should integrate cleanly with the existing architecture.

Avoid introducing shortcuts that make future maintenance more difficult.
