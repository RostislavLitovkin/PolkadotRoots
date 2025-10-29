using CommunityCore.Events;
using CommunityCore.Interest;
using CommunityCore.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Model;
using System.Collections.ObjectModel;

namespace PolkadotRoots.Pages;

public partial class EventDetailsViewModel : ObservableObject
{
    private readonly CommunityEventsApiClient api;
    private readonly StorageApiClient storage;
    private readonly CommunityInterestApiClient interestApi;
    private readonly long id;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string subtitle = string.Empty;
    [ObservableProperty] private string bannerImage;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string startText = string.Empty;
    [ObservableProperty] private string endText = string.Empty;
    [ObservableProperty] private string addressLine = string.Empty;
    [ObservableProperty] private string priceText = string.Empty;
    [ObservableProperty] private string? lumaUrl;
    [ObservableProperty] private string? website;
    [ObservableProperty] private string capacityText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InterestedText))]
    private long? interested = null;

    public string InterestedText => Interested.HasValue ? $"Interested: {Interested}" : "Interested: loading";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InterestButtonIsVisible))]
    private bool isInterested = false;

    public bool InterestButtonIsVisible => !IsInterested;

    [RelayCommand]
    public async Task InterestAsync()
    {
        if (IsInterested)
        {
            return;
        }

        var account = await KeysModel.GetAccountAsync("");

        if (account is null)
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        try
        {
            var response = await interestApi.RegisterAsync(account, id, timestamp);

            IsInterested = true;

            Interested++;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }


    [RelayCommand]
    public Task DotbackAsync() => Shell.Current.Navigation.PushAsync(new DotbackRegistrationPage(id, Title));

    [RelayCommand]
    public async Task ManageDotbacksAsync()
    {
        try
        {
            await Shell.Current.Navigation.PushAsync(new DotbacksPage(eventId: id, title: $"{Title} DOT-backs"));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }


    public ObservableCollection<string> Organizers { get; } = new();

    public EventDetailsViewModel(CommunityEventsApiClient api, StorageApiClient storage, CommunityInterestApiClient interestApi, long id)
    {
        this.api = api;
        this.storage = storage;
        this.interestApi = interestApi;
        this.id = id;
    }

    [RelayCommand]
    private async Task OpenUrlAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        await Launcher.Default.OpenAsync(new Uri(url));
    }

    [RelayCommand]
    private async Task OpenMapsAsync()
    {
        if (string.IsNullOrWhiteSpace(AddressLine)) return;
        var url = $"https://www.google.com/maps?q={Uri.EscapeDataString(AddressLine)}";
        await Launcher.Default.OpenAsync(new Uri(url));
    }

    [RelayCommand]
    private async Task OpenOrganizerAsync(string? addr)
    {
        if (string.IsNullOrWhiteSpace(addr)) return;
        var url = $"https://assethub-polkadot.subscan.io/account/{Uri.EscapeDataString(addr)}";
        await Launcher.Default.OpenAsync(new Uri(url));
    }

    public async Task LoadAsync()
    {
        var eventTask = api.GetAsync(id);
        var interestTask = interestApi.ListAsync(id);

        var ev = await eventTask;
        if (ev == null) return;

        Title = ev.Name ?? "Untitled event";

        var (startText, endText, subtitle) = FormatTimes(ev.TimeStart, ev.TimeEnd,
            FirstNonEmpty(ev.Address, ev.Country) ?? string.Empty);
        StartText = startText; EndText = endText; Subtitle = subtitle;

        Description = ev.Description ?? string.Empty;
        AddressLine = FirstNonEmpty(ev.Address, ev.Country) ?? string.Empty;
        PriceText = string.IsNullOrWhiteSpace(ev.Price) ? "See details" : ev.Price;
        LumaUrl = ev.LumaUrl; Website = ev.Website;
        CapacityText = ev.Capacity.HasValue ? ev.Capacity.Value.ToString() : string.Empty;

        Organizers.Clear();
        if (ev.OrganizatorAddresses != null)
            foreach (var a in ev.OrganizatorAddresses) Organizers.Add(a);

        BannerImage = await storage.GetImageAsync(ev.Image!);

        var inter = await interestTask;

        Interested = inter?.Count ?? 0;

        var address = KeysModel.GetSubstrateKey("");
        IsInterested = inter?.Any(i => i.Address == address) ?? false;

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

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
}
