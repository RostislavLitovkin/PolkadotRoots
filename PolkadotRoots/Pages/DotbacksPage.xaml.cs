using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class DotbacksPage : PageTemplate
{
    public DotbacksPage(long eventId, string? title)
    {
        InitializeComponent();

        BindingContext = new DotbacksViewModel(eventId, title);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = ((DotbacksViewModel)BindingContext).InitializeAsync();
    }

    private void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        _ = ((DotbacksViewModel)BindingContext).LoadNextPageAsync();
    }
}
