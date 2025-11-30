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
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string address = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? description;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? lumaUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? website;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? mapsUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? country;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? locationAddress;

    [ObservableProperty]
    private string? phoneNumber;

    [ObservableProperty]
    private string? emailAddress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? capacityText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? price = "FREE with App";

    // new date inputs from date picker
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private DateTime startDate = default;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private DateTime endDate = default;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private Stream? imageStream = null;

    // Edit mode specific
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private long? id = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? existingImagePath = null;

    // New: multi organizer addresses bound to FormMultiAddressInputView
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private List<string> organisatorAddresses = new();

    private bool IsEdit => Id.HasValue;

    public ButtonStateEnum SubmitButtonState
    {
        get
        {
            bool baseOk =
                !string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(Address) &&
                !string.IsNullOrWhiteSpace(MapsUrl) &&
                !string.IsNullOrWhiteSpace(Country) &&
                !string.IsNullOrWhiteSpace(LocationAddress) &&
                !string.IsNullOrWhiteSpace(CapacityText) &&
                !string.IsNullOrWhiteSpace(Price) &&
                StartDate != default &&
                EndDate != default;

            if (!baseOk) return ButtonStateEnum.Disabled;

            // When creating, require new image; when editing, allow keeping existing image
            bool hasImage = ImageStream != null || (IsEdit && !string.IsNullOrWhiteSpace(ExistingImagePath));
            return hasImage ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled;
        }
    }

    public RegisterEventViewModel(StorageApiClient storage, CommunityEventsApiClient eventsApi)
    {
        this.storage = storage;
        this.eventsApi = eventsApi;
    }

    public void Init()
    {
        // Default address to currently selected account when creating new
        if (!IsEdit)
        {
            var list = new List<string>();
            if (KeysModel.HasSubstrateKey())
            {
                var key = KeysModel.GetSubstrateKey();
                Address = key;
                if (!string.IsNullOrWhiteSpace(key))
                {
                    list.Add(key);
                }
            }
            else
            {
                Address = string.Empty;
            }

            // assign after we prepared the list so the UI picks it up
            OrganisatorAddresses = list;
        }
    }

    public async Task InitForEditAsync(long eventId)
    {
        try
        {
            var ev = await eventsApi.GetAsync(eventId);
            if (ev is null) return;

            Title = "Edit Event";
            Id = ev.Id;

            Name = ev.Name ?? string.Empty;
            Description = ev.Description;
            LumaUrl = ev.LumaUrl;
            Website = ev.Website;
            MapsUrl = ev.MapsUrl;
            Country = ev.Country;
            LocationAddress = ev.Address;
            PhoneNumber = ev.PhoneNumber;
            EmailAddress = ev.EmailAddress;
            CapacityText = ev.Capacity?.ToString();
            Price = ev.Price ?? Price; // keep existing default if null

            if (ev.OrganizatorAddresses != null && ev.OrganizatorAddresses.Count > 0)
            {
                Address = ev.OrganizatorAddresses[0];
                OrganisatorAddresses = new List<string>(ev.OrganizatorAddresses);
            }
            else
            {
                Address = KeysModel.HasSubstrateKey() ? KeysModel.GetSubstrateKey() : string.Empty;
                OrganisatorAddresses = string.IsNullOrWhiteSpace(Address) ? new() : new() { Address };
            }

            ExistingImagePath = ev.Image;

            // Convert unix seconds or milliseconds to DateTime
            StartDate = FromUnixToLocalDateTime(ev.TimeStart) ?? default;
            EndDate = FromUnixToLocalDateTime(ev.TimeEnd) ?? default;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private static DateTime? FromUnixToLocalDateTime(long? ts)
    {
        if (!ts.HasValue) return null;
        try
        {
            long v = ts.Value;
            if (v >= 1_000_000_000_000) v /= 1000; // normalize to seconds
            return DateTimeOffset.FromUnixTimeSeconds(v).LocalDateTime;
        }
        catch
        {
            return null;
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
                return;
            }

            string? imagePath = ExistingImagePath;

            if (ImageStream != null)
            {
                var folder = $"events";

                // Compress and downscale to avoid 413 (Payload Too Large) without changing orientation
                using var compressed = await Task.Run(() => ImageModel.CompressImageToJpeg(ImageStream!, maxWidth: 1600, maxHeight: 1600, targetBytes: 1024 * 1024));
                compressed.Position = 0;

                var fileName = $"{Guid.NewGuid().ToString()}.jpg";
                var contentType = "image/jpeg";

                var upload = await storage.UploadImageAsync(compressed, fileName, contentType, folder);
                imagePath = $"{folder}/{fileName}";
            }

            uint? capacity = null;
            if (!string.IsNullOrWhiteSpace(CapacityText) && uint.TryParse(CapacityText, out var cap))
                capacity = cap;

            long? timeStart = null;
            long? timeEnd = null;

            // Prefer date picker values when provided
            if (StartDate != default)
                timeStart = new DateTimeOffset(StartDate).ToUnixTimeSeconds();
            if (EndDate != default)
                timeEnd = new DateTimeOffset(EndDate).ToUnixTimeSeconds();

            var dto = new EventDto
            {
                Id = Id,
                OrganizatorAddresses = OrganisatorAddresses.Where(address => !string.IsNullOrWhiteSpace(address)).ToList(),
                Name = Name,
                Description = Description,
                Image = imagePath,
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

            if (IsEdit)
            {
                var updated = await eventsApi.PutAsync(account, Id, dto);
                await Shell.Current.DisplayAlertAsync("Success", $"Event '{updated.Name}' updated.", "OK");
            }
            else
            {
                var created = await eventsApi.CreateAsync(account, dto);
                await Shell.Current.DisplayAlertAsync("Success", $"Event '{created.Name}' created.", "OK");
            }

            await Shell.Current.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}
