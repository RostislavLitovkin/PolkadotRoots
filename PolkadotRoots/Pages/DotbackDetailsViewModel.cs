using CommunityCore;
using CommunityCore.Dotback;
using CommunityCore.Events;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.Loading;
using PlutoFramework.Components.TransactionAnalyzer;
using PlutoFramework.Constants;
using PlutoFramework.Model;
using PlutoFramework.Model.HydraDX;
using PolkadotRoots.Helpers;
using PlutoFramework.Components.WebView;

namespace PolkadotRoots.Pages;

public partial class DotbackDetailsViewModel : ObservableObject
{
    private readonly DotbackDto dto;

    [ObservableProperty]
    private string title = "Dotback";

    [ObservableProperty]
    private long eventId;

    [ObservableProperty]
    private string eventImageUrl = string.Empty;

    [ObservableProperty]
    private string eventTitle = string.Empty;

    [ObservableProperty]
    private string eventStartDate = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    [NotifyPropertyChangedFor(nameof(LocalValueText))]
    private double usdAmount;

    [ObservableProperty]
    private string? imageUrl;

    // Conversion state
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConversion))]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    [NotifyPropertyChangedFor(nameof(LocalValueText))]
    private string? country;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConversion))]
    [NotifyPropertyChangedFor(nameof(LocalValueText))]
    private string? currency;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConversion))]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    [NotifyPropertyChangedFor(nameof(LocalValueText))]
    private string? currencySymbol;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConversion))]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    [NotifyPropertyChangedFor(nameof(LocalValueText))]
    private decimal? usdToLocalRate;

    public bool HasConversion => UsdToLocalRate.HasValue && !string.IsNullOrWhiteSpace(Currency) && !string.IsNullOrWhiteSpace(Country);

    public string ValueText => HasConversion ? $"{UsdAmount:F2} USD ≈ {(decimal)UsdAmount / UsdToLocalRate!.Value:F2} {Currency}" : $"{UsdAmount:F2} USD";

    public string LocalValueText => HasConversion ? $"{(decimal)UsdAmount / UsdToLocalRate!.Value:F2} {(string.IsNullOrWhiteSpace(CurrencySymbol) ? Currency : CurrencySymbol)}" : string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    [NotifyPropertyChangedFor(nameof(ButtonIsVisible))]
    private bool paid;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    [NotifyPropertyChangedFor(nameof(ButtonIsVisible))]
    private bool rejected;

    public string StatusText => Rejected ? "Rejected" : Paid ? "Paid" : "Pending";

    public Color StatusColor => Rejected ? Colors.DarkRed : Paid ? Colors.Green : Colors.Orange;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ButtonIsVisible))]
    private bool isOrganizer;

    public bool ButtonIsVisible => IsOrganizer && !(Rejected || Paid);

    // Subscan link
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSubscan))]
    private string? subscanUrl;

    public bool HasSubscan => !string.IsNullOrWhiteSpace(SubscanUrl);

    public DotbackDetailsViewModel(DotbackDto dto)
    {
        this.dto = dto;
    }

    public async Task LoadAsync()
    {
        EventId = dto.EventId;
        Address = dto.Address;
        UsdAmount = dto.UsdAmount;
        Paid = dto.Paid;
        Rejected = dto.Rejected;
        SubscanUrl = dto.SubscanUrl;

        try
        {
            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                ImageUrl = await CommunityClientHelper.StorageApi.GetImageAsync(dto.ImageUrl);
            }
        }
        catch { }

        // Load event country and conversion rate + organizer check
        try
        {
            var http = new HttpClient();
            var eventsApi = new CommunityEventsApiClient(http, new CommunityApiOptions());
            var ev = await eventsApi.GetAsync(EventId);
            if (ev != null)
            {
                Country = ev.Country;
                var (currencySymbol, isoCurrencySymbol, exchangeRateLocalToUsd) = await CurrencyHelper.GetCurrencySymbolAndRateToUsdAsync(Country);
                CurrencySymbol = currencySymbol;
                Currency = isoCurrencySymbol; // ISO code
                UsdToLocalRate = exchangeRateLocalToUsd;

                try
                {
                    var myAddress = KeysModel.GetSubstrateKey();
                    IsOrganizer = !string.IsNullOrWhiteSpace(myAddress) && (ev.OrganizatorAddresses?.Contains(myAddress) == true);
                }
                catch
                {
                    IsOrganizer = false;
                }
            }
        }
        catch { }

        try
        {
            var ev = await CommunityClientHelper.EventsApi.GetAsync(EventId);

            if (ev != null && !string.IsNullOrWhiteSpace(ev.Image))
            {
                EventTitle = ev.Name ?? "";
                EventStartDate = TimeDateHelper.FormatTimes(ev.TimeStart, ev.TimeEnd).start;
                EventImageUrl = await CommunityClientHelper.StorageApi.GetImageAsync(ev.Image);
            }
        }
        catch
        {

        }
    }

    [RelayCommand]
    public async Task PayAsync()
    {
        var loading = DependencyService.Get<FullPageLoadingViewModel>();

        var token = CancellationToken.None;
        try
        {
            var client = await SubstrateClientModel.GetOrAddSubstrateClientAsync(EndpointEnum.PolkadotAssetHub, CancellationToken.None);

            int decimals = Endpoints.GetEndpointDictionary[EndpointEnum.PolkadotAssetHub].Decimals;

            var dotSpotPrice = Sdk.GetSpotPrice("DOT");

            if (dotSpotPrice is null)
            {
                throw new Exception("Could not retrieve DOT spot price.");
            }

            decimal dotAmount = (decimal)UsdAmount / (decimal)dotSpotPrice;

            System.Numerics.BigInteger dotAmountPlanck = (System.Numerics.BigInteger)((decimal)System.Numerics.BigInteger.Pow(10, decimals) * dotAmount);

            var method = TransferModel.NativeTransfer(client, Address, dotAmountPlanck);

            var account = await KeysModel.GetAccountAsync();
            if (account is null) return;

            var transactionAnalyzerConfirmationViewModel = DependencyService.Get<TransactionAnalyzerConfirmationViewModel>();
            var txHash = await transactionAnalyzerConfirmationViewModel.LoadAsync(
                client,
                method,
                showDAppView: false,
                enableLoading: true,
                token: token
            );

            if (txHash is null)
            {
                return;
            }

            var subscanUrl = $"https://assethub-polkadot.subscan.io/extrinsic/{txHash}";

            var result = await CommunityClientHelper.DotbacksApi.UpdateStatusAsync(account, dto.EventId, dto.Address, paid: true, rejected: null, subscanUrl: subscanUrl);

            await Shell.Current.DisplayAlertAsync("Success", "Dotback was paid successfully.", "OK");
            await Shell.Current.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await Shell.Current.DisplayAlertAsync("Payment error", ex.Message, "OK");
        }

        loading.IsVisible = false;
    }

    [RelayCommand]
    public async Task RejectAsync()
    {
        try
        {
            var admin = await KeysModel.GetAccountAsync("");
            if (admin is null) return;

            var http = new HttpClient();
            var api = new CommunityDotbacksApiClient(http, new CommunityApiOptions());

            var updated = await api.UpdateStatusAsync(admin, dto.EventId, dto.Address, paid: null, rejected: true, subscanUrl: null);

            if (updated?.Rejected == true)
            {
                Title = "Dotback (Rejected)";
                await Shell.Current.DisplayAlertAsync("Rejected", "The dotback was marked as rejected.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Rejected", "Dotback status updated.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task OpenSubscanAsync()
    {
        if (string.IsNullOrWhiteSpace(SubscanUrl))
            return;

        try
        {
            await Shell.Current.Navigation.PushAsync(new ExtensionWebViewPage(SubscanUrl));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private Task OpenEventDetailsPageAsync() => Shell.Current.Navigation.PushAsync(new EventDetailsPage(EventId));
}
