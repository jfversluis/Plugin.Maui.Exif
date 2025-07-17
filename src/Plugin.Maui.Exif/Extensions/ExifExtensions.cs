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

    /// <summary>
    /// Creates a new ExifData instance with updated camera information.
    /// </summary>
    /// <param name="exifData">The original EXIF data.</param>
    /// <param name="make">The camera make.</param>
    /// <param name="model">The camera model.</param>
    /// <returns>A new ExifData instance with updated camera information.</returns>
    public static ExifData WithCameraInfo(this ExifData exifData, string? make = null, string? model = null)
    {
        var newExifData = CloneExifData(exifData);
        if (make is not null) newExifData.Make = make;
        if (model is not null) newExifData.Model = model;
        return newExifData;
    }

    /// <summary>
    /// Creates a new ExifData instance with updated GPS coordinates.
    /// </summary>
    /// <param name="exifData">The original EXIF data.</param>
    /// <param name="latitude">The latitude coordinate.</param>
    /// <param name="longitude">The longitude coordinate.</param>
    /// <param name="altitude">The altitude in meters (optional).</param>
    /// <returns>A new ExifData instance with updated GPS coordinates.</returns>
    public static ExifData WithGpsCoordinates(this ExifData exifData, double latitude, double longitude, double? altitude = null)
    {
        var newExifData = CloneExifData(exifData);
        newExifData.Latitude = latitude;
        newExifData.Longitude = longitude;
        if (altitude.HasValue) newExifData.Altitude = altitude.Value;
        return newExifData;
    }

    /// <summary>
    /// Creates a new ExifData instance with updated metadata.
    /// </summary>
    /// <param name="exifData">The original EXIF data.</param>
    /// <param name="artist">The artist/photographer name.</param>
    /// <param name="copyright">The copyright information.</param>
    /// <param name="imageDescription">The image description.</param>
    /// <returns>A new ExifData instance with updated metadata.</returns>
    public static ExifData WithMetadata(this ExifData exifData, string? artist = null, string? copyright = null, string? imageDescription = null)
    {
        var newExifData = CloneExifData(exifData);
        if (artist is not null) newExifData.Artist = artist;
        if (copyright is not null) newExifData.Copyright = copyright;
        if (imageDescription is not null) newExifData.ImageDescription = imageDescription;
        return newExifData;
    }

    /// <summary>
    /// Creates a new ExifData instance with updated date taken.
    /// </summary>
    /// <param name="exifData">The original EXIF data.</param>
    /// <param name="dateTaken">The date when the photo was taken.</param>
    /// <returns>A new ExifData instance with updated date taken.</returns>
    public static ExifData WithDateTaken(this ExifData exifData, DateTime dateTaken)
    {
        var newExifData = CloneExifData(exifData);
        newExifData.DateTaken = dateTaken;
        return newExifData;
    }

    /// <summary>
    /// Creates a new ExifData instance with camera settings.
    /// </summary>
    /// <param name="exifData">The original EXIF data.</param>
    /// <param name="fNumber">The aperture f-number.</param>
    /// <param name="exposureTime">The exposure time in seconds.</param>
    /// <param name="iso">The ISO speed rating.</param>
    /// <param name="focalLength">The focal length in millimeters.</param>
    /// <returns>A new ExifData instance with updated camera settings.</returns>
    public static ExifData WithCameraSettings(this ExifData exifData, double? fNumber = null, double? exposureTime = null, int? iso = null, double? focalLength = null)
    {
        var newExifData = CloneExifData(exifData);
        if (fNumber.HasValue) newExifData.FNumber = fNumber.Value;
        if (exposureTime.HasValue) newExifData.ExposureTime = exposureTime.Value;
        if (iso.HasValue) newExifData.Iso = iso.Value;
        if (focalLength.HasValue) newExifData.FocalLength = focalLength.Value;
        return newExifData;
    }

    /// <summary>
    /// Removes GPS coordinates from the EXIF data.
    /// </summary>
    /// <param name="exifData">The original EXIF data.</param>
    /// <returns>A new ExifData instance without GPS coordinates.</returns>
    public static ExifData WithoutGpsCoordinates(this ExifData exifData)
    {
        var newExifData = CloneExifData(exifData);
        newExifData.Latitude = null;
        newExifData.Longitude = null;
        newExifData.Altitude = null;
        return newExifData;
    }

    private static ExifData CloneExifData(ExifData source)
    {
        return new ExifData
        {
            Make = source.Make,
            Model = source.Model,
            DateTaken = source.DateTaken,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            Altitude = source.Altitude,
            Width = source.Width,
            Height = source.Height,
            Orientation = source.Orientation,
            Flash = source.Flash,
            FocalLength = source.FocalLength,
            FNumber = source.FNumber,
            Iso = source.Iso,
            ExposureTime = source.ExposureTime,
            Software = source.Software,
            Copyright = source.Copyright,
            ImageDescription = source.ImageDescription,
            Artist = source.Artist,
            AllTags = new Dictionary<string, object?>(source.AllTags)
        };
    }
}
