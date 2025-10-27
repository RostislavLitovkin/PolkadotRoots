using CommunityCore;
using CommunityCore.Events;
using CommunityCore.Storage;
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

public sealed class EventsViewModel
{
    private readonly CommunityEventsApiClient api;
    private readonly StorageApiClient storage;
    private int pageIndex = 0;
    private bool reachedEnd = false;
    private bool busy = false;

    public bool Initialized { get; private set; }
    public ObservableCollection<EventListItem> Items { get; } = new();

    public ICommand OpenDetailsCommand { get; }

    public EventsViewModel(CommunityEventsApiClient api, StorageApiClient storage)
    {
        this.api = api;
        this.storage = storage;

        OpenDetailsCommand = new Command<object?>(async param =>
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
                    await Application.Current.MainPage.Navigation.PushAsync(new EventDetailsPage(id.Value));
                }
            }
            catch { /* ignore navigation errors */ }
        });
    }

    public async Task LoadNextPageAsync()
    {
        Console.WriteLine("Load next called");
        if (busy || reachedEnd) return;
        busy = true;
        try
        {
            Console.WriteLine("Loading next events...");


            var page = await api.GetPageAsync(page: pageIndex, size: 20);

            Console.WriteLine("events: " + page.Content.Count);

            reachedEnd = page.Last || page.Content.Count == 0;
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
            busy = false;
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
        var c = ev.Image;
        if (!string.IsNullOrWhiteSpace(c))
        {
            if (Uri.TryCreate(c, UriKind.Absolute, out var _)) return c; // already URL
            try
            {
                string folder = "events";
                string file = c;
                var idx = c.LastIndexOf('/');
                if (idx > 0)
                {
                    folder = c.Substring(0, idx);
                    file = c[(idx + 1)..];
                }
                var url = await storage.GetImageAsync(file, folder: folder);
                if (!string.IsNullOrWhiteSpace(url)) return url;
            }
            catch { /* ignore */ }
        }
        return null;
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
}
