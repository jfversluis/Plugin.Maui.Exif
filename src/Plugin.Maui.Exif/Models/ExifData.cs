namespace Plugin.Maui.Exif.Models;

/// <summary>
/// Represents EXIF metadata extracted from an image file.
/// </summary>
public class ExifData
{
    /// <summary>
    /// Camera make (manufacturer).
    /// </summary>
    public string? Make { get; set; }

    /// <summary>
    /// Camera model.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Date and time when the image was taken.
    /// </summary>
    public DateTime? DateTaken { get; set; }

    /// <summary>
    /// GPS latitude coordinate.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// GPS longitude coordinate.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// GPS altitude in meters.
    /// </summary>
    public double? Altitude { get; set; }

    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Image orientation.
    /// </summary>
    public ImageOrientation? Orientation { get; set; }

    /// <summary>
    /// Flash mode used when taking the photo.
    /// </summary>
    public FlashMode? Flash { get; set; }

    /// <summary>
    /// Focal length in millimeters.
    /// </summary>
    public double? FocalLength { get; set; }

    /// <summary>
    /// F-number (aperture).
    /// </summary>
    public double? FNumber { get; set; }

    /// <summary>
    /// ISO speed rating.
    /// </summary>
    public int? Iso { get; set; }

    /// <summary>
    /// Exposure time in seconds.
    /// </summary>
    public double? ExposureTime { get; set; }

    /// <summary>
    /// Software used to process the image.
    /// </summary>
    public string? Software { get; set; }

    /// <summary>
    /// Copyright information.
    /// </summary>
    public string? Copyright { get; set; }

    /// <summary>
    /// Image description or comment.
    /// </summary>
    public string? ImageDescription { get; set; }

    /// <summary>
    /// Artist/photographer name.
    /// </summary>
    public string? Artist { get; set; }

    /// <summary>
    /// All available EXIF tags as key-value pairs.
    /// </summary>
    public Dictionary<string, object?> AllTags { get; set; } = new Dictionary<string, object?>();
}
