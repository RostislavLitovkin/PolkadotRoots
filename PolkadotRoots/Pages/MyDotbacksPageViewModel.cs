using CommunityCore.Dotback;
using CommunityCore.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Model;
using System.Collections.ObjectModel;

namespace PolkadotRoots.Pages;

public partial class MyDotbacksViewModel : ObservableObject
{
    private readonly CommunityDotbacksApiClient api;
    private readonly StorageApiClient storage;

    private int pageIndex = 0;
    private IReadOnlyList<DotbackDto> all = Array.Empty<DotbackDto>();

    [ObservableProperty]
    private string title = "My DOT-backs";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoItems))]
    private bool reachedEnd = false;

    public bool NoItems => Initialized && Items.Count == 0;

    [ObservableProperty]
    private bool busy = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoItems))]
    private bool initialized;
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

    public MyDotbacksViewModel(CommunityDotbacksApiClient api, StorageApiClient storage)
    {
        this.api = api;
        this.storage = storage;
    }

    public async Task LoadNextPageAsync()
    {
        if (Busy || ReachedEnd) return;
        Busy = true;
        try
        {
            if (all.Count == 0)
            {
                // initial fetch by current user's substrate address
                if (!KeysModel.HasSubstrateKey())
                {
                    ReachedEnd = true;
                    Initialized = true;
                    return;
                }

                var myAddress = KeysModel.GetSubstrateKey();
                if (string.IsNullOrWhiteSpace(myAddress) || myAddress.StartsWith("Error"))
                {
                    ReachedEnd = true;
                    Initialized = true;
                    return;
                }

                all = await api.ListByAddressAsync(myAddress);
            }

            var chunk = all.Skip(pageIndex * 20).Take(20).ToList();
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
            Console.WriteLine("MyDotbacks exception: ");
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
                imageSrc = await storage.GetImageAsync(d.ImageUrl);
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
