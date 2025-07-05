using Plugin.Maui.Exif;
using Plugin.Maui.Exif.Extensions;
using System.Text;

namespace Plugin.Maui.Exif.Sample;

public partial class MainPage : ContentPage
{
    readonly IExif exif;

    public MainPage(IExif exif)
    {
        InitializeComponent();
        this.exif = exif;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await PermissionUtility.RequestLocationPermissionAsync();
        await PermissionUtility.RequestMediaLocationPermissionAsync();
    }

    private async void OnSelectImageClicked(object sender, EventArgs e)
    {
        try
        {
            // Show action sheet to choose between camera, gallery, and file picker
            var action = await DisplayActionSheet("Select Image Source", "Cancel", null, "Take Photo", "Choose from Gallery", "Browse Files");
            
            FileResult result = null;

            if (action == "Take Photo")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    result = await MediaPicker.Default.CapturePhotoAsync();
                }
                else
                {
                    await DisplayAlert("Not Supported", "Camera capture is not supported on this device.", "OK");
                    return;
                }
            }
            else if (action == "Choose from Gallery")
            {
                result = await MediaPicker.Default.PickPhotoAsync();
            }
            else if (action == "Browse Files")
            {
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.image" } }, // UTType for images on iOS
                        { DevicePlatform.Android, new[] { "image/*" } }, // MIME type for images on Android
                        { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" } }, // File extensions for Windows
                        { DevicePlatform.Tizen, new[] { "image/*" } },
                        { DevicePlatform.macOS, new[] { "public.image" } }, // UTType for images on macOS
                    });

                var options = new PickOptions
                {
                    PickerTitle = "Select an image file",
                    FileTypes = customFileType,
                };

                result = await FilePicker.Default.PickAsync(options);
            }

            if (result is not null)
            {
                // Display the selected image
                SelectedImage.Source = ImageSource.FromFile(result.FullPath);
                SelectedImage.IsVisible = true;

                // Read EXIF data
                var exifData = await exif.ReadFromFileAsync(result.FullPath);

                if (exifData is not null)
                {
                    DisplayExifData(exifData);
                    ExifDataFrame.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("No EXIF Data", "This image doesn't contain EXIF metadata.", "OK");
                    ExifDataFrame.IsVisible = false;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to read image: {ex.Message}", "OK");
        }
    }

    private void DisplayExifData(Plugin.Maui.Exif.Models.ExifData exifData)
    {
        // Camera info
        CameraInfoLabel.Text = $"📷 {exifData.GetCameraInfo()}";

        // Camera settings
        CameraSettingsLabel.Text = $"⚙️ {exifData.GetFormattedCameraSettings()}";

        // Date taken
        DateTakenLabel.Text = exifData.DateTaken.HasValue 
            ? $"📅 {exifData.DateTaken.Value:yyyy-MM-dd HH:mm:ss}"
            : "📅 Date not available";

        // Dimensions
        DimensionsLabel.Text = (exifData.Width.HasValue && exifData.Height.HasValue)
            ? $"📐 {exifData.Width}×{exifData.Height} pixels"
            : "📐 Dimensions not available";

        // GPS
        if (exifData.HasGpsCoordinates())
        {
            var coordinates = exifData.GetFormattedGpsCoordinates();
            var altitude = exifData.Altitude.HasValue ? $", {exifData.Altitude.Value:F1}m" : "";
            GpsLabel.Text = $"🌍 {coordinates}{altitude}";
        }
        else
        {
            GpsLabel.Text = "🌍 No GPS data available";
        }

        // Orientation
        OrientationLabel.Text = exifData.Orientation.HasValue
            ? $"🔄 {exifData.Orientation}"
            : "🔄 Orientation not specified";

        // All tags
        var allTagsText = new StringBuilder();
        foreach (var tag in exifData.AllTags.OrderBy(t => t.Key))
        {
            allTagsText.AppendLine($"{tag.Key}: {tag.Value}");
        }
        AllTagsLabel.Text = allTagsText.Length > 0 ? allTagsText.ToString() : "No additional tags found";
    }
}
