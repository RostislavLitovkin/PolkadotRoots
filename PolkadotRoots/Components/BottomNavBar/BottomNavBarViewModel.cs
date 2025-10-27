using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PolkadotRoots.Components.BottomNavBar
{

    public partial class BottomNavBarViewModel : ObservableObject
    {
        public enum NavBarSelection
        {
            Assets,
            Nfts,
            Events
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AssetsIsSelected))]
        [NotifyPropertyChangedFor(nameof(NftsIsSelected))]
        [NotifyPropertyChangedFor(nameof(EventsIsSelected))]
        private NavBarSelection selected = NavBarSelection.Nfts;

        public bool AssetsIsSelected => Selected == NavBarSelection.Assets;

        public bool NftsIsSelected => Selected == NavBarSelection.Nfts;

        public bool EventsIsSelected => Selected == NavBarSelection.Events;

        [RelayCommand]
        public async Task SelectAssetsAsync()
        {
            if (AssetsIsSelected)
            {
                return;
            }

            Selected = NavBarSelection.Assets;

            await Shell.Current.GoToAsync("//Assets", animate: false);
        }

        [RelayCommand]
        public async Task SelectNftsAsync()
        {
            if (NftsIsSelected)
            {
                return;
            }

            Selected = NavBarSelection.Nfts;

            await Shell.Current.GoToAsync("//MainPage", animate: false);
        }

        [RelayCommand]
        public async Task SelectEventsAsync()
        {
            if (EventsIsSelected)
            {
                return;
            }

            Selected = NavBarSelection.Events;

            await Shell.Current.GoToAsync("//Events", animate: false);
        }
    }
}
