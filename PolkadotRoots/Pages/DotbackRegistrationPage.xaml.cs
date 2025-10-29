using CommunityCore;
using CommunityCore.Dotback;
using CommunityCore.Storage;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class DotbackRegistrationPage : PageTemplate
{
    private readonly DotbackRegistrationViewModel vm;

    public DotbackRegistrationPage(long eventId, string? eventName)
    {
        InitializeComponent();

        var storage = new StorageApiClient(new HttpClient(), new CommunityApiOptions());
        var api = new CommunityDotbacksApiClient(new HttpClient(), new CommunityApiOptions());

        vm = new DotbackRegistrationViewModel(storage, api, eventId, eventName);
        BindingContext = vm;
        vm.Init();
    }
}
