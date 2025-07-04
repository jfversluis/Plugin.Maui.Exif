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
    const int RequestMediaLocationId = 1001;
    const int RequestReadImagesId = 1002;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        RequestMediaLocationPermission();
    }

    void RequestMediaLocationPermission()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(29)) // Android 10+
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessMediaLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] 
                { Manifest.Permission.AccessMediaLocation}, RequestMediaLocationId);
            }
        }
    }
}