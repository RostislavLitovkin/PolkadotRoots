using CommunityCore.Dotback;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Model;
using PolkadotRoots.Helpers;
using Substrate.NetApi;
using System.Collections.ObjectModel;

namespace PolkadotRoots.Pages;

public sealed class DotbackListItem
{
    public required string ImageSource { get; init; }
    public double UsdRequested => Dotback.UsdAmount;
    public long EventId => Dotback.EventId;
    public string Address => Dotback.Address;
    public string PolkadotFormattedAddress => Utils.GetAddressFrom(Utils.GetPublicKeyFrom(Dotback.Address), ss58Prefix: 0);
    public bool Paid => Dotback.Paid;
    public bool Rejected => Dotback.Rejected;
    public double? DotAmountPaid => Dotback.DotAmountPaid;
    public long? UnixDatePaid => Dotback.UnixDatePaid;
    public long? UnixDateOfRequest => Dotback.UnixDateOfRequest;
    public DotbackDto Dotback { get; init; } = null!;
}

public partial class DotbacksViewModel : ObservableObject
{
    private readonly long? eventFilter;

    private int pageIndex = 0; // simulated paging over entire list for now

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoItems))]
    private bool initialized;

    [ObservableProperty]
    private string title = "DOT-back requests";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoItems))]
    private bool reachedEnd = false;

    public bool NoItems => Initialized && Items.Count == 0;

    [ObservableProperty]
    private bool busy = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    private bool isOrganizer;

    public bool DownloadButtonVisible => IsOrganizer && eventFilter.HasValue;
    public ObservableCollection<DotbackListItem> Items { get; } = new();

    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (Initialized) return;
        Initialized = true;

        await LoadOrganizerFlagAsync();
        await LoadNextPageAsync();
    }

    private async Task LoadOrganizerFlagAsync()
    {
        try
        {
            if (eventFilter is not long eid) return;

            var ev = await CommunityClientHelper.EventsApi.GetAsync(eid);
            if (ev?.OrganizatorAddresses == null || ev.OrganizatorAddresses.Count == 0) return;

            var my = KeysModel.GetSubstrateKey();
            IsOrganizer = !string.IsNullOrWhiteSpace(my) && ev.OrganizatorAddresses.Contains(my);
        }
        catch
        {
            IsOrganizer = false;
        }
    }

    [RelayCommand]
    public async Task DownloadCsvAsync()
    {
        if (!DownloadButtonVisible) return;
        if (eventFilter is not long eid) return;

        try
        {
            Busy = true;
            var csv = await CommunityClientHelper.DotbacksStatisticsApi.ExportEventCsvAsync(eid);
            if (csv is null || csv.Length == 0)
            {
                await Shell.Current.DisplayAlertAsync("No data", "No dotbacks found for this event.", "OK");
                return;
            }

            var fileName = $"dotbacks-event-{eid}.csv";
            using var stream = new MemoryStream(csv);
            var result = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                await Shell.Current.DisplayAlertAsync("Saved", $"CSV saved to {result.FilePath}", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error", "Failed to save CSV.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            Busy = false;
        }
    }

    [RelayCommand]
    public async Task OpenDetailsAsync(object param)
    {
        try
        {
            DotbackListItem? item = param as DotbackListItem;
            if (item is null) return;
            await Shell.Current.Navigation.PushAsync(new DotbackDetailsPage(item.Dotback));
        }
        catch { }
    }

    public DotbacksViewModel(long? eventId, string? title)
    {
        this.eventFilter = eventId;
        if (!string.IsNullOrWhiteSpace(title)) Title = title!;
    }

    public async Task LoadNextPageAsync()
    {
        if (Busy || ReachedEnd) return;
        Busy = true;
        try
        {
            // API has no paging, so fetch per event/address; implement simple paging in client
            IReadOnlyList<DotbackDto> list = eventFilter is long eid
                ? await CommunityClientHelper.DotbacksApi.ListByEventAsync(eid)
                : [];

            var chunk = list.Skip(pageIndex * 20).Take(20).ToList();
            if (chunk.Count == 0)
            {
                ReachedEnd = true;
            }
            else
            {
                foreach (var d in chunk)
                {
                    var item = await MapAsync(d);
                    Items.Add(item);
                }
                pageIndex++;
            }
            Initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Dotbacks exception: ");
            Console.WriteLine(ex);
        }
        finally
        {
            Busy = false;
        }
    }

    private async Task<DotbackListItem> MapAsync(DotbackDto d)
    {
        string imageSrc = "";
        try
        {
            if (!string.IsNullOrWhiteSpace(d.ImageUrl))
            {
                Console.WriteLine("Image source: ");
                Console.WriteLine(d.ImageUrl);
                imageSrc = await CommunityClientHelper.StorageApi.GetImageAsync(d.ImageUrl);
                Console.WriteLine(imageSrc);

            }
        }
        catch { }

        return new DotbackListItem
        {
            ImageSource = imageSrc,
            Dotback = d
        };
    }
}
