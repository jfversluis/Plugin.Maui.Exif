using Foundation;
using ImageIO;
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
            using var url = NSUrl.FromFilename(filePath);
            using var imageSource = CGImageSource.FromUrl(url);
            
            if (imageSource is null)
            {
                return null;
            }

            return ExtractExifData(imageSource);
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
            using var data = NSData.FromStream(stream);
            if (data is null)
            {
                return null;
            }
                
            using var imageSource = CGImageSource.FromData(data);
            
            if (imageSource is null)
            {
                return null;
            }

            return ExtractExifData(imageSource);
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
        var exifData = await ReadFromFileAsync(filePath);
        return exifData?.Latitude is not null && exifData?.Longitude is not null;
    }

    public async Task<bool> HasGpsDataAsync(Stream stream)
    {
        var exifData = await ReadFromStreamAsync(stream);
        return exifData?.Latitude is not null && exifData?.Longitude is not null;
    }

    private static ExifData? ExtractExifData(CGImageSource imageSource)
    {
        var properties = imageSource.GetProperties(0);
        if (properties is null)
        {
            return null;
        }

        var exifData = new ExifData();

        // Extract basic image properties
        if (properties.PixelWidth.HasValue)
        {
            exifData.Width = (int)properties.PixelWidth.Value;
        }

        if (properties.PixelHeight.HasValue)
        {
            exifData.Height = (int)properties.PixelHeight.Value;
        }

        if (properties.Orientation.HasValue)
        {
            var orientation = (int)properties.Orientation.Value;
            if (Enum.IsDefined(typeof(ImageOrientation), orientation))
            {
                exifData.Orientation = (ImageOrientation)orientation;
            }
        }

        // Extract EXIF properties
        var propertyDict = properties.Dictionary;
        if (propertyDict is not null)
        {
            var exifKey = new NSString("{Exif}");
            if (propertyDict.ContainsKey(exifKey) && propertyDict[exifKey] is NSDictionary exifDict)
            {
                ExtractExifProperties(exifDict, exifData);
            }

            // Extract TIFF properties
            var tiffKey = new NSString("{TIFF}");
            if (propertyDict.ContainsKey(tiffKey) && propertyDict[tiffKey] is NSDictionary tiffDict)
            {
                ExtractTiffProperties(tiffDict, exifData);
            }

            // Extract GPS properties
            var gpsKey = new NSString("{GPS}");
            if (propertyDict.ContainsKey(gpsKey) && propertyDict[gpsKey] is NSDictionary gpsDict)
            {
                ExtractGpsProperties(gpsDict, exifData);
            }

            // Add all properties to AllTags
            AddPropertiesToAllTags(propertyDict, exifData.AllTags);
        }

        return exifData;
    }

    private static void ExtractExifProperties(NSDictionary exifDict, ExifData exifData)
    {
        var dateKey = new NSString("DateTimeOriginal");
        if (exifDict.ContainsKey(dateKey) && exifDict[dateKey] is NSString dateString)
        {
            if (DateTime.TryParseExact(dateString.ToString(), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                exifData.DateTaken = date;
            }
        }

        var focalLengthKey = new NSString("FocalLength");
        if (exifDict.ContainsKey(focalLengthKey) && exifDict[focalLengthKey] is NSNumber focalLengthNumber)
        {
            exifData.FocalLength = focalLengthNumber.DoubleValue;
        }

        var fNumberKey = new NSString("FNumber");
        if (exifDict.ContainsKey(fNumberKey) && exifDict[fNumberKey] is NSNumber fNumberNumber)
        {
            exifData.FNumber = fNumberNumber.DoubleValue;
        }

        var isoKey = new NSString("ISOSpeedRatings");
        if (exifDict.ContainsKey(isoKey) && exifDict[isoKey] is NSArray isoArray && isoArray.Count > 0)
        {
            var isoNumber = isoArray.GetItem<NSNumber>(0);
            if (isoNumber is not null)
            {
                exifData.Iso = isoNumber.Int32Value;
            }
        }

        var exposureTimeKey = new NSString("ExposureTime");
        if (exifDict.ContainsKey(exposureTimeKey) && exifDict[exposureTimeKey] is NSNumber exposureTimeNumber)
        {
            exifData.ExposureTime = exposureTimeNumber.DoubleValue;
        }

        var flashKey = new NSString("Flash");
        if (exifDict.ContainsKey(flashKey) && exifDict[flashKey] is NSNumber flashNumber)
        {
            var flash = flashNumber.Int32Value;
            if (Enum.IsDefined(typeof(FlashMode), flash))
            {
                exifData.Flash = (FlashMode)flash;
            }
        }
    }

    private static void ExtractTiffProperties(NSDictionary tiffDict, ExifData exifData)
    {
        if (tiffDict.TryGetValue(CGImageProperties.TIFFMake, out var makeObj) && makeObj is NSString makeString)
        {
            exifData.Make = makeString.ToString();
        }

        if (tiffDict.TryGetValue(CGImageProperties.TIFFModel, out var modelObj) && modelObj is NSString modelString)
        {
            exifData.Model = modelString.ToString();
        }

        if (tiffDict.TryGetValue(CGImageProperties.TIFFSoftware, out var softwareObj) && softwareObj is NSString softwareString)
        {
            exifData.Software = softwareString.ToString();
        }

        if (tiffDict.TryGetValue(CGImageProperties.TIFFArtist, out var artistObj) && artistObj is NSString artistString)
        {
            exifData.Artist = artistString.ToString();
        }

        if (tiffDict.TryGetValue(CGImageProperties.TIFFImageDescription, out var descriptionObj) && descriptionObj is NSString descriptionString)
        {
            exifData.ImageDescription = descriptionString.ToString();
        }
    }

    private static void ExtractGpsProperties(NSDictionary gpsDict, ExifData exifData)
    {
        // Extract latitude
        if (gpsDict.TryGetValue(CGImageProperties.GPSLatitude, out var latObj) && latObj is NSNumber latNumber &&
            gpsDict.TryGetValue(CGImageProperties.GPSLatitudeRef, out var latRefObj) && latRefObj is NSString latRefString)
        {
            var lat = latNumber.DoubleValue;
            var latRef = latRefString.ToString();
            
            if (latRef is not null)
            {
                exifData.Latitude = latRef == "S" ? -lat : lat;
            }
        }

        // Extract longitude
        if (gpsDict.TryGetValue(CGImageProperties.GPSLongitude, out var lonObj) && lonObj is NSNumber lonNumber &&
            gpsDict.TryGetValue(CGImageProperties.GPSLongitudeRef, out var lonRefObj) && lonRefObj is NSString lonRefString)
        {
            var lon = lonNumber.DoubleValue;
            var lonRef = lonRefString.ToString();
            
            if (lonRef is not null)
            {
                exifData.Longitude = lonRef == "W" ? -lon : lon;
            }
        }

        // Extract altitude
        if (gpsDict.TryGetValue(CGImageProperties.GPSAltitude, out var altObj) && altObj is NSNumber altNumber)
        {
            var alt = altNumber.DoubleValue;
            // Check altitude reference (0 = above sea level, 1 = below sea level)
            var altRef = 0;
            if (gpsDict.TryGetValue(CGImageProperties.GPSAltitudeRef, out var altRefObj) && altRefObj is NSNumber altRefNumber)
            {
                altRef = altRefNumber.Int32Value;
            }
            
            exifData.Altitude = altRef == 1 ? -alt : alt;
        }
    }

    private static void AddPropertiesToAllTags(NSDictionary properties, Dictionary<string, object?> allTags)
    {
        foreach (var kvp in properties)
        {
            var key = kvp.Key.ToString();
            var value = kvp.Value;

            if (value is NSDictionary dict)
            {
                // For nested dictionaries, add each key-value pair with a prefix
                foreach (var nestedKvp in dict)
                {
                    var nestedKey = $"{key}.{nestedKvp.Key}";
                    allTags[nestedKey] = ConvertNSObjectToClrObject(nestedKvp.Value);
                }
            }
            else
            {
                allTags[key] = ConvertNSObjectToClrObject(value);
            }
        }
    }

    private static object? ConvertNSObjectToClrObject(NSObject? nsObject)
    {
        return nsObject switch
        {
            NSString str => str.ToString(),
            NSNumber num => num.DoubleValue,
            NSArray array => array.ToArray().Select(ConvertNSObjectToClrObject).ToArray(),
            NSDictionary dict => dict.ToDictionary(
                kvp => kvp.Key.ToString(), 
                kvp => ConvertNSObjectToClrObject(kvp.Value)),
            _ => nsObject?.ToString()
        };
    }

    public async Task<bool> WriteToFileAsync(string filePath, ExifData exifData)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || exifData is null)
        {
            return false;
        }

        return await Task.Run(() =>
        {
            try
            {
                using var sourceUrl = NSUrl.FromFilename(filePath);
                using var sourceImageSource = CGImageSource.FromUrl(sourceUrl);
                
                if (sourceImageSource is null)
                {
                    return false;
                }

                using var image = sourceImageSource.CreateImage(0, null);
                if (image is null)
                {
                    return false;
                }

                using var destinationUrl = NSUrl.FromFilename(filePath);
                using var destination = CGImageDestination.Create(destinationUrl, sourceImageSource.TypeIdentifier, 1);
                
                if (destination is null)
                {
                    return false;
                }

                var properties = CreateImageProperties(exifData);
                destination.AddImage(image, properties);
                
                return destination.Close();
            }
            catch (Exception)
            {
                return false;
            }
        });
    }

    public async Task<bool> WriteToStreamAsync(Stream inputStream, Stream outputStream, ExifData exifData)
    {
        if (inputStream is null || outputStream is null || exifData is null)
        {
            return false;
        }

        return await Task.Run(() =>
        {
            try
            {
                using var inputData = NSData.FromStream(inputStream);
                if (inputData is null)
                {
                    return false;
                }

                using var sourceImageSource = CGImageSource.FromData(inputData);
                if (sourceImageSource is null)
                {
                    return false;
                }

                using var image = sourceImageSource.CreateImage(0, null);
                if (image is null)
                {
                    return false;
                }

                using var outputData = new NSMutableData();
                using var destination = CGImageDestination.Create(outputData, sourceImageSource.TypeIdentifier, 1);
                
                if (destination is null)
                {
                    return false;
                }

                var properties = CreateImageProperties(exifData);
                destination.AddImage(image, properties);
                
                if (!destination.Close())
                {
                    return false;
                }

                // Copy the output data to the output stream
                outputData.AsStream().CopyTo(outputStream);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        });
    }

    private static NSDictionary CreateImageProperties(ExifData exifData)
    {
        var properties = new NSMutableDictionary();
        
        // Create EXIF dictionary
        var exifDict = new NSMutableDictionary();
        
        if (exifData.DateTaken.HasValue)
        {
            var dateString = exifData.DateTaken.Value.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
            exifDict.SetValueForKey(new NSString(dateString), new NSString("DateTimeOriginal"));
        }

        if (exifData.FocalLength.HasValue)
        {
            exifDict.SetValueForKey(NSNumber.FromDouble(exifData.FocalLength.Value), new NSString("FocalLength"));
        }

        if (exifData.FNumber.HasValue)
        {
            exifDict.SetValueForKey(NSNumber.FromDouble(exifData.FNumber.Value), new NSString("FNumber"));
        }

        if (exifData.Iso.HasValue)
        {
            var isoArray = NSArray.FromNSObjects(NSNumber.FromInt32(exifData.Iso.Value));
            exifDict.SetValueForKey(isoArray, new NSString("ISOSpeedRatings"));
        }

        if (exifData.ExposureTime.HasValue)
        {
            exifDict.SetValueForKey(NSNumber.FromDouble(exifData.ExposureTime.Value), new NSString("ExposureTime"));
        }

        if (exifData.Flash.HasValue)
        {
            exifDict.SetValueForKey(NSNumber.FromInt32((int)exifData.Flash.Value), new NSString("Flash"));
        }

        properties.SetValueForKey(exifDict, new NSString("{Exif}"));

        // Create TIFF dictionary
        var tiffDict = new NSMutableDictionary();

        if (!string.IsNullOrEmpty(exifData.Make))
        {
            tiffDict.SetValueForKey(new NSString(exifData.Make), CGImageProperties.TIFFMake);
        }

        if (!string.IsNullOrEmpty(exifData.Model))
        {
            tiffDict.SetValueForKey(new NSString(exifData.Model), CGImageProperties.TIFFModel);
        }

        if (!string.IsNullOrEmpty(exifData.Software))
        {
            tiffDict.SetValueForKey(new NSString(exifData.Software), CGImageProperties.TIFFSoftware);
        }

        if (!string.IsNullOrEmpty(exifData.Artist))
        {
            tiffDict.SetValueForKey(new NSString(exifData.Artist), CGImageProperties.TIFFArtist);
        }

        if (!string.IsNullOrEmpty(exifData.ImageDescription))
        {
            tiffDict.SetValueForKey(new NSString(exifData.ImageDescription), CGImageProperties.TIFFImageDescription);
        }

        if (!string.IsNullOrEmpty(exifData.Copyright))
        {
            tiffDict.SetValueForKey(new NSString(exifData.Copyright), CGImageProperties.TIFFCopyright);
        }

        properties.SetValueForKey(tiffDict, new NSString("{TIFF}"));

        // Create GPS dictionary
        if (exifData.Latitude.HasValue && exifData.Longitude.HasValue)
        {
            var gpsDict = new NSMutableDictionary();

            gpsDict.SetValueForKey(NSNumber.FromDouble(Math.Abs(exifData.Latitude.Value)), CGImageProperties.GPSLatitude);
            gpsDict.SetValueForKey(new NSString(exifData.Latitude.Value >= 0 ? "N" : "S"), CGImageProperties.GPSLatitudeRef);

            gpsDict.SetValueForKey(NSNumber.FromDouble(Math.Abs(exifData.Longitude.Value)), CGImageProperties.GPSLongitude);
            gpsDict.SetValueForKey(new NSString(exifData.Longitude.Value >= 0 ? "E" : "W"), CGImageProperties.GPSLongitudeRef);

            if (exifData.Altitude.HasValue)
            {
                gpsDict.SetValueForKey(NSNumber.FromDouble(Math.Abs(exifData.Altitude.Value)), CGImageProperties.GPSAltitude);
                gpsDict.SetValueForKey(NSNumber.FromInt32(exifData.Altitude.Value >= 0 ? 0 : 1), CGImageProperties.GPSAltitudeRef);
            }

            properties.SetValueForKey(gpsDict, new NSString("{GPS}"));
        }

        // Set image dimensions and orientation
        if (exifData.Width.HasValue)
        {
            properties.SetValueForKey(NSNumber.FromInt32(exifData.Width.Value), CGImageProperties.PixelWidth);
        }

        if (exifData.Height.HasValue)
        {
            properties.SetValueForKey(NSNumber.FromInt32(exifData.Height.Value), CGImageProperties.PixelHeight);
        }

        if (exifData.Orientation.HasValue)
        {
            properties.SetValueForKey(NSNumber.FromInt32((int)exifData.Orientation.Value), CGImageProperties.Orientation);
        }

        return properties;
    }
}