using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.Credits;
using PlutoFramework.Components.Keys;
using PlutoFramework.Components.Nova;
using PlutoFramework.Components.Settings;
using PlutoFramework.Model;

namespace PolkadotRoots.Pages
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool hasAccount;
        public SettingsPageViewModel()
        {
            hasAccount = KeysModel.HasSubstrateKey();
        }

        [RelayCommand]
        public Task KeysAsync() => Shell.Current.Navigation.PushAsync(new KeyListPage());

        [RelayCommand]
        public Task DeveloperSettingsAsync() => Shell.Current.Navigation.PushAsync(new DeveloperSettingsPage());

        [RelayCommand]
        public Task ImportFromNovaAsync() => Shell.Current.Navigation.PushAsync(new NovaExportGuidePage());

        [RelayCommand]
        public Task ExportToNovaAsync() => Browser.Default.OpenAsync("https://docs.novawallet.io/nova-wallet-wiki/wallet-management/import-an-existing-wallet/import-via-passphrase", BrowserLaunchMode.SystemPreferred);

        [RelayCommand]
        public Task CreditsAsync() => Shell.Current.Navigation.PushAsync(new CreditsPage());

    }
}
