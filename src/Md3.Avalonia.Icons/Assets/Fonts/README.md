# Embedded Material Symbols Rounded font

`MaterialSymbolsRounded.ttf` is the complete, unmodified Google **Material Symbols Rounded** variable TrueType font and is embedded in `Md3.Avalonia.Icons`. Consumers do not need to download or register a font manually.

Pinned upstream asset:

- Repository: <https://github.com/google/material-design-icons>
- Commit: `737e3324305806514d7909874fa1818ae1808232`
- Git blob: `101997d7897c7c9864b3bdd2262320bddcdc11db`
- Path: `variablefont/MaterialSymbolsRounded[FILL,GRAD,opsz,wght].ttf`
- Version recorded by the font: `2.973`
- SHA-256: `95b24392bb49efd1bc3e92cff4e2452ad094461bab7c97e7d8723fab97e330ca`
- Glyphs: `6646`; Unicode cmap entries: `4405`
- Variable axes: `FILL`, `GRAD`, `opsz`, `wght`

`Md3.Avalonia.Icons.Lite` embeds a 45-codepoint, real-outline subset generated from this exact upstream file. It is not a placeholder font. Both package assets retain the family name `Material Symbols Rounded` and are covered by `MaterialSymbolsRounded-LICENSE.txt` (Apache-2.0).

Run the release gate from the repository root:

```bash
python3 scripts/verify-fonts.py
```

The verifier pins both checksums, requires the real variable-font tables, and rejects missing, locally fabricated, or substituted binaries. Updating the font requires an explicit upstream provenance/checksum update and regeneration of the Lite subset from the same source.
