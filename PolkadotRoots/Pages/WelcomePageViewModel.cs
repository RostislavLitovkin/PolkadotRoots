

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.Mnemonics;
using PlutoFramework.Components.Password;
using PlutoFramework.Model;

namespace PolkadotRoots.Pages
{
    public partial class WelcomePageViewModel : ObservableObject
    {
        [RelayCommand]
        public Task JoinAsync() => Shell.Current.Navigation.PushAsync(new SetupPasswordPage
        {
            Navigation = App.GenerateNewAccountAsync
        });

        // Lets go pyramid code
        [RelayCommand]
        public Task ImportAccountAsync() => Shell.Current.Navigation.PushAsync(
            new SetupPasswordPage()
            {
                Navigation = () => Shell.Current.Navigation.PushAsync(
                    new EnterMnemonicsPage(
                        new EnterMnemonicsViewModel
                        {
                            Navigation = NavigationModel.NavigateAfterAccountCreation
                        }
                    )
                )
            }
        );
    }
}
