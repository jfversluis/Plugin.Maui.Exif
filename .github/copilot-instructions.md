# $ Copilot InstructionsREPO 

## Project Overview

This is a .NET MAUI plugin that provides the ability to read EXIF metadata from image files. It targets Android, iOS, macOS (Catalyst), Windows.

### Architecture

Core interface: ` ReadFromFileAsync, ReadFromStreamAsync, HasExifDataAsync, HasGpsDataAsync.IExif` 
Models: `ExifData`, `ImageOrientation`, `FlashMode`.

- Android: ExifInterface
- iOS/macOS: ImageIO CGImageSource
- Windows: BitmapDecoder metadata

## Code Conventions

### Namespace
All code uses: `Plugin.Maui.Exif`

### File Naming
- `*.shared. Cross-platform codecs` 
- `*.android. Androidcs` 
- `*.macios. iOS/macOScs` 
- `*.windows. Windowscs` 
- `*.net. Generic .NET fallbackcs` 

### Standards
- File-scoped namespaces
- `camelCase` for private fields, `PascalCase` for public
- XML docs required on all public APIs
- Null-conditional operators for platform interop

## Building

```bash
dotnet build src/Plugin.Maui.Exif/Plugin.Maui.Exif.csproj -c Release
```

## When Making Changes
1. Ensure the plugin builds on all target platforms
2. If adding public API, update the interface
3. Implement on all supported platforms
4. Update sample app and README
