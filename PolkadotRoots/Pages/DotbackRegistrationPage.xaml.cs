using CommunityCore;
using CommunityCore.Dotback;
using CommunityCore.Events;
using CommunityCore.Storage;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class DotbackRegistrationPage : PageTemplate
{
    private readonly DotbackRegistrationViewModel vm;

    public DotbackRegistrationPage(long eventId, string? eventName, string countryCode)
    {
        InitializeComponent();

        var http = new HttpClient();
        var storage = new StorageApiClient(http, new CommunityApiOptions());
        var api = new CommunityDotbacksApiClient(http, new CommunityApiOptions());
        var eventsApi = new CommunityEventsApiClient(http, new CommunityApiOptions());

        vm = new DotbackRegistrationViewModel(storage, api, eventId, eventName, countryCode);
        BindingContext = vm;
        _ = vm.InitAsync();
    }
}
