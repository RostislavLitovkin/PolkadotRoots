using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.WebView;
using PlutoFramework.Model;
using Substrate.NetApi;

namespace PolkadotRoots.Components
{
    public partial class ClaimNftButtonViewModel : ObservableObject
    {
        [ObservableProperty]
        string code = "";

        [RelayCommand]
        public async Task OnButtonPressedAsync()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                return;
            }

            var uri = $"https://dotmemo.xyz/claim/{Code}?address={Utils.GetAddressFrom(KeysModel.GetPublicKeyBytes(), 0)}";
            await Shell.Current.Navigation.PushAsync(new ExtensionWebViewPage(uri));
        }
    }
}
