using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiApp1;

public class Message
{
    public bool RelayState { get; set; }
}

public partial class MainPage : ContentPage
{
    HttpClient _httpClient = new HttpClient();
    private CancellationTokenSource _cancellationTokenSource;
    IEnumerable<ConnectionProfile> profiles = Connectivity.Current.ConnectionProfiles;
    string baseUrl = "http://192.168.4.1";

    public MainPage()
	{
		InitializeComponent();
    }


    private async void ResetLpg(object sender, EventArgs e)
    {
        if ( await CheckAndRequestLocationPermission() == PermissionStatus.Granted)
        {
            if (profiles.Contains(ConnectionProfile.WiFi))
            {
                Message msg = new()
                {
                    RelayState = true
                };
                JsonSerializerOptions options = new()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                string msgJson = JsonSerializer.Serialize<Message>(msg, options);

                var httpContent = new StringContent(msgJson, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{baseUrl}/post", httpContent);
                if (response != null)
                {
                    StatusLbl.Text = await response.Content.ReadAsStringAsync() + "\nReset Request send to ESP - 32";
                }
                _cancellationTokenSource = new CancellationTokenSource();
                UpdateArc();
            }
            else
            {
                StatusLbl.Text = "No WiFi connection";
            }
        }
    }

    private async void UpdateArc()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (profiles.Contains(ConnectionProfile.WiFi))
            {
                var response = await _httpClient.GetAsync($"{baseUrl}/lpgstatus");
                if (response != null)
                {
                    StatusLbl.Text = await response.Content.ReadAsStringAsync();
                }
            }
            else
            {
                StatusLbl.Text = "No WiFi connection";
            }

            SemanticScreenReader.Announce(StatusLbl.Text);

            await Task.Delay(1000);
        }

        // Reset the view
    }

    public async Task<PermissionStatus> CheckAndRequestLocationPermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.NetworkState>();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            return status;
        }

        if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
        {
            // Prompt the user with additional information as to why the permission is needed
        }

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        return status;
    }
}

