# 7 Nights on Mars — presentation assets

This folder contains the editable PowerPoint deck and its PDF export.

## Files

- `7_Nights_on_Mars_NASA_Space_Apps_2026.pptx` — 16:9 editable deck.
- `7_Nights_on_Mars_NASA_Space_Apps_2026.pdf` — eight-page review/export copy.
- `generate-presentation.mjs` — repeatable generator for both deliverables.

## Design and content

- Language: English.
- Challenge framing: **Space Mission Design Game**.
- Visual palette: habitat night `#121A2E`, Space Apps purple `#2C006B`, energy yellow `#F2C14E`, alert red `#E5484D`, off-white `#F4F1EA` and success green `#4CAF7A`.
- Game images: `../Capturas/`, produced by this project.
- The Space Apps brand lockup on slides 1, 2 and 8 reproduces the placement and colors of the image supplied in the planning conversation. The conversation attachment was not available as a source file, so it is drawn as editable vector elements. Replace it with the approved original logo asset before external publication if brand guidelines require the exact file.

## Regeneration

The deck is generated with `pptxgenjs` and `pdfkit`. Install the documented dependencies, then run the generator:

```powershell
Set-Location Docs/Presentacion
npm install
node generate-presentation.mjs
```

Review the PPTX and PDF visually after regeneration. The source captures may change as the game evolves.
