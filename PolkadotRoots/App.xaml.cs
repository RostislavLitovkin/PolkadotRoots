using Microsoft.Maui.ApplicationModel;
using PlutoFramework.Model;
using PlutoFrameworkCore;
using PolkadotRoots.Components.BottomNavBar;
using System.Linq;

namespace PolkadotRoots
{
    public partial class App : Application
    {
        public App()
        {
            NavigationModel.NavigateAfterAccountCreation = NewMainPageNavigationAsync;

            InitializeComponent();

            DependencyService.Register<BottomNavBarViewModel>();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(CreateRootPage());
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
    }
}