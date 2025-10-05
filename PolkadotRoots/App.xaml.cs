using PlutoFramework.Model;
using PlutoFramework.Model.Initializers;
using PolkadotRoots.Components.BottomNavBar;

namespace PolkadotRoots
{
    public partial class App : Application
    {
        private bool _isInitialized;

        public App()
        {
            InitializeComponent();

            // Show a lightweight loading UI first.
            MainPage = CreateLoadingPage();

            // Defer heavy work until the first frame renders.
            Dispatcher.Dispatch(async () => await InitializeAsync());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(MainPage ?? CreateLoadingPage());
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            // Let the first frame render before doing heavier setup.
            await Task.Yield();

            _ = Task.Run(PlutoFramework.MauiAppBuilderExtensions.InitializePlutoFrameworkFull);

            NavigationModel.NavigateAfterAccountCreation = NewMainPageNavigationAsync;

            DependencyService.Register<BottomNavBarViewModel>();

            // Switch to the real root page after initialization.
            await SetRootPageAsync(CreateRootPage());
        }

        private static Page CreateLoadingPage()
        {
            return new ContentPage
            {
                Content = new Grid
                {
                    Children =
                    {
                        new ActivityIndicator
                        {
                            IsRunning = true,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                        },
                    },
                },
            };
        }

        public static Task NewMainPageNavigationAsync()
        {
            return SetRootPageAsync(new AppShell());
        }

        public static async Task GenerateNewAccountAsync()
        {
            await KeysModel.GenerateNewAccountAsync();
            await SetRootPageAsync(new AppShell());
        }

        private static Page CreateRootPage()
        {
            return KeysModel.HasSubstrateKey()
                ? new AppShell()
                : new OnboardingShell();
        }

        public static Task SetRootPageAsync(Page page)
        {
            if (MainThread.IsMainThread)
            {
                UpdateWindowPage(page);
                return Task.CompletedTask;
            }

            return MainThread.InvokeOnMainThreadAsync(() => UpdateWindowPage(page));
        }

        private static void UpdateWindowPage(Page page)
        {
            var window = Current?.Windows.FirstOrDefault();
            if (window is not null)
            {
                window.Page = page;
            }
        }

        protected override void OnStart()
        {
            // Launch push notifications services
            PushNotificationsAppInitializer.Initialize(
                "https://notifications-api.plutolabs.app"
            );
        }
    }
}