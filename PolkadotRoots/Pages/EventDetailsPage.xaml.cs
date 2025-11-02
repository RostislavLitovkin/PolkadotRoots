using CommunityCore;
using CommunityCore.Events;
using CommunityCore.Interest;
using CommunityCore.Storage;
using Hydration.NetApi.Generated;
using PlutoFramework.Model;
using PlutoFramework.Model.HydraDX;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class EventDetailsPage : PageTemplate
{
    private readonly EventDetailsViewModel vm;

    public EventDetailsPage(long id)
    {
        InitializeComponent();

        var http = new HttpClient();
        var eventsApi = new CommunityEventsApiClient(http, new CommunityApiOptions());
        var storage = new StorageApiClient(http, new CommunityApiOptions());
        var interestApi = new CommunityInterestApiClient(http, new CommunityApiOptions());
        vm = new EventDetailsViewModel(eventsApi, storage, interestApi, id);
        BindingContext = vm;



        Console.WriteLine("My Address: " + KeysModel.GetSubstrateKey());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await vm.LoadAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
