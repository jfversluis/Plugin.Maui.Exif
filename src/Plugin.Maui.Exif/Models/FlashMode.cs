namespace Plugin.Maui.Exif.Models;

/// <summary>
/// Represents the flash mode used when taking a photo.
/// </summary>
public enum FlashMode
{
    /// <summary>
    /// Flash did not fire.
    /// </summary>
    None = 0,

    /// <summary>
    /// Flash fired.
    /// </summary>
    Fired = 1,

    /// <summary>
    /// Flash fired, return light not detected.
    /// </summary>
    FiredReturnNotDetected = 5,

    /// <summary>
    /// Flash fired, return light detected.
    /// </summary>
    FiredReturnDetected = 7,

    /// <summary>
    /// Flash did not fire, compulsory flash mode.
    /// </summary>
    CompulsoryNotFired = 16,

    /// <summary>
    /// Flash fired, compulsory flash mode.
    /// </summary>
    CompulsoryFired = 24,

    /// <summary>
    /// Flash fired, compulsory flash mode, return light not detected.
    /// </summary>
    CompulsoryFiredReturnNotDetected = 25,

    /// <summary>
    /// Flash fired, compulsory flash mode, return light detected.
    /// </summary>
    CompulsoryFiredReturnDetected = 31,

    /// <summary>
    /// Flash did not fire, auto mode.
    /// </summary>
    AutoNotFired = 32,

    /// <summary>
    /// Flash fired, auto mode.
    /// </summary>
    AutoFired = 48,

    /// <summary>
    /// Flash fired, auto mode, return light not detected.
    /// </summary>
    AutoFiredReturnNotDetected = 73,

    /// <summary>
    /// Flash fired, auto mode, return light detected.
    /// </summary>
    AutoFiredReturnDetected = 79
}
