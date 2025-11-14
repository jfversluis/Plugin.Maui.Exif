using Plugin.Maui.Exif.Models;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Globalization;
using Windows.Foundation;
using Windows.Foundation.Collections;

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

            // Get all Standard properties
            string[] allExifPropertyKeys = new[]
            {
                // Basic Properties
                "System.Photo.CameraManufacturer",
                "System.Photo.CameraModel",
                "System.Photo.DateTaken",
                "System.Photo.Orientation",
                "System.ApplicationName",
                "System.Copyright",
                "System.Comment",
                "System.Author",

                // Camera Properties
                "System.Photo.FocalLength",
                "System.Photo.FNumber",
                "System.Photo.ISOSpeed",
                "System.Photo.ExposureTime",
                "System.Photo.Flash",

                // GPS Properties
                "System.GPS.Latitude",
                "System.GPS.Longitude",
                "System.GPS.Altitude",
                "System.GPS.LatitudeRef",
                "System.GPS.LongitudeRef",
                "System.GPS.AltitudeRef"
            };
            var allProperties = await properties.GetPropertiesAsync(allExifPropertyKeys);
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

    private static async Task ExtractBasicProperties(BitmapPropertiesView properties, ExifData exifData)
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

    private static async Task ExtractCameraProperties(BitmapPropertiesView properties, ExifData exifData)
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

    private static async Task ExtractGpsProperties(BitmapPropertiesView properties, ExifData exifData)
    {
        var propertiesToRead = new[]
        {
            "System.GPS.Latitude",
            "System.GPS.Longitude",
            "System.GPS.Altitude",
            "System.GPS.LatitudeRef",
            "System.GPS.LongitudeRef",
            "System.GPS.AltitudeRef"
        };

        try
        {
            var results = await properties.GetPropertiesAsync(propertiesToRead);

            if (results.TryGetValue("System.GPS.Latitude", out var latitude))
            {
                double? latValue = null;
                if (latitude?.Value is double[] latArray && latArray.Length >= 3)
                {
                    // Convert from degrees, minutes, seconds to decimal degrees
                    latValue = latArray[0] + latArray[1] / 60.0 + latArray[2] / 3600.0;
                }
                else if (latitude?.Value is double latDouble)
                {
                    latValue = latDouble;
                }

                if (latValue.HasValue)
                {
                    // Apply hemisphere reference (N/S)
                    if (results.TryGetValue("System.GPS.LatitudeRef", out var latRef) && 
                        latRef?.Value?.ToString() == "S")
                    {
                        latValue = -latValue.Value;
                    }
                    exifData.Latitude = latValue;
                }
            }

            if (results.TryGetValue("System.GPS.Longitude", out var longitude))
            {
                double? lonValue = null;
                if (longitude?.Value is double[] lonArray && lonArray.Length >= 3)
                {
                    // Convert from degrees, minutes, seconds to decimal degrees
                    lonValue = lonArray[0] + lonArray[1] / 60.0 + lonArray[2] / 3600.0;
                }
                else if (longitude?.Value is double lonDouble)
                {
                    lonValue = lonDouble;
                }

                if (lonValue.HasValue)
                {
                    // Apply hemisphere reference (E/W)
                    if (results.TryGetValue("System.GPS.LongitudeRef", out var lonRef) && 
                        lonRef?.Value?.ToString() == "W")
                    {
                        lonValue = -lonValue.Value;
                    }
                    exifData.Longitude = lonValue;
                }
            }

            if (results.TryGetValue("System.GPS.Altitude", out var altitude))
            {
                if (altitude?.Value is double altValue)
                {
                    // Apply altitude reference (0 = above sea level, 1 = below sea level)
                    if (results.TryGetValue("System.GPS.AltitudeRef", out var altRef))
                    {
                        if (altRef?.Value is byte altRefByte && altRefByte == 1)
                        {
                            altValue = -altValue;
                        }
                    }
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

    public async Task<bool> WriteToFileAsync(string filePath, ExifData exifData)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || exifData is null)
        {
            return false;
        }

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            
            // Read the original image
            using var inputStream = await file.OpenAsync(FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(inputStream);
            
            // Create a temporary file for writing
            var parentFolder = await file.GetParentAsync();
            var tempFile = await parentFolder.CreateFileAsync($"{file.Name}.tmp", 
                CreationCollisionOption.ReplaceExisting);
            
            try
            {
                using var outputStream = await tempFile.OpenAsync(FileAccessMode.ReadWrite);
                var success = await WriteImageWithExifData(decoder, outputStream, exifData);
                
                if (success)
                {
                    // Replace the original file with the temporary file
                    await tempFile.MoveAndReplaceAsync(file);
                    return true;
                }
                else
                {
                    await tempFile.DeleteAsync();
                    return false;
                }
            }
            catch
            {
                try
                {
                    await tempFile.DeleteAsync();
                }
                catch
                {
                    // Ignore cleanup errors
                }
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> WriteToStreamAsync(Stream inputStream, Stream outputStream, ExifData exifData)
    {
        if (inputStream is null || outputStream is null || exifData is null)
        {
            return false;
        }

        try
        {
            using var randomAccessInputStream = inputStream.AsRandomAccessStream();
            var decoder = await BitmapDecoder.CreateAsync(randomAccessInputStream);
            
            using var randomAccessOutputStream = outputStream.AsRandomAccessStream();
            return await WriteImageWithExifData(decoder, randomAccessOutputStream, exifData);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<bool> WriteImageWithExifData(BitmapDecoder decoder, IRandomAccessStream outputStream, ExifData exifData)
    {
        try
        {
            // Get the encoder ID based on the original format
            var encoderId = GetEncoderIdFromDecoder(decoder);
            
            var encoder = await BitmapEncoder.CreateAsync(encoderId, outputStream);
            
            // Get the pixel data from the original image
            var pixelDataProvider = await decoder.GetPixelDataAsync();
            var pixelData = pixelDataProvider.DetachPixelData();
            
            // Set the pixel data
            encoder.SetPixelData(
                decoder.BitmapPixelFormat,
                decoder.BitmapAlphaMode,
                decoder.PixelWidth,
                decoder.PixelHeight,
                decoder.DpiX,
                decoder.DpiY,
                pixelData);

            // Set EXIF properties
            await SetExifProperties(encoder.BitmapProperties, exifData);
            
            await encoder.FlushAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static Guid GetEncoderIdFromDecoder(BitmapDecoder decoder)
    {
        // Map decoder codec IDs to encoder codec IDs
        return decoder.DecoderInformation.CodecId switch
        {
            _ when decoder.DecoderInformation.CodecId == BitmapDecoder.JpegDecoderId => BitmapEncoder.JpegEncoderId,
            _ when decoder.DecoderInformation.CodecId == BitmapDecoder.PngDecoderId => BitmapEncoder.PngEncoderId,
            _ when decoder.DecoderInformation.CodecId == BitmapDecoder.TiffDecoderId => BitmapEncoder.TiffEncoderId,
            _ when decoder.DecoderInformation.CodecId == BitmapDecoder.BmpDecoderId => BitmapEncoder.BmpEncoderId,
            _ => BitmapEncoder.JpegEncoderId // Default to JPEG
        };
    }

    private static double[] ConvertDecimalDegreesToDMS(double decimalDegrees)
    {
        // Convert decimal degrees to degrees, minutes, seconds format
        var degrees = Math.Floor(decimalDegrees);
        var minutesDecimal = (decimalDegrees - degrees) * 60;
        var minutes = Math.Floor(minutesDecimal);
        var seconds = (minutesDecimal - minutes) * 60;
        
        return new double[] { degrees, minutes, seconds };
    }

    private static async Task SetExifProperties(BitmapProperties properties, ExifData exifData)
    {
        var propertiesToSet = new Dictionary<string, object>();

        // Basic properties
        if (!string.IsNullOrEmpty(exifData.Make))
        {
            propertiesToSet["System.Photo.CameraManufacturer"] = exifData.Make;
        }

        if (!string.IsNullOrEmpty(exifData.Model))
        {
            propertiesToSet["System.Photo.CameraModel"] = exifData.Model;
        }

        if (exifData.DateTaken.HasValue)
        {
            propertiesToSet["System.Photo.DateTaken"] = new DateTimeOffset(exifData.DateTaken.Value);
        }

        if (exifData.Orientation.HasValue)
        {
            propertiesToSet["System.Photo.Orientation"] = (ushort)exifData.Orientation.Value;
        }

        if (!string.IsNullOrEmpty(exifData.Software))
        {
            propertiesToSet["System.ApplicationName"] = exifData.Software;
        }

        if (!string.IsNullOrEmpty(exifData.Copyright))
        {
            propertiesToSet["System.Copyright"] = exifData.Copyright;
        }

        if (!string.IsNullOrEmpty(exifData.ImageDescription))
        {
            propertiesToSet["System.Comment"] = exifData.ImageDescription;
        }

        if (!string.IsNullOrEmpty(exifData.Artist))
        {
            propertiesToSet["System.Author"] = new string[] { exifData.Artist };
        }

        // Camera settings
        if (exifData.FocalLength.HasValue)
        {
            propertiesToSet["System.Photo.FocalLength"] = exifData.FocalLength.Value;
        }

        if (exifData.FNumber.HasValue)
        {
            propertiesToSet["System.Photo.FNumber"] = exifData.FNumber.Value;
        }

        if (exifData.Iso.HasValue)
        {
            propertiesToSet["System.Photo.ISOSpeed"] = (ushort)exifData.Iso.Value;
        }

        if (exifData.ExposureTime.HasValue)
        {
            propertiesToSet["System.Photo.ExposureTime"] = exifData.ExposureTime.Value;
        }

        if (exifData.Flash.HasValue)
        {
            propertiesToSet["System.Photo.Flash"] = (byte)exifData.Flash.Value;
        }

        // GPS properties
        if (exifData.Latitude.HasValue)
        {
            var latDMS = ConvertDecimalDegreesToDMS(Math.Abs(exifData.Latitude.Value));
            propertiesToSet["System.GPS.Latitude"] = latDMS;
            propertiesToSet["System.GPS.LatitudeRef"] = exifData.Latitude.Value >= 0 ? "N" : "S";
        }

        if (exifData.Longitude.HasValue)
        {
            var lonDMS = ConvertDecimalDegreesToDMS(Math.Abs(exifData.Longitude.Value));
            propertiesToSet["System.GPS.Longitude"] = lonDMS;
            propertiesToSet["System.GPS.LongitudeRef"] = exifData.Longitude.Value >= 0 ? "E" : "W";
        }

        if (exifData.Altitude.HasValue)
        {
            propertiesToSet["System.GPS.Altitude"] = Math.Abs(exifData.Altitude.Value);
            propertiesToSet["System.GPS.AltitudeRef"] = exifData.Altitude.Value >= 0 ? (byte)0 : (byte)1;
        }

        // Set all properties at once
        try
        {
            var convertedProperties = propertiesToSet.Select(kvp => 
                new KeyValuePair<string, BitmapTypedValue>(kvp.Key, 
                    kvp.Value switch
                    {
                        string s => new BitmapTypedValue(s, PropertyType.String),
                        int i => new BitmapTypedValue(i, PropertyType.Int32),
                        uint ui => new BitmapTypedValue(ui, PropertyType.UInt32),
                        ushort us => new BitmapTypedValue(us, PropertyType.UInt16),
                        byte b => new BitmapTypedValue(b, PropertyType.UInt8),
                        double d => new BitmapTypedValue(d, PropertyType.Double),
                        DateTimeOffset dto => new BitmapTypedValue(dto, PropertyType.DateTime),
                        double[] da => new BitmapTypedValue(da, PropertyType.DoubleArray),
                        string[] sa => new BitmapTypedValue(sa, PropertyType.StringArray),
                        _ => new BitmapTypedValue(kvp.Value, PropertyType.String)
                    }));
            await properties.SetPropertiesAsync(convertedProperties);
        }
        catch (Exception)
        {
            // Some properties might not be settable, continue anyway
        }
    }
}