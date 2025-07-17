using Plugin.Maui.Exif.Models;

namespace Plugin.Maui.Exif;

/// <summary>
/// Interface for reading EXIF metadata from image files.
/// </summary>
public interface IExif
{
    /// <summary>
    /// Reads EXIF metadata from an image file.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <returns>The EXIF metadata, or null if no metadata is available.</returns>
    Task<ExifData?> ReadFromFileAsync(string filePath);

    /// <summary>
    /// Reads EXIF metadata from an image stream.
    /// </summary>
    /// <param name="stream">The stream containing the image data.</param>
    /// <returns>The EXIF metadata, or null if no metadata is available.</returns>
    Task<ExifData?> ReadFromStreamAsync(Stream stream);

    /// <summary>
    /// Checks if the specified file contains EXIF metadata.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <returns>True if the file contains EXIF metadata, false otherwise.</returns>
    Task<bool> HasExifDataAsync(string filePath);

    /// <summary>
    /// Checks if the specified stream contains EXIF metadata.
    /// </summary>
    /// <param name="stream">The stream containing the image data.</param>
    /// <returns>True if the stream contains EXIF metadata, false otherwise.</returns>
    Task<bool> HasExifDataAsync(Stream stream);

    /// <summary>
    /// Checks if the specified file contains GPS metadata.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <returns>True if the file contains GPS metadata, false otherwise.</returns>
    Task<bool> HasGpsDataAsync(string filePath);

    /// <summary>
    /// Checks if the specified stream contains GPS metadata.
    /// </summary>
    /// <param name="stream">The stream containing the image data.</param>
    /// <returns>True if the stream contains GPS metadata, false otherwise.</returns>
    Task<bool> HasGpsDataAsync(Stream stream);

    /// <summary>
    /// Writes EXIF metadata to an image file.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <param name="exifData">The EXIF data to write.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    Task<bool> WriteToFileAsync(string filePath, ExifData exifData);

    /// <summary>
    /// Writes EXIF metadata to an image stream and outputs the result to another stream.
    /// </summary>
    /// <param name="inputStream">The input stream containing the original image data.</param>
    /// <param name="outputStream">The output stream to write the image with EXIF data.</param>
    /// <param name="exifData">The EXIF data to write.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    Task<bool> WriteToStreamAsync(Stream inputStream, Stream outputStream, ExifData exifData);
}