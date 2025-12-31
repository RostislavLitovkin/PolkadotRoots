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

        vm = new DotbackRegistrationViewModel(eventId, countryCode);
        BindingContext = vm;
        _ = vm.InitAsync();
    }
}
