using CommunityCore.Admins;
using CommunityCore.Events;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.MessagePopup;
using PlutoFramework.Components.TransferView;
using PlutoFramework.Components.UniversalScannerView;
using PlutoFramework.Components.Vault;
using PlutoFramework.Model;
using Plutonication;
using PolkadotRoots.Components.BottomNavBar;
using PolkadotRoots.Helpers;
using static PolkadotRoots.Components.BottomNavBar.BottomNavBarViewModel;

namespace PolkadotRoots.Pages
{
    public sealed class NextInterestedEventItem
    {
        public long Id { get; init; }
        public string? Title { get; init; }
        public string? StartDate { get; init; }
        public string? ImageSource { get; init; }
    }

    public partial class MainPageViewModel : ObservableObject
    {
        [RelayCommand]
        public Task QrAsync() => Shell.Current.Navigation.PushAsync(new UniversalScannerPage { OnScannedMethod = OnScanned });

        [RelayCommand]
        public Task Settings() => Shell.Current.Navigation.PushAsync(new SettingsPage());

        [ObservableProperty]
        private bool isRefreshing = false;

        [ObservableProperty]
        private bool isAdmin = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNextInterestedEvent))]
        [NotifyPropertyChangedFor(nameof(NoNextInterestedEvent))]
        private NextInterestedEventItem? nextInterestedEvent;

        public bool HasNextInterestedEvent => NextInterestedEvent is not null;
        public bool NoNextInterestedEvent => !HasNextInterestedEvent;

        private bool loadingNextEvent;

        public MainPageViewModel()
        {
            // fire and forget admin check
            _ = RefreshIsAdminAsync();
            _ = LoadNextInterestedEventAsync();
        }

        [RelayCommand]
        public Task ViewDotbacksAsync() => Shell.Current.Navigation.PushAsync(new MyDotbacksPage());

        private async Task RefreshIsAdminAsync()
        {
            try
            {
                if (!KeysModel.HasSubstrateKey())
                {
                    IsAdmin = false;
                    return;
                }

                var myAddress = KeysModel.GetSubstrateKey();

                if (string.IsNullOrWhiteSpace(myAddress) || myAddress.StartsWith("Error"))
                {
                    IsAdmin = false;
                    return;
                }

                var client = new CommunityAdminsApiClient(new HttpClient());
                var admins = await client.GetAllAsync();

                IsAdmin = admins?.Contains(myAddress) == true;
            }
            catch
            {
                IsAdmin = false;
            }
        }

        private async Task LoadNextInterestedEventAsync()
        {
            if (loadingNextEvent)
            {
                return;
            }

            loadingNextEvent = true;
            NextInterestedEvent = null;

            try
            {
                if (!KeysModel.HasSubstrateKey())
                {
                    return;
                }

                var myAddress = KeysModel.GetSubstrateKey();
                if (string.IsNullOrWhiteSpace(myAddress) || myAddress.StartsWith("Error"))
                {
                    return;
                }

                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                EventDto? best = null;

                var page = 0;
                const int size = 20;
                var reachedEnd = false;

                while (!reachedEnd)
                {
                    var eventsPage = await CommunityClientHelper.EventsApi.GetPageAsync(page: page, size: size, hasEnded: false);
                    reachedEnd = eventsPage.Last || eventsPage.Content.Count == 0;
                    page++;

                    foreach (var ev in eventsPage.Content)
                    {
                        if (ev.Id is null || ev.TimeStart is null || ev.TimeStart <= now)
                        {
                            continue;
                        }

                        var interests = await CommunityClientHelper.InterestApi.ListAsync(ev.Id.Value);
                        if (interests.Any(i => i.Address == myAddress))
                        {
                            if (best is null || (ev.TimeStart ?? long.MaxValue) < (best.TimeStart ?? long.MaxValue))
                            {
                                best = ev;
                            }
                        }
                    }

                    if (best is not null)
                    {
                        break;
                    }
                }

                if (best is not null)
                {
                    var startText = TimeDateHelper.FormatTimes(best.TimeStart, best.TimeEnd).start;
                    var imageSrc = await ImageHelper.ResolveImageAsync(best);

                    NextInterestedEvent = new NextInterestedEventItem
                    {
                        Id = best.Id!.Value,
                        Title = best.Name ?? "Untitled event",
                        StartDate = startText,
                        ImageSource = imageSrc
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                loadingNextEvent = false;
            }
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            IsRefreshing = true;

            _ = SubstrateClientModel.ChangeConnectedClientsAsync(EndpointsModel.GetSelectedEndpointKeys(), CancellationToken.None);

            await LoadNextInterestedEventAsync();

            await Task.Delay(5000);

            IsRefreshing = false;

            // also refresh admin flag on manual refresh
            _ = RefreshIsAdminAsync();
        }

        [RelayCommand]
        public Task RegisterEventAsync() => Shell.Current.Navigation.PushAsync(new RegisterEventPage());

        [RelayCommand]
        public Task ExploreEventsAsync() {

            var navbarViewModel = DependencyService.Get<BottomNavBarViewModel>();
            navbarViewModel.Selected = NavBarSelection.Events;

            return Shell.Current.Navigation.PushAsync(new EventsPage());
        } 

        [RelayCommand]
        public async Task OpenNextEventAsync(object? param)
        {
            try
            {
                long? id = null;
                switch (param)
                {
                    case long l: id = l; break;
                    case int i: id = i; break;
                    case string s when long.TryParse(s, out var v): id = v; break;
                }

                if (id.HasValue)
                {
                    await Shell.Current.Navigation.PushAsync(new EventDetailsPage(id.Value));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void OnScanned(object? sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (e.Results.Length <= 0)
                {
                    return;
                }

                try
                {
                    var scannedValue = e.Results[0].Value;

                    // trying to connect to a dApp
                    if (scannedValue.Length > 14 && scannedValue.Substring(0, 14) == "plutonication:")
                    {
                        AccessCredentials ac = new AccessCredentials(new Uri(scannedValue));

                        PlutonicationModel.ProcessAccessCredentials(ac);
                    }
                    else if (scannedValue.Length > 13 && scannedValue.Substring(0, 13) == "plutolayout: ")
                    {
                        // LATER: check validity

                        CustomLayoutModel.SaveLayout(scannedValue);
                    }
                    else if (scannedValue.Length > 10 && scannedValue.Substring(0, 10) == "substrate:")
                    {
                        var viewModel = DependencyService.Get<TransferViewModel>();

                        viewModel.GetFeeAsync();

                        viewModel.IsVisible = true;

                        var scannedAddress = e.Results[e.Results.Length - 1].Value;

                        if (scannedAddress.Substring(10).IndexOf(":") != -1)
                        {
                            viewModel.Address = scannedAddress.Substring(10, scannedAddress.Substring(10).IndexOf(":"));
                        }
                        else
                        {
                            viewModel.Address = scannedAddress.Substring(10);
                        }
                    }
                    else if (Substrate.NetApi.Utils.Bytes2HexString(e.Results[0].Raw).IndexOf("530102") != -1)
                    {
                        var vaultSign = DependencyService.Get<VaultSignViewModel>();

                        await vaultSign.SignExtrinsicAsync("0x" + Substrate.NetApi.Utils.Bytes2HexString(e.Results[0].Raw).Substring(Substrate.NetApi.Utils.Bytes2HexString(e.Results[0].Raw).IndexOf("530102") + 6));
                    }
                    else
                    {
                        var messagePopup = DependencyService.Get<MessagePopupViewModel>();

                        messagePopup.Title = "Unable to read QR code";
                        messagePopup.Text = "The QR code was in incorrect format.";

                        messagePopup.IsVisible = true;
                    }

                    await Shell.Current.Navigation.PopAsync();
                }
                catch (Exception ex)
                {

                    // Does not make much sense now...
                    return;

                    var messagePopup = DependencyService.Get<MessagePopupViewModel>();

                    messagePopup.Title = "BasePage Error";
                    messagePopup.Text = ex.Message;

                    messagePopup.IsVisible = true;
                }
            });
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
        }
    }
}
