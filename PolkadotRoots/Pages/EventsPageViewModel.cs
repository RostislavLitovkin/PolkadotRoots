using CommunityCore.Events;
using CommunityCore.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.Loading;
using PolkadotRoots.Helpers;
using System.Collections.ObjectModel;

namespace PolkadotRoots.Pages;

public sealed class EventListItem
{
    public string? ImageSource { get; init; }
    public string Title { get; init; } = "Untitled event";
    public string StartText { get; init; } = string.Empty;
    public string EndText { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public long? Id { get; init; }
}

public partial class EventsViewModel : ObservableObject
{
    private readonly CommunityEventsApiClient api;
    private readonly StorageApiClient storage;
    private int pageIndex = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoItems))]
    private bool reachedEnd = false;

    public bool NoItems => Initialized && Items.Count == 0;

    [ObservableProperty]
    private bool busy = false;

    [ObservableProperty]
    private bool isRefreshing = false;

    public bool Initialized { get; private set; }
    public ObservableCollection<EventListItem> Items { get; } = new();

    [RelayCommand]
    public async Task OpenDetailsAsync(object param)
    {
        var loading = DependencyService.Get<FullPageLoadingViewModel>();

        loading.IsVisible = true;
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
        catch { /* ignore navigation errors */ }
        loading.IsVisible = false;
    }

    public EventsViewModel(CommunityEventsApiClient api, StorageApiClient storage)
    {
        this.api = api;
        this.storage = storage;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (Busy) return;
        IsRefreshing = true;
        try
        {
            // Reset paging and content
            pageIndex = 0;
            ReachedEnd = false;
            Initialized = false;
            Items.Clear();

            await LoadNextPageAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    public async Task LoadNextPageAsync()
    {
        if (Busy || ReachedEnd) return;
        Busy = true;
        try
        {
            var page = await api.GetPageAsync(page: pageIndex, size: 20);

            ReachedEnd = page.Last || page.Content.Count == 0;
            pageIndex++;

            foreach (var ev in page.Content)
            {
                var item = await MapAsync(ev);
                Items.Add(item);
            }

            Initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Events exception: ");
            Console.WriteLine(ex);
            // TODO: show error toast/popup
        }
        finally
        {
            Busy = false;
        }
    }

    private async Task<EventListItem> MapAsync(EventDto ev)
    {
        string title = FirstNonEmpty(ev.Name) ?? "Untitled event";
        string venue = FirstNonEmpty(ev.Address, ev.Country) ?? string.Empty;

        var (startText, endText) = TimeDateHelper.FormatTimes(ev.TimeStart, ev.TimeEnd);

        string? imageSrc = await ImageHelper.ResolveImageAsync(ev);

        return new EventListItem
        {
            Id = ev.Id,
            Title = title,
            StartText = startText,
            EndText = endText,
            Location = venue,
            ImageSource = imageSrc
        };
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
}
