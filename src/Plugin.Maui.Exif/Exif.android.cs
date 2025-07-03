using AndroidX.ExifInterface.Media;
using Plugin.Maui.Exif.Models;
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

        return await Task.Run(() =>
        {
            try
            {
                var exifInterface = new ExifInterface(filePath);
                return ExtractExifData(exifInterface);
            }
            catch (Exception)
            {
                return null;
            }
        });
    }

    public async Task<ExifData?> ReadFromStreamAsync(Stream stream)
    {
        if (stream is null)
        {
            return null;
        }

        return await Task.Run(() =>
        {
            try
            {
                var exifInterface = new ExifInterface(stream);
                return ExtractExifData(exifInterface);
            }
            catch (Exception)
            {
                return null;
            }
        });
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
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        return await Task.Run(() =>
        {
            try
            {
                var exifInterface = new ExifInterface(filePath);
                var gpsLat = exifInterface.GetAttribute(ExifInterface.TagGpsLatitude);
                var gpsLon = exifInterface.GetAttribute(ExifInterface.TagGpsLongitude);
                
                return !string.IsNullOrEmpty(gpsLat) && !string.IsNullOrEmpty(gpsLon);
            }
            catch (Exception)
            {
                return false;
            }
        });
    }

    public async Task<bool> HasGpsDataAsync(Stream stream)
    {
        if (stream is null)
        {
            return false;
        }

        return await Task.Run(() =>
        {
            try
            {
                var exifInterface = new ExifInterface(stream);
                var gpsLat = exifInterface.GetAttribute(ExifInterface.TagGpsLatitude);
                var gpsLon = exifInterface.GetAttribute(ExifInterface.TagGpsLongitude);
                
                return !string.IsNullOrEmpty(gpsLat) && !string.IsNullOrEmpty(gpsLon);
            }
            catch (Exception)
            {
                return false;
            }
        });
    }

    private static ExifData ExtractExifData(ExifInterface exifInterface)
    {
        var exifData = new ExifData
        {
            // Basic image properties
            Make = exifInterface.GetAttribute(ExifInterface.TagMake),
            Model = exifInterface.GetAttribute(ExifInterface.TagModel),
            Software = exifInterface.GetAttribute(ExifInterface.TagSoftware),
            Copyright = exifInterface.GetAttribute(ExifInterface.TagCopyright),
            ImageDescription = exifInterface.GetAttribute(ExifInterface.TagImageDescription),
            Artist = exifInterface.GetAttribute(ExifInterface.TagArtist)
        };

        // Date taken
        var dateTimeOriginal = exifInterface.GetAttribute(ExifInterface.TagDatetimeOriginal);
        if (!string.IsNullOrEmpty(dateTimeOriginal))
        {
            if (DateTime.TryParseExact(dateTimeOriginal, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                exifData.DateTaken = date;
            }
        }

        // Image dimensions
        if (int.TryParse(exifInterface.GetAttribute(ExifInterface.TagImageWidth), out var width))
        {
            exifData.Width = width;
        }

        if (int.TryParse(exifInterface.GetAttribute(ExifInterface.TagImageLength), out var height))
        {
            exifData.Height = height;
        }

        // Orientation
        var orientation = exifInterface.GetAttributeInt(ExifInterface.TagOrientation, ExifInterface.OrientationNormal);
        if (Enum.IsDefined(typeof(ImageOrientation), orientation))
        {
            exifData.Orientation = (ImageOrientation)orientation;
        }

        // Camera settings
        if (double.TryParse(exifInterface.GetAttribute(ExifInterface.TagFocalLength), out var focalLength))
        {
            exifData.FocalLength = focalLength;
        }

        if (double.TryParse(exifInterface.GetAttribute(ExifInterface.TagFNumber), out var fNumber))
        {
            exifData.FNumber = fNumber;
        }

        if (int.TryParse(exifInterface.GetAttribute(ExifInterface.TagIsoSpeed), out var iso))
        {
            exifData.Iso = iso;
        }

        // Exposure time (convert from fraction string like "1/60" to decimal)
        var exposureTimeStr = exifInterface.GetAttribute(ExifInterface.TagExposureTime);
        if (!string.IsNullOrEmpty(exposureTimeStr))
        {
            if (ParseFraction(exposureTimeStr, out var exposureTime))
            {
                exifData.ExposureTime = exposureTime;
            }
        }

        // Flash
        var flash = exifInterface.GetAttributeInt(ExifInterface.TagFlash, -1);
        if (flash != -1 && Enum.IsDefined(typeof(FlashMode), flash))
        {
            exifData.Flash = (FlashMode)flash;
        }

        // GPS data
        ExtractGpsData(exifInterface, exifData);

        // Add all available tags
        AddAllAvailableTags(exifInterface, exifData.AllTags);

        return exifData;
    }

    private static void ExtractGpsData(ExifInterface exifInterface, ExifData exifData)
    {
        // Get GPS coordinates
        var gpsLat = exifInterface.GetAttribute(ExifInterface.TagGpsLatitude);
        var gpsLatRef = exifInterface.GetAttribute(ExifInterface.TagGpsLatitudeRef);
        var gpsLon = exifInterface.GetAttribute(ExifInterface.TagGpsLongitude);
        var gpsLonRef = exifInterface.GetAttribute(ExifInterface.TagGpsLongitudeRef);

        if (!string.IsNullOrEmpty(gpsLat) && !string.IsNullOrEmpty(gpsLon))
        {
            if (ParseGpsCoordinate(gpsLat, out var latitude) && 
                ParseGpsCoordinate(gpsLon, out var longitude))
            {
                // Apply hemisphere reference
                if (gpsLatRef == "S")
                {
                    latitude = -latitude;
                }
                if (gpsLonRef == "W")
                {
                    longitude = -longitude;
                }

                exifData.Latitude = latitude;
                exifData.Longitude = longitude;
            }
        }

        // Get GPS altitude
        var gpsAltitude = exifInterface.GetAttribute(ExifInterface.TagGpsAltitude);
        var gpsAltitudeRef = exifInterface.GetAttribute(ExifInterface.TagGpsAltitudeRef);
        
        if (!string.IsNullOrEmpty(gpsAltitude) && ParseFraction(gpsAltitude, out var altitude))
        {
            // GPS altitude reference: 0 = above sea level, 1 = below sea level
            if (gpsAltitudeRef == "1")
            {
                altitude = -altitude;
            }
            
            exifData.Altitude = altitude;
        }
    }

    private static bool ParseGpsCoordinate(string coordinate, out double result)
    {
        result = 0;
        
        if (string.IsNullOrEmpty(coordinate))
        {
            return false;
        }

        // GPS coordinates are in the format "degrees/1,minutes/1,seconds/1"
        var parts = coordinate.Split(',');
        if (parts.Length != 3)
        {
            return false;
        }

        if (ParseFraction(parts[0], out var degrees) &&
            ParseFraction(parts[1], out var minutes) &&
            ParseFraction(parts[2], out var seconds))
        {
            result = degrees + (minutes / 60.0) + (seconds / 3600.0);
            return true;
        }

        return false;
    }

    private static bool ParseFraction(string fraction, out double result)
    {
        result = 0;
        
        if (string.IsNullOrEmpty(fraction))
        {
            return false;
        }

        if (fraction.Contains('/'))
        {
            var parts = fraction.Split('/');
            if (parts.Length == 2 && 
                double.TryParse(parts[0], out var numerator) && 
                double.TryParse(parts[1], out var denominator) && 
                denominator != 0)
            {
                result = numerator / denominator;
                return true;
            }
        }
        else if (double.TryParse(fraction, out result))
        {
            return true;
        }

        return false;
    }

    private static void AddAllAvailableTags(ExifInterface exifInterface, Dictionary<string, object?> allTags)
    {
        // Define all standard EXIF tags that we want to check
        var standardTags = new[]
        {
            ExifInterface.TagApertureValue,
            ExifInterface.TagArtist,
            ExifInterface.TagBitsPerSample,
            ExifInterface.TagColorSpace,
            ExifInterface.TagCompression,
            ExifInterface.TagCopyright,
            ExifInterface.TagDatetime,
            ExifInterface.TagDatetimeDigitized,
            ExifInterface.TagDatetimeOriginal,
            ExifInterface.TagExposureBiasValue,
            ExifInterface.TagExposureMode,
            ExifInterface.TagExposureProgram,
            ExifInterface.TagExposureTime,
            ExifInterface.TagFlash,
            ExifInterface.TagFNumber,
            ExifInterface.TagFocalLength,
            ExifInterface.TagGpsAltitude,
            ExifInterface.TagGpsAltitudeRef,
            ExifInterface.TagGpsAreaInformation,
            ExifInterface.TagGpsDatestamp,
            ExifInterface.TagGpsDestBearing,
            ExifInterface.TagGpsDestBearingRef,
            ExifInterface.TagGpsDestDistance,
            ExifInterface.TagGpsDestDistanceRef,
            ExifInterface.TagGpsDestLatitude,
            ExifInterface.TagGpsDestLatitudeRef,
            ExifInterface.TagGpsDestLongitude,
            ExifInterface.TagGpsDestLongitudeRef,
            ExifInterface.TagGpsDifferential,
            ExifInterface.TagGpsDop,
            ExifInterface.TagGpsImgDirection,
            ExifInterface.TagGpsImgDirectionRef,
            ExifInterface.TagGpsLatitude,
            ExifInterface.TagGpsLatitudeRef,
            ExifInterface.TagGpsLongitude,
            ExifInterface.TagGpsLongitudeRef,
            ExifInterface.TagGpsMapDatum,
            ExifInterface.TagGpsMeasureMode,
            ExifInterface.TagGpsProcessingMethod,
            ExifInterface.TagGpsSatellites,
            ExifInterface.TagGpsSpeed,
            ExifInterface.TagGpsSpeedRef,
            ExifInterface.TagGpsStatus,
            ExifInterface.TagGpsTimestamp,
            ExifInterface.TagGpsTrack,
            ExifInterface.TagGpsTrackRef,
            ExifInterface.TagGpsVersionId,
            ExifInterface.TagImageDescription,
            ExifInterface.TagImageLength,
            ExifInterface.TagImageWidth,
            ExifInterface.TagIsoSpeed,
            ExifInterface.TagMake,
            ExifInterface.TagMeteringMode,
            ExifInterface.TagModel,
            ExifInterface.TagOrientation,
            ExifInterface.TagPhotometricInterpretation,
            ExifInterface.TagResolutionUnit,
            ExifInterface.TagSamplesPerPixel,
            ExifInterface.TagSceneCaptureType,
            ExifInterface.TagSoftware,
            ExifInterface.TagSubsecTime,
            ExifInterface.TagSubsecTimeDigitized,
            ExifInterface.TagSubsecTimeOriginal,
            ExifInterface.TagWhiteBalance
        };

        foreach (var tag in standardTags)
        {
            var value = exifInterface.GetAttribute(tag);
            if (!string.IsNullOrEmpty(value))
            {
                allTags[tag] = value;
            }
        }
    }
}
