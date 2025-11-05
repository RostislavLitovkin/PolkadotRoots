using CommunityCore;
using CommunityCore.Events;
using CommunityCore.Storage;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class RegisterEventPage : PageTemplate
{
    private readonly RegisterEventViewModel vm;
    private readonly long? eventId;

    public RegisterEventPage()
    {
        InitializeComponent();

        var http = new HttpClient();
        var storage = new StorageApiClient(http, new CommunityApiOptions());
        var eventsApi = new CommunityEventsApiClient(http, new CommunityApiOptions());
        vm = new RegisterEventViewModel(storage, eventsApi);
        BindingContext = vm;
        eventId = null;
    }

    public RegisterEventPage(long eventId)
    {
        InitializeComponent();

        var http = new HttpClient();
        var storage = new StorageApiClient(http, new CommunityApiOptions());
        var eventsApi = new CommunityEventsApiClient(http, new CommunityApiOptions());
        vm = new RegisterEventViewModel(storage, eventsApi);
        BindingContext = vm;
        this.eventId = eventId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (eventId.HasValue)
        {
            await vm.InitForEditAsync(eventId.Value);
        }
        else
        {
            vm.Init();
        }
    }
}
