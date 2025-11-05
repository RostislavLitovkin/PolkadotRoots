using CommunityCore;
using CommunityCore.Dotback;
using CommunityCore.Storage;
using Hydration.NetApi.Generated;
using PlutoFramework.Model;
using PlutoFramework.Model.HydraDX;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class DotbackDetailsPage : PageTemplate
{
    private readonly DotbackDetailsViewModel vm;

    public DotbackDetailsPage(DotbackDto dto)
    {
        InitializeComponent();

        var storage = new StorageApiClient(new HttpClient(), new CommunityApiOptions());
        var dotbacksApi = new CommunityDotbacksApiClient(new HttpClient(), new CommunityApiOptions());
        vm = new DotbackDetailsViewModel(dto, storage, dotbacksApi);
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await AppearingAsync();
    }

    private async Task AppearingAsync()
    {
        if (Sdk.Assets.Count == 0)
        {
            var client = await SubstrateClientModel.GetOrAddSubstrateClientAsync(PlutoFramework.Constants.EndpointEnum.Hydration, CancellationToken.None);
            await Sdk.GetAssetsAsync((SubstrateClientExt)client.SubstrateClient, null, CancellationToken.None);
        }

        await vm.LoadAsync();

    }
}
