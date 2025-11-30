using CommunityCore.Dotback;
using CommunityCore.Events;
using CommunityCore.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.Buttons;
using PlutoFramework.Model;

namespace PolkadotRoots.Pages;

public partial class DotbackRegistrationViewModel : ObservableObject
{
    private readonly StorageApiClient storage;
    private readonly CommunityDotbacksApiClient dotbacksApi;
    private readonly CommunityEventsApiClient? eventsApi;
    private readonly long eventId;

    [ObservableProperty]
    private string title = "Submit DOT-back";

    [ObservableProperty]
    private string? eventName;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    [NotifyPropertyChangedFor(nameof(HasConvertedEstimate))]
    private double amount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    private string? selectedImageUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private string? fileName = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonState))]
    private Stream? imageStream = null;

    // Conversion info
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConversionText))]
    [NotifyPropertyChangedFor(nameof(HasConversion))]
    [NotifyPropertyChangedFor(nameof(HasConvertedEstimate))]
    private string country;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConversionText))]
    [NotifyPropertyChangedFor(nameof(HasConversion))]
    [NotifyPropertyChangedFor(nameof(HasConvertedEstimate))]
    private string? currency;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConversionText))]
    [NotifyPropertyChangedFor(nameof(HasConversion))]
    [NotifyPropertyChangedFor(nameof(RequestedAmountTitleText))]
    private decimal? usdToLocalRate;

    public ButtonStateEnum SubmitButtonState => Amount > 0 && ImageStream is not null ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled;

    public bool HasImage => !string.IsNullOrWhiteSpace(SelectedImageUrl);

    public long EventId => eventId;

    public bool HasConversion => UsdToLocalRate.HasValue && !string.IsNullOrWhiteSpace(Currency);

    public bool HasConvertedEstimate => Amount > 0 && HasConversion;

    public string ConversionText => HasConversion
        ? $"1 USD ≈ {1 / UsdToLocalRate!.Value:F2} {Currency}"
        : string.Empty;

    public string RequestedAmountTitleText => $"Enter requested Amount: (in {Currency})";
    public DotbackRegistrationViewModel(StorageApiClient storage, CommunityDotbacksApiClient dotbacksApi, long eventId, string? eventName, string countryCode)
    {
        this.storage = storage;
        this.dotbacksApi = dotbacksApi;
        this.eventId = eventId;
        this.eventName = eventName;
        this.Country = countryCode;
    }

    public async Task InitAsync()
    {
        Address = KeysModel.GetSubstrateKey("") ?? string.Empty;

        (var currencySymbol, var isoCurrencySymbol, var exchangeRate) = await Helpers.CurrencyHelper.GetCurrencySymbolAndRateToUsdAsync(Country!);
        Currency = currencySymbol;
        UsdToLocalRate = exchangeRate;
    }

    public void Init()
    {
        // Back-compat if called
        _ = InitAsync();
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        try
        {
            var account = await KeysModel.GetAccountAsync();

            if (account is null) return;

            var folder = $"dotbacks/{EventId}";

            // Compress and downscale to avoid 413 (Payload Too Large) without changing orientation
            using var compressed = await Task.Run(() => ImageModel.CompressImageToJpeg(ImageStream!, maxWidth: 1600, maxHeight: 1600, targetBytes: 1024 * 1024));
            compressed.Position = 0;

            FileName = $"{account.Value}.jpg";
            var contentType = "image/jpeg";

            var upload = await storage.UploadImageAsync(compressed, FileName, contentType, folder);

            var reg = new DotbackRegistrationDto
            {
                EventId = eventId,
                Address = account.Value,
                UsdAmount = Amount * (double)(UsdToLocalRate ?? 0),
                ImageUrl = $"dotbacks/{EventId}/{FileName}",
            };

            var result = await dotbacksApi.UpsertAsync(account, reg);

            await Shell.Current.DisplayAlertAsync("Success", "Your dotback was submitted.", "OK");
            await Shell.Current.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}
