namespace Plugin.Maui.Exif;

/// <summary>
/// Static class for accessing EXIF functionality.
/// </summary>
public static class Exif
{
	static IExif? defaultImplementation;

	/// <summary>
	/// Provides the default implementation for static usage of this API.
	/// </summary>
	public static IExif Default =>
		defaultImplementation ??= new ExifImplementation();

	internal static void SetDefault(IExif? implementation) =>
		defaultImplementation = implementation;
}
