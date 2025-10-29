using CommunityCore;
using CommunityCore.Dotback;
using CommunityCore.Storage;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class DotbacksPage : PageTemplate
{
    private readonly DotbacksViewModel vm;

    public DotbacksPage(long eventId, string? title)
    {
        InitializeComponent();

        // Instantiate required API clients
        var dotbacksApi = new CommunityDotbacksApiClient(new HttpClient(), new CommunityApiOptions());
        var storage = new StorageApiClient(new HttpClient(), new CommunityApiOptions());

        vm = new DotbacksViewModel(dotbacksApi, storage, eventId, title);
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.LoadNextPageAsync();
    }

    private async void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        await vm.LoadNextPageAsync();
    }
}
