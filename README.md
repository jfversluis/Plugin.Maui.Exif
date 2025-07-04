# Plugin.Maui.Exif

`Plugin.Maui.Exif` provides the ability to read EXIF metadata from image files in your .NET MAUI application across iOS, Android, and Windows platforms.

Learn more about this plugin in [this video](https://youtu.be/plT4Tsd6ORI).

## Features

- Read EXIF metadata from image files and streams
- Extract common metadata like camera make/model, date taken, GPS coordinates, camera settings
- Cross-platform support (iOS, macOS Catalyst, Android, Windows)
- Easy-to-use API with both static and dependency injection patterns
- Extension methods for common EXIF data operations

## Installation

```xml
<PackageReference Include="Plugin.Maui.Exif" Version="1.0.0" />
```

## Usage

### Basic Usage

```csharp
// Using the static API
var exifData = await Exif.Default.ReadFromFileAsync(imagePath);

// Using dependency injection
builder.Services.AddSingleton<IExif>(Exif.Default);

// In your class
public MainPage(IExif exif)
{
    var exifData = await exif.ReadFromFileAsync(imagePath);
}
```

### Reading EXIF Data

```csharp
var exifData = await Exif.Default.ReadFromFileAsync("path/to/image.jpg");

if (exifData != null)
{
    // Basic image info
    Console.WriteLine($"Camera: {exifData.Make} {exifData.Model}");
    Console.WriteLine($"Date taken: {exifData.DateTaken}");
    Console.WriteLine($"Dimensions: {exifData.Width}x{exifData.Height}");
    
    // Camera settings
    Console.WriteLine($"F-number: f/{exifData.FNumber}");
    Console.WriteLine($"Exposure time: {exifData.ExposureTime}s");
    Console.WriteLine($"ISO: {exifData.Iso}");
    Console.WriteLine($"Focal length: {exifData.FocalLength}mm");
    
    // GPS coordinates
    if (exifData.HasGpsCoordinates())
    {
        Console.WriteLine($"Location: {exifData.Latitude}, {exifData.Longitude}");
        Console.WriteLine($"Altitude: {exifData.Altitude}m");
    }
}
```

### Reading from Stream

```csharp
using var stream = File.OpenRead("path/to/image.jpg");
var exifData = await Exif.Default.ReadFromStreamAsync(stream);
```

### Extension Methods

The plugin includes useful extension methods:

```csharp
// Check if GPS coordinates are available
if (exifData.HasGpsCoordinates())
{
    // Get formatted GPS string
    var coordinates = exifData.GetFormattedGpsCoordinates();
    // Result: "37.421998°N, 122.084000°W"
}

// Get formatted camera settings
var settings = exifData.GetFormattedCameraSettings();
// Result: "f/2.2, 1/120s, ISO 100, 24mm"

// Get camera information
var camera = exifData.GetCameraInfo();
// Result: "Apple iPhone 12 Pro"

// Check if image needs rotation
if (exifData.NeedsRotation())
{
    var angle = exifData.GetRotationAngle();
    // Rotate the image by the returned angle
}
```

## Available EXIF Properties

### Basic Image Information
- `Width` / `Height` - Image dimensions
- `Orientation` - Image orientation
- `DateTaken` - Date and time the photo was taken

### Camera Information
- `Make` - Camera manufacturer
- `Model` - Camera model
- `Software` - Software used to process the image

### Camera Settings
- `FNumber` - Aperture f-number
- `ExposureTime` - Shutter speed in seconds
- `Iso` - ISO sensitivity
- `FocalLength` - Focal length in millimeters
- `Flash` - Flash mode used

### GPS Information
- `Latitude` - GPS latitude
- `Longitude` - GPS longitude
- `Altitude` - GPS altitude in meters

### Additional Information
- `Copyright` - Copyright information
- `Artist` - Photographer/artist name
- `ImageDescription` - Image description or comment
- `AllTags` - Dictionary containing all available EXIF tags

## Platform Support

| Platform | Supported | Implementation |
|----------|-----------|----------------|
| iOS | ✅ | ImageIO Framework |
| macOS Catalyst | ✅ | ImageIO Framework |
| Android | ✅ | AndroidX ExifInterface |
| Windows | ✅ | Windows Runtime BitmapDecoder |

## Permissions

### Android
Add the following permissions to your `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.READ_MEDIA_IMAGES" />
```
> [!NOTE]
> When using `MediaPicker.PickPhotoAsync()`, the selected image may not include GPS EXIF data due to Android privacy restrictions (API 29+).  
> To access GPS metadata in this case, you must declare and request the `ACCESS_MEDIA_LOCATION` runtime permission.
>
> When using `MediaPicker.CapturePhotoAsync()`, the GPS data may be missing unless your app has location permissions (`ACCESS_FINE_LOCATION` or `ACCESS_COARSE_LOCATION`) and the selected camera app supports embedding location info.  
> Note that the camera app behavior varies across devices and cannot be controlled by your app.

### iOS
No special permissions required for reading EXIF data from files accessible to your app.

### Windows
No special permissions required.

## Sample App

The repository includes a sample app demonstrating how to:
- Select images using FilePicker
- Read and display EXIF metadata
- Show formatted camera settings and GPS information
- Display all available EXIF tags

## Future Enhancements

- Writing EXIF metadata (planned for future versions)
- Additional metadata formats support
- Batch processing capabilities

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License. 

From there, after [creating a GitHub release](https://docs.github.com/repositories/releasing-projects-on-github/managing-releases-in-a-repository) your plugin will be automatically released on NuGet!
