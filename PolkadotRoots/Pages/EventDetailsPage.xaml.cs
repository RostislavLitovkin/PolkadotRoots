using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class EventDetailsPage : PageTemplate
{
    public EventDetailsPage(long id)
    {
        InitializeComponent();
        BindingContext = new EventDetailsViewModel(id);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = ((EventDetailsViewModel)BindingContext).LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ((EventDetailsViewModel)BindingContext).Stop();
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        _ = Navigation.PopAsync();
    }
}
