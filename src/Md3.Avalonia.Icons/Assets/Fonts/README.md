# Offline Material Symbols font slot

This repository intentionally does **not** contain the Material Symbols font binary.

Before building `Md3.Avalonia.Icons`, obtain the official Google **Material Symbols Rounded** variable font under the Apache-2.0 license and copy/rename it to:

```text
src/Md3.Avalonia.Icons/Assets/Fonts/MaterialSymbolsRounded.ttf
```

The expected source file is commonly named:

```text
MaterialSymbolsRounded[FILL,GRAD,opsz,wght].ttf
```

Alternatively, keep the font outside the repository and pass its absolute path:

```bash
dotnet build src/Md3.Avalonia.Icons/Md3.Avalonia.Icons.csproj \
  -p:MaterialSymbolsRoundedFontFile=/absolute/path/MaterialSymbolsRounded.ttf
```

The package scripts can use an external font without copying it:

```bash
scripts/build-nuget.sh --font /absolute/path/MaterialSymbolsRounded.ttf
```

```powershell
.\scripts\build-nuget.ps1 -Font "D:\OfflineAssets\MaterialSymbolsRounded.ttf"
```

The build fails deliberately when the file is missing. Do not replace it with a look-alike Unicode font.

The retained `MaterialSymbolsRounded-LICENSE.txt` applies to the official font. Verify the source and checksum of the offline asset according to your release process.
