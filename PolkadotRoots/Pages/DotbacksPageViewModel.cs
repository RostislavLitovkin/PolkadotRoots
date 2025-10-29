using CommunityCore.Dotback;
using CommunityCore.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PolkadotRoots.Pages;

public sealed class DotbackListItem
{
    public required string ImageSource { get; init; }
    public double UsdRequested => Dotback.UsdAmount;
    public long EventId => Dotback.EventId;
    public string Address => Dotback.Address;
    public bool Paid => Dotback.Paid;
    public bool Rejected => Dotback.Rejected;
    public DotbackDto Dotback { get; init; } = null!;
}

public partial class DotbacksViewModel : ObservableObject
{
    private readonly CommunityDotbacksApiClient api;
    private readonly StorageApiClient storage;
    private readonly long? eventFilter;

    private int pageIndex = 0; // simulated paging over entire list for now

    [ObservableProperty]
    private string title = "DOT-back requests";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoItems))]
    private bool reachedEnd = false;

    public bool NoItems => Initialized && Items.Count == 0;

    [ObservableProperty]
    private bool busy = false;

    public bool Initialized { get; private set; }
    public ObservableCollection<DotbackListItem> Items { get; } = new();

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

    public DotbacksViewModel(CommunityDotbacksApiClient api, StorageApiClient storage, long? eventId, string? title)
    {
        this.api = api;
        this.storage = storage;
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
                ? await api.ListByEventAsync(eid)
                : await api.ListByAddressAsync(""); // not supported; fallback in UI would be event-only usage

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
                imageSrc = await storage.GetImageAsync(d.ImageUrl);
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
