using CommunityCore;
using CommunityCore.Events;
using CommunityCore.Storage;

namespace PolkadotRoots.Pages;

public partial class EventsPage : ContentPage
{
    private readonly CommunityEventsApiClient eventsApi;
    private readonly StorageApiClient storageApi;
    private readonly EventsViewModel vm;

    public EventsPage()
    {
        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);

        InitializeComponent();

        var http = new HttpClient();
        eventsApi = new CommunityEventsApiClient(http, new CommunityApiOptions());
        storageApi = new StorageApiClient(http, new CommunityApiOptions());
        vm = new EventsViewModel(eventsApi, storageApi);
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!vm.Initialized)
            await vm.LoadNextPageAsync();
    }

    private async void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        await vm.LoadNextPageAsync();
    }
}