# Changelog

All notable changes to **7 Noches en Marte** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and uses the project version as the release reference.

## [Unreleased]

### Planned

- Integrate approved audio assets while preserving the procedural-audio fallback.

## [1.0.5] — 2026-11-15

### Added

- Intro narration playback after the player presses **Jugar**.
- Looping `minijuegos` track for the four station scenes.

### Changed

- Main music now begins when the player reaches the map, pauses during a station and resumes upon returning.

## [1.0.4] — 2026-11-15

### Changed

- Lowered the habitat fan hum to 0.12 volume.
- Made hull creaks start 10 seconds after the fans turn off and repeat every 10 seconds while they remain off.

## [1.0.3] — 2026-11-15

### Changed

- Renamed `intro_music.wav` to `musica.wav` so the existing `Resources.Load("Audio/musica")` lookup uses the supplied music without a code change.
- Documented the extension-agnostic audio naming contract and the unassigned `death.mp3` resource in the backlog.

## [1.0.2] — 2026-11-15

### Added

- Eight-slide English presentation in `Docs/Presentacion/7_Nights_on_Mars_NASA_Space_Apps_2026.pptx`.
- Matching eight-page PDF export in `Docs/Presentacion/7_Nights_on_Mars_NASA_Space_Apps_2026.pdf`.
- Repeatable deck generator and presentation README, using current 16:9 gameplay captures and the Space Mission Design Game framing.

### Verified

- The PPTX contains eight slide XML files.
- The PDF contains eight page objects.

## [1.0.1] — 2026-11-15

### Added

- `Docs/Backlog.md`, with the audio user story, technical asset contract, acceptance criteria, presentation outline and delivery checklist.

### Verified

- Unity compiled the runtime and editor assemblies with the required Unity 6000.6.8f1 setup.
- **SieteNoches > Probar partida completa** completed with “partida completa sin errores”.
- The shared Scene List contains the nine required game scenes, all enabled.

## [1.0.0]

### Added

- Initial playable version of *7 Noches en Marte*: seven-night mission flow, four connected stations, eight endings and scientific mission report.
