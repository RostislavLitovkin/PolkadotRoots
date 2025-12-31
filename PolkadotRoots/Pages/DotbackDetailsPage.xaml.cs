using CommunityCore.Dotback;
using Hydration.NetApi.Generated;
using PlutoFramework.Model;
using PlutoFramework.Model.HydraDX;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class DotbackDetailsPage : PageTemplate
{
    public DotbackDetailsPage(DotbackDto dto)
    {
        InitializeComponent();
        BindingContext = new DotbackDetailsViewModel(dto);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = AppearingAsync();
    }

    private async Task AppearingAsync()
    {
        if (Sdk.Assets.Count == 0)
        {
            var client = await SubstrateClientModel.GetOrAddSubstrateClientAsync(PlutoFramework.Constants.EndpointEnum.Hydration, CancellationToken.None);
            await Sdk.GetAssetsAsync((SubstrateClientExt)client.SubstrateClient, null, CancellationToken.None);
        }

        await ((DotbackDetailsViewModel)BindingContext).LoadAsync();
    }
}
