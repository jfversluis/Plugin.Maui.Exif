using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Plugin.Maui.Exif.Sample;

internal static class PermissionUtility
{
    public static async Task<PermissionStatus> RequestLocationPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<LocationWhenInUse>();
        }
        return status;
    }

    public static async Task<PermissionStatus> RequestMediaLocationPermissionAsync()
    {
        if (OperatingSystem.IsAndroid() && OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            var status = await Permissions.CheckStatusAsync<MediaLocation>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<MediaLocation>();
            }
            return status;
        }
        else
        {
            return PermissionStatus.Granted;
        }
    }
}

public class MediaLocation : BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new[]
        {
            (global::Android.Manifest.Permission.AccessMediaLocation, true)
        };
#endif
}
