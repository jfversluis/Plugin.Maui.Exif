using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace Plugin.Maui.Feature.Sample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    const int RequestPermissionsId = 1001;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        RequestLocationRelatedPermissions();
    }

    void RequestLocationRelatedPermissions()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(29)) // Android 10+
        {
            var permissionsToRequest = new List<string>();

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
                permissionsToRequest.Add(Manifest.Permission.AccessFineLocation);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
                permissionsToRequest.Add(Manifest.Permission.AccessCoarseLocation);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessMediaLocation) != Permission.Granted)
                permissionsToRequest.Add(Manifest.Permission.AccessMediaLocation);

            if (permissionsToRequest.Count > 0)
            {
                ActivityCompat.RequestPermissions(this, permissionsToRequest.ToArray(), RequestPermissionsId);
            }
        }
    }
}
