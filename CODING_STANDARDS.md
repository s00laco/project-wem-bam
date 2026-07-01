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

## User Experience

Performance and responsiveness are priorities.

Long-running operations should not block the UI.

## Future Development

New features should integrate cleanly with the existing architecture.

Avoid introducing shortcuts that make future maintenance more difficult.
