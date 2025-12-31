using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class EventDetailsPage : PageTemplate
{
    private readonly EventDetailsViewModel vm;

    public EventDetailsPage(long id)
    {
        InitializeComponent();
        vm = new EventDetailsViewModel(id);
        BindingContext = vm;
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
