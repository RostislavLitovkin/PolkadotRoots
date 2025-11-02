using CommunityCore.Events;
using CommunityCore.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.Loading;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PolkadotRoots.Pages;

public sealed class EventListItem
{
    public string? ImageSource { get; init; }
    public string Title { get; init; } = "Untitled event";
    public string Subtitle { get; init; } = string.Empty;
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
                await Application.Current.MainPage.Navigation.PushAsync(new EventDetailsPage(id.Value));
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

        var (startText, endText, subtitle) = FormatTimes(ev.TimeStart, ev.TimeEnd, venue);

        string? imageSrc = await ResolveImageAsync(ev);

        return new EventListItem
        {
            Id = ev.Id,
            Title = title,
            Subtitle = subtitle,
            StartText = startText,
            EndText = endText,
            Location = venue,
            ImageSource = imageSrc
        };
    }

    private static (string start, string end, string subtitle) FormatTimes(long? start, long? end, string venue)
    {
        DateTimeOffset? s = FromUnixMaybe(start);
        DateTimeOffset? e = FromUnixMaybe(end);

        string startText = s.HasValue ? s.Value.ToLocalTime().ToString("ddd, MMM d • HH:mm") : "TBA";
        string endText = e.HasValue ? e.Value.ToLocalTime().ToString("ddd, MMM d • HH:mm") : "TBA";

        string subtitle;
        if (s.HasValue && e.HasValue)
        {
            bool sameDay = s.Value.Date == e.Value.Date;
            if (sameDay)
                subtitle = $"{s.Value.ToLocalTime():ddd, MMM d • HH:mm} – {e.Value.ToLocalTime():HH:mm}";
            else
                subtitle = $"{s.Value.ToLocalTime():ddd, MMM d • HH:mm} – {e.Value.ToLocalTime():ddd, MMM d • HH:mm}";
        }
        else if (s.HasValue)
            subtitle = s.Value.ToLocalTime().ToString("ddd, MMM d • HH:mm");
        else
            subtitle = "Date to be announced";

        if (!string.IsNullOrWhiteSpace(venue))
            subtitle = string.IsNullOrWhiteSpace(subtitle) ? venue : $"{subtitle} • {venue}";

        return (startText, endText, subtitle);
    }

    private static DateTimeOffset? FromUnixMaybe(long? val)
    {
        if (val is null) return null;
        try
        {
            var v = val.Value;
            // seconds vs millis
            if (v < 1_000_000_000_000) v *= 1000;
            return DateTimeOffset.FromUnixTimeMilliseconds(v);
        }
        catch { return null; }
    }

    private async Task<string?> ResolveImageAsync(EventDto ev)
    {
        if (string.IsNullOrWhiteSpace(ev.Image))
        {
            return null;

        }

        try
        {
            var url = await storage.GetImageAsync(ev.Image);
            if (!string.IsNullOrWhiteSpace(url)) return url;
        }
        catch { /* ignore */ }

        return null;

    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
}
