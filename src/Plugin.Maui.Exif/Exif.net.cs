using Plugin.Maui.Exif.Models;

namespace Plugin.Maui.Exif;

partial class ExifImplementation : IExif
{
    public Task<ExifData?> ReadFromFileAsync(string filePath)
    {
        throw new NotImplementedException("EXIF reading is not supported on this platform. This plugin requires iOS, Android, or Windows.");
    }

    public Task<ExifData?> ReadFromStreamAsync(Stream stream)
    {
        throw new NotImplementedException("EXIF reading is not supported on this platform. This plugin requires iOS, Android, or Windows.");
    }

    public Task<bool> HasExifDataAsync(string filePath)
    {
        throw new NotImplementedException("EXIF reading is not supported on this platform. This plugin requires iOS, Android, or Windows.");
    }

    public Task<bool> HasExifDataAsync(Stream stream)
    {
        throw new NotImplementedException("EXIF reading is not supported on this platform. This plugin requires iOS, Android, or Windows.");
    }

    public Task<bool> HasGpsDataAsync(string filePath)
    {
        throw new NotImplementedException("EXIF reading is not supported on this platform. This plugin requires iOS, Android, or Windows.");
    }

    public Task<bool> HasGpsDataAsync(Stream stream)
    {
        throw new NotImplementedException("EXIF reading is not supported on this platform. This plugin requires iOS, Android, or Windows.");
    }
}