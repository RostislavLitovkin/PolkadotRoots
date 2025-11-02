using CommunityCore;
using CommunityCore.Dotback;
using CommunityCore.Storage;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class MyDotbacksPage : PageTemplate
{
    private readonly MyDotbacksViewModel vm;

    public MyDotbacksPage()
    {
        InitializeComponent();

        var dotbacksApi = new CommunityDotbacksApiClient(new HttpClient(), new CommunityApiOptions());
        var storage = new StorageApiClient(new HttpClient(), new CommunityApiOptions());

        vm = new MyDotbacksViewModel(dotbacksApi, storage);
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
