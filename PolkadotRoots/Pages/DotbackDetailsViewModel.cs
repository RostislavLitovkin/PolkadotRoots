using CommunityCore;
using CommunityCore.Dotback;
using CommunityCore.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.TransactionAnalyzer;
using PlutoFramework.Constants;
using PlutoFramework.Model;
using PlutoFramework.Model.HydraDX;
using System.Numerics;

namespace PolkadotRoots.Pages;

public partial class DotbackDetailsViewModel : ObservableObject
{
    private readonly DotbackDto dto;
    private readonly StorageApiClient storage;
    private readonly CommunityDotbacksApiClient dotbacksApi;

    [ObservableProperty]
    private string title = "Dotback";

    [ObservableProperty]
    private long eventId;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private double usdAmount;

    [ObservableProperty]
    private string? imageUrl;

    public DotbackDetailsViewModel(DotbackDto dto, StorageApiClient storage, CommunityDotbacksApiClient dotbacksApi)
    {
        this.dto = dto;
        this.storage = storage;
        this.dotbacksApi = dotbacksApi;
    }

    public async Task LoadAsync()
    {
        EventId = dto.EventId;
        Address = dto.Address;
        UsdAmount = dto.UsdAmount;

        try
        {
            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                ImageUrl = await storage.GetImageAsync(dto.ImageUrl);
            }
        }
        catch { }
    }

    [RelayCommand]
    public async Task PayAsync()
    {
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

            BigInteger dotAmountPlanck = (BigInteger)((decimal)BigInteger.Pow(10, decimals) * dotAmount);

            var method = TransferModel.NativeTransfer(client, Address, dotAmountPlanck);

            var account = await KeysModel.GetAccountAsync();
            if (account is null) return;

            var transactionAnalyzerConfirmationViewModel = DependencyService.Get<TransactionAnalyzerConfirmationViewModel>();
            var txHash = await transactionAnalyzerConfirmationViewModel.LoadAsync(
                client,
                method,
                showDAppView: false,
                token: token
            );

            if (txHash is null)
            {
                return;
            }

            var subscanUrl = $"https://assethub-polkadot.subscan.io/extrinsic/{txHash}";

            var result = await dotbacksApi.UpdateStatusAsync(account, dto.EventId, dto.Address, paid: true, rejected: null, subscanUrl: subscanUrl);

            await Shell.Current.DisplayAlert("Success", "Dotback was paid successfully.", "OK");
            await Shell.Current.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Payment error", ex.Message, "OK");
        }
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
                await Shell.Current.DisplayAlert("Rejected", "The dotback was marked as rejected.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Rejected", "Dotback status updated.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
