using CommunityCore;
using CommunityCore.Events;
using CommunityCore.Storage;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class RegisterEventPage : PageTemplate
{
    private readonly RegisterEventViewModel vm;

    public RegisterEventPage()
    {
        InitializeComponent();

        var http = new HttpClient();
        var storage = new StorageApiClient(http, new CommunityApiOptions());
        var eventsApi = new CommunityEventsApiClient(http, new CommunityApiOptions());
        vm = new RegisterEventViewModel(storage, eventsApi);
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.Init();
    }
}
