using PlutoFramework;
using PlutoFramework.Components.NetworkSelect;
using PlutoFramework.Model;
using PlutoFramework.Templates.PageTemplate;

namespace PolkadotRoots.Pages;

public partial class MainPage : PageTemplate, IPlutoFrameworkMainPage
{
    public IList<IView> Views => StackLayout?.Children ?? [];
    public static VerticalStackLayout? StackLayout { get; set; }
    public static MultiNetworkSelectView? NetworksView { get; set; }

	public MainPage()
	{
        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);

        InitializeComponent();

		BindingContext = new MainPageViewModel();

        networksView.IsVisible = Preferences.Get(PreferencesModel.SETTINGS_DISPLAY_NETWORKS, (bool)Application.Current.Resources["DisplayNetworks"]);
        NetworksView = networksView;

        StackLayout = stackLayout;

        MainPageLayoutUpdater.MainPage = this;

        _ = SubstrateClientModel.ChangeConnectedClientsAsync(EndpointsModel.GetSelectedEndpointKeys(), CancellationToken.None);
    }
}