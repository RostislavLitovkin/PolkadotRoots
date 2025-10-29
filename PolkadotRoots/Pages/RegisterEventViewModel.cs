using CommunityCore.Events;
using CommunityCore.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.Buttons;
using PlutoFramework.Model;

namespace PolkadotRoots.Pages;

public partial class RegisterEventViewModel : ObservableObject
{
    private readonly StorageApiClient storage;
    private readonly CommunityEventsApiClient eventsApi;

    [ObservableProperty]
    private string title = "Register Event";

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string name = string.Empty;

    [ObservableProperty]
    private string? description;

    [ObservableProperty]
    private string? lumaUrl;

    [ObservableProperty]
    private string? website;

    [ObservableProperty]
    private string? mapsUrl;

    [ObservableProperty]
    private string? country;

    [ObservableProperty]
    private string? locationAddress;

    [ObservableProperty]
    private string? phoneNumber;

    [ObservableProperty]
    private string? emailAddress;

    [ObservableProperty]
    private string? capacityText;

    [ObservableProperty]
    private string? price;

    [ObservableProperty]
    private string? startText; // e.g. 2025-01-31 18:00

    [ObservableProperty]
    private string? endText;   // e.g. 2025-01-31 23:00

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    private string? selectedImageUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? fileName;

    public ButtonStateEnum SubmitButtonState =>
        !string.IsNullOrWhiteSpace(Name) && FileName is not null ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled;

    public bool HasImage => !string.IsNullOrWhiteSpace(SelectedImageUrl);

    public RegisterEventViewModel(StorageApiClient storage, CommunityEventsApiClient eventsApi)
    {
        this.storage = storage;
        this.eventsApi = eventsApi;
    }

    public void Init()
    {
        Address = KeysModel.GetSubstrateKey("") ?? string.Empty;
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select event image",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null) return;

            await using var stream = await result.OpenReadAsync();

            FileName = Guid.NewGuid().ToString();
            var contentType = result.ContentType ?? "image/*";

            var folder = "events";

            var upload = await storage.UploadImageAsync(stream, FileName, contentType, folder);

            SelectedImageUrl = upload.Url;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        try
        {
            var account = await KeysModel.GetAccountAsync();
            if (account is null)
            {
                await Shell.Current.DisplayAlert("Error", "No account available.", "OK");
                return;
            }

            uint? capacity = null;
            if (!string.IsNullOrWhiteSpace(CapacityText) && uint.TryParse(CapacityText, out var cap))
                capacity = cap;

            long? timeStart = null;
            if (!string.IsNullOrWhiteSpace(StartText) && DateTimeOffset.TryParse(StartText, out var startDto))
                timeStart = startDto.ToUnixTimeSeconds();

            long? timeEnd = null;
            if (!string.IsNullOrWhiteSpace(EndText) && DateTimeOffset.TryParse(EndText, out var endDto))
                timeEnd = endDto.ToUnixTimeSeconds();

            var dto = new EventDto
            {
                OrganizatorAddresses = new List<string> { account.Value },
                Name = Name,
                Description = Description,
                Image = SelectedImageUrl,
                LumaUrl = LumaUrl,
                Website = Website,
                MapsUrl = MapsUrl,
                Country = Country,
                Address = LocationAddress,
                PhoneNumber = PhoneNumber,
                EmailAddress = EmailAddress,
                Capacity = capacity,
                Price = Price,
                TimeStart = timeStart,
                TimeEnd = timeEnd,
            };

            var created = await eventsApi.CreateAsync(account, dto);

            await Shell.Current.DisplayAlert("Success", $"Event '{created.Name}' created.", "OK");
            await Shell.Current.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
