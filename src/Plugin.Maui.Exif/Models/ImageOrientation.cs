namespace Plugin.Maui.Exif.Models;

/// <summary>
/// Represents the orientation of an image.
/// </summary>
public enum ImageOrientation
{
    /// <summary>
    /// Normal orientation (0°).
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Flipped horizontally.
    /// </summary>
    FlipHorizontal = 2,

    /// <summary>
    /// Rotated 180°.
    /// </summary>
    Rotate180 = 3,

    /// <summary>
    /// Flipped vertically.
    /// </summary>
    FlipVertical = 4,

    /// <summary>
    /// Flipped horizontally and rotated 90° clockwise.
    /// </summary>
    FlipHorizontalRotate90 = 5,

    /// <summary>
    /// Rotated 90° clockwise.
    /// </summary>
    Rotate90 = 6,

    /// <summary>
    /// Flipped horizontally and rotated 270° clockwise.
    /// </summary>
    FlipHorizontalRotate270 = 7,

    /// <summary>
    /// Rotated 270° clockwise.
    /// </summary>
    Rotate270 = 8
}
