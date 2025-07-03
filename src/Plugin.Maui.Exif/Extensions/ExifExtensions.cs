using Plugin.Maui.Exif.Models;

namespace Plugin.Maui.Exif.Extensions;

/// <summary>
/// Extension methods for working with EXIF data.
/// </summary>
public static class ExifExtensions
{
    /// <summary>
    /// Checks if the image has GPS coordinates.
    /// </summary>
    /// <param name="exifData">The EXIF data to check.</param>
    /// <returns>True if both latitude and longitude are available.</returns>
    public static bool HasGpsCoordinates(this ExifData exifData)
    {
        return exifData.Latitude.HasValue && exifData.Longitude.HasValue;
    }

    /// <summary>
    /// Gets a formatted GPS coordinates string.
    /// </summary>
    /// <param name="exifData">The EXIF data.</param>
    /// <returns>A formatted GPS coordinates string, or null if no GPS data is available.</returns>
    public static string? GetFormattedGpsCoordinates(this ExifData exifData)
    {
        if (!exifData.HasGpsCoordinates())
        {
            return null;
        }

        var latDirection = exifData.Latitude >= 0 ? "N" : "S";
        var lonDirection = exifData.Longitude >= 0 ? "E" : "W";

        return $"{Math.Abs(exifData.Latitude!.Value):F6}°{latDirection}, {Math.Abs(exifData.Longitude!.Value):F6}°{lonDirection}";
    }

    /// <summary>
    /// Gets a formatted camera settings string.
    /// </summary>
    /// <param name="exifData">The EXIF data.</param>
    /// <returns>A formatted camera settings string with available information.</returns>
    public static string GetFormattedCameraSettings(this ExifData exifData)
    {
        var settings = new List<string>();

        if (exifData.FNumber.HasValue)
        {
            settings.Add($"f/{exifData.FNumber.Value:F1}");
        }

        if (exifData.ExposureTime.HasValue)
        {
            if (exifData.ExposureTime.Value >= 1)
            {
                settings.Add($"{exifData.ExposureTime.Value:F1}s");
            }
            else
            {
                settings.Add($"1/{Math.Round(1 / exifData.ExposureTime.Value)}s");
            }
        }

        if (exifData.Iso.HasValue)
        {
            settings.Add($"ISO {exifData.Iso.Value}");
        }

        if (exifData.FocalLength.HasValue)
        {
            settings.Add($"{exifData.FocalLength.Value:F0}mm");
        }

        return settings.Count > 0 ? string.Join(", ", settings) : "No camera settings available";
    }

    /// <summary>
    /// Gets the camera information string.
    /// </summary>
    /// <param name="exifData">The EXIF data.</param>
    /// <returns>A formatted camera information string.</returns>
    public static string GetCameraInfo(this ExifData exifData)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(exifData.Make))
        {
            parts.Add(exifData.Make);
        }

        if (!string.IsNullOrEmpty(exifData.Model))
        {
            parts.Add(exifData.Model);
        }

        return parts.Count > 0 ? string.Join(" ", parts) : "Unknown camera";
    }

    /// <summary>
    /// Checks if the image needs rotation based on EXIF orientation.
    /// </summary>
    /// <param name="exifData">The EXIF data.</param>
    /// <returns>True if the image needs rotation.</returns>
    public static bool NeedsRotation(this ExifData exifData)
    {
        return exifData.Orientation.HasValue && 
               exifData.Orientation != ImageOrientation.Normal;
    }

    /// <summary>
    /// Gets the rotation angle in degrees based on EXIF orientation.
    /// </summary>
    /// <param name="exifData">The EXIF data.</param>
    /// <returns>The rotation angle in degrees.</returns>
    public static int GetRotationAngle(this ExifData exifData)
    {
        return exifData.Orientation switch
        {
            ImageOrientation.Rotate90 => 90,
            ImageOrientation.Rotate180 => 180,
            ImageOrientation.Rotate270 => 270,
            ImageOrientation.FlipHorizontalRotate90 => 90,
            ImageOrientation.FlipHorizontalRotate270 => 270,
            _ => 0
        };
    }
}
