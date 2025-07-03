using Plugin.Maui.Exif.Models;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Globalization;

namespace Plugin.Maui.Exif;

partial class ExifImplementation : IExif
{
    public async Task<ExifData?> ReadFromFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            using var stream = await file.OpenAsync(FileAccessMode.Read);
            return await ExtractExifDataFromStream(stream);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<ExifData?> ReadFromStreamAsync(Stream stream)
    {
        if (stream is null)
        {
            return null;
        }

        try
        {
            using var randomAccessStream = stream.AsRandomAccessStream();
            return await ExtractExifDataFromStream(randomAccessStream);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> HasExifDataAsync(string filePath)
    {
        var exifData = await ReadFromFileAsync(filePath);
        return exifData is not null && exifData.AllTags.Count > 0;
    }

    public async Task<bool> HasExifDataAsync(Stream stream)
    {
        var exifData = await ReadFromStreamAsync(stream);
        return exifData is not null && exifData.AllTags.Count > 0;
    }

    public async Task<bool> HasGpsDataAsync(string filePath)
    {
        var exifData = await ReadFromFileAsync(filePath);
        return exifData?.Latitude is not null && exifData?.Longitude is not null;
    }

    public async Task<bool> HasGpsDataAsync(Stream stream)
    {
        var exifData = await ReadFromStreamAsync(stream);
        return exifData?.Latitude is not null && exifData?.Longitude is not null;
    }

    private static async Task<ExifData?> ExtractExifDataFromStream(IRandomAccessStream stream)
    {
        try
        {
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var exifData = new ExifData();

            // Basic image properties
            exifData.Width = (int)decoder.PixelWidth;
            exifData.Height = (int)decoder.PixelHeight;

            // Get bitmap properties which contain EXIF data
            var properties = decoder.BitmapProperties;
            
            // Extract standard EXIF properties
            await ExtractBasicProperties(properties, exifData);
            await ExtractCameraProperties(properties, exifData);
            await ExtractGpsProperties(properties, exifData);

            // Get all available properties
            var allProperties = await properties.GetPropertiesAsync(new string[0]);
            foreach (var property in allProperties)
            {
                if (property.Value is not null)
                {
                    exifData.AllTags[property.Key] = ConvertBitmapTypedValue(property.Value);
                }
            }

            return exifData;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task ExtractBasicProperties(BitmapProperties properties, ExifData exifData)
    {
        var propertiesToRead = new[]
        {
            "System.Photo.CameraManufacturer",
            "System.Photo.CameraModel",
            "System.Photo.DateTaken",
            "System.Photo.Orientation",
            "System.ApplicationName",
            "System.Copyright",
            "System.Comment",
            "System.Author"
        };

        try
        {
            var results = await properties.GetPropertiesAsync(propertiesToRead);

            if (results.TryGetValue("System.Photo.CameraManufacturer", out var make))
            {
                exifData.Make = make?.Value?.ToString();
            }

            if (results.TryGetValue("System.Photo.CameraModel", out var model))
            {
                exifData.Model = model?.Value?.ToString();
            }

            if (results.TryGetValue("System.Photo.DateTaken", out var dateTaken))
            {
                if (dateTaken?.Value is DateTimeOffset dateTimeOffset)
                {
                    exifData.DateTaken = dateTimeOffset.DateTime;
                }
            }

            if (results.TryGetValue("System.Photo.Orientation", out var orientation))
            {
                if (orientation?.Value is ushort orientationValue && 
                    Enum.IsDefined(typeof(ImageOrientation), (int)orientationValue))
                {
                    exifData.Orientation = (ImageOrientation)orientationValue;
                }
            }

            if (results.TryGetValue("System.ApplicationName", out var software))
            {
                exifData.Software = software?.Value?.ToString();
            }

            if (results.TryGetValue("System.Copyright", out var copyright))
            {
                exifData.Copyright = copyright?.Value?.ToString();
            }

            if (results.TryGetValue("System.Comment", out var description))
            {
                exifData.ImageDescription = description?.Value?.ToString();
            }

            if (results.TryGetValue("System.Author", out var artist))
            {
                if (artist?.Value is string[] authors && authors.Length > 0)
                {
                    exifData.Artist = string.Join(", ", authors);
                }
            }
        }
        catch (Exception)
        {
            // Continue if some properties can't be read
        }
    }

    private static async Task ExtractCameraProperties(BitmapProperties properties, ExifData exifData)
    {
        var propertiesToRead = new[]
        {
            "System.Photo.FocalLength",
            "System.Photo.FNumber",
            "System.Photo.ISOSpeed",
            "System.Photo.ExposureTime",
            "System.Photo.Flash"
        };

        try
        {
            var results = await properties.GetPropertiesAsync(propertiesToRead);

            if (results.TryGetValue("System.Photo.FocalLength", out var focalLength))
            {
                if (focalLength?.Value is double focalLengthValue)
                {
                    exifData.FocalLength = focalLengthValue;
                }
            }

            if (results.TryGetValue("System.Photo.FNumber", out var fNumber))
            {
                if (fNumber?.Value is double fNumberValue)
                {
                    exifData.FNumber = fNumberValue;
                }
            }

            if (results.TryGetValue("System.Photo.ISOSpeed", out var iso))
            {
                if (iso?.Value is ushort isoValue)
                {
                    exifData.Iso = isoValue;
                }
            }

            if (results.TryGetValue("System.Photo.ExposureTime", out var exposureTime))
            {
                if (exposureTime?.Value is double exposureTimeValue)
                {
                    exifData.ExposureTime = exposureTimeValue;
                }
            }

            if (results.TryGetValue("System.Photo.Flash", out var flash))
            {
                if (flash?.Value is byte flashValue && 
                    Enum.IsDefined(typeof(FlashMode), (int)flashValue))
                {
                    exifData.Flash = (FlashMode)flashValue;
                }
            }
        }
        catch (Exception)
        {
            // Continue if some properties can't be read
        }
    }

    private static async Task ExtractGpsProperties(BitmapProperties properties, ExifData exifData)
    {
        var propertiesToRead = new[]
        {
            "System.GPS.Latitude",
            "System.GPS.Longitude",
            "System.GPS.Altitude"
        };

        try
        {
            var results = await properties.GetPropertiesAsync(propertiesToRead);

            if (results.TryGetValue("System.GPS.Latitude", out var latitude))
            {
                if (latitude?.Value is double[] latArray && latArray.Length >= 3)
                {
                    // Convert from degrees, minutes, seconds to decimal degrees
                    exifData.Latitude = latArray[0] + latArray[1] / 60.0 + latArray[2] / 3600.0;
                }
                else if (latitude?.Value is double latValue)
                {
                    exifData.Latitude = latValue;
                }
            }

            if (results.TryGetValue("System.GPS.Longitude", out var longitude))
            {
                if (longitude?.Value is double[] lonArray && lonArray.Length >= 3)
                {
                    // Convert from degrees, minutes, seconds to decimal degrees
                    exifData.Longitude = lonArray[0] + lonArray[1] / 60.0 + lonArray[2] / 3600.0;
                }
                else if (longitude?.Value is double lonValue)
                {
                    exifData.Longitude = lonValue;
                }
            }

            if (results.TryGetValue("System.GPS.Altitude", out var altitude))
            {
                if (altitude?.Value is double altValue)
                {
                    exifData.Altitude = altValue;
                }
            }
        }
        catch (Exception)
        {
            // Continue if GPS properties can't be read
        }
    }

    private static object? ConvertBitmapTypedValue(BitmapTypedValue typedValue)
    {
        return typedValue.Type switch
        {
            PropertyType.String => typedValue.Value?.ToString(),
            PropertyType.UInt16 => typedValue.Value,
            PropertyType.UInt32 => typedValue.Value,
            PropertyType.Int16 => typedValue.Value,
            PropertyType.Int32 => typedValue.Value,
            PropertyType.Single => typedValue.Value,
            PropertyType.Double => typedValue.Value,
            PropertyType.DateTime => typedValue.Value,
            PropertyType.BooleanArray => typedValue.Value,
            PropertyType.UInt16Array => typedValue.Value,
            PropertyType.UInt32Array => typedValue.Value,
            PropertyType.Int16Array => typedValue.Value,
            PropertyType.Int32Array => typedValue.Value,
            PropertyType.SingleArray => typedValue.Value,
            PropertyType.DoubleArray => typedValue.Value,
            PropertyType.StringArray => typedValue.Value,
            _ => typedValue.Value?.ToString()
        };
    }
}