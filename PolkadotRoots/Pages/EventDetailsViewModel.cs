using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Components.WebView;
using PlutoFramework.Model;
using PolkadotRoots.Helpers;
using Substrate.NetApi;
using System.Collections.ObjectModel;
using System.Text;

namespace PolkadotRoots.Pages;

public partial class EventDetailsViewModel : ObservableObject
{
    private readonly long id;
    private DateTimeOffset? startTime;
    private DateTimeOffset? endTime;
    private IDispatcherTimer? countdownTimer;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string? bannerImage;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string startText = string.Empty;
    [ObservableProperty] private string endText = string.Empty;
    [ObservableProperty] private string addressLine = string.Empty;
    [ObservableProperty] private string priceText = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLumaUrl))]
    private string? lumaUrl;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasWebsite))]
    private string? website;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCapacityText))]
    private string capacityText = string.Empty;
    [ObservableProperty] private string country = string.Empty;
    [ObservableProperty] private string daysOnlyText = string.Empty;
    [ObservableProperty] private bool showDaysOnly = false;
    [ObservableProperty] private bool inProgressIsVisible = false;
    [ObservableProperty] private bool eventEndedIsVisible = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountdownTextIsVisible))]
    private bool showTimeBreakdown;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMapsLink))]
    private string? mapsUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGoogleMeets))]
    private string? googleMeetsUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasZoom))]
    private string? zoomUrl;

    public bool CountdownTextIsVisible => !ShowTimeBreakdown;
    [ObservableProperty] private int hoursRemaining;
    [ObservableProperty] private int minutesRemaining;
    [ObservableProperty] private int secondsRemaining;

    public bool HasLumaUrl => !string.IsNullOrWhiteSpace(LumaUrl);
    public bool HasWebsite => !string.IsNullOrWhiteSpace(Website);
    public bool HasCapacityText => !string.IsNullOrWhiteSpace(CapacityText);
    public bool HasMapsLink => !string.IsNullOrWhiteSpace(MapsUrl) || !string.IsNullOrWhiteSpace(AddressLine);
    public bool HasGoogleMeets => !string.IsNullOrWhiteSpace(GoogleMeetsUrl);
    public bool HasZoom => !string.IsNullOrWhiteSpace(ZoomUrl);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SupportsDotbackText))]
    [NotifyPropertyChangedFor(nameof(DotbackButtonIsVisible))]
    [NotifyPropertyChangedFor(nameof(IsOrganizerAndSupportsDotback))]
    private bool supportsDotback;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTelegram))]
    private string? telegram;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDiscord))]
    private string? discord;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasX))]
    private string? x;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasYoutube))]
    private string? youtube;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEventbrite))]
    private string? eventbrite;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImportedFrom))]
    private string? importedFrom;

    public bool HasTelegram => !string.IsNullOrWhiteSpace(Telegram);
    public bool HasDiscord => !string.IsNullOrWhiteSpace(Discord);
    public bool HasX => !string.IsNullOrWhiteSpace(X);
    public bool HasYoutube => !string.IsNullOrWhiteSpace(Youtube);
    public bool HasEventbrite => !string.IsNullOrWhiteSpace(Eventbrite);
    public bool HasImportedFrom => !string.IsNullOrWhiteSpace(ImportedFrom);

    public string SupportsDotbackText => SupportsDotback ? "Yes" : "No";
    public bool DotbackButtonIsVisible => SupportsDotback;
    public bool IsOrganizerAndSupportsDotback => SupportsDotback && IsOrganizer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOrganizer))]
    [NotifyPropertyChangedFor(nameof(OrganizersPolkadotFormatted))]
    [NotifyPropertyChangedFor(nameof(IsOrganizerOrAdmin))]
    [NotifyPropertyChangedFor(nameof(IsOrganizerAndSupportsDotback))]
    private ObservableCollection<string> organizers = new();

    public ObservableCollection<string> OrganizersPolkadotFormatted => new(
        Organizers.Select(address => Utils.GetAddressFrom(Utils.GetPublicKeyFrom(address), ss58Prefix: 0))
    );

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InterestedText))]
    private long? interested = null;

    public string InterestedText => Interested.HasValue ? $"Interested: {Interested}" : "Interested: loading";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InterestButtonIsVisible))]
    private bool isInterested = false;

    public bool InterestButtonIsVisible => !IsInterested;

    public bool IsOrganizer => Organizers.Contains(KeysModel.GetSubstrateKey());

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOrganizerOrAdmin))]
    private bool isAdmin;

    public bool IsOrganizerOrAdmin => IsOrganizer || IsAdmin;

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
            var response = await CommunityClientHelper.InterestApi.RegisterAsync(account, id, timestamp);

            IsInterested = true;

            Interested++;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }


    [RelayCommand]
    public Task DotbackAsync() => Shell.Current.Navigation.PushAsync(new DotbackRegistrationPage(id, Title, Country));

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


    [RelayCommand]
    public Task EditEventAsync() => Shell.Current.Navigation.PushAsync(new RegisterEventPage(id));

    [RelayCommand]
    public async Task DeleteEventAsync()
    {
        if (!IsOrganizerOrAdmin)
            return;

        var confirm = await Shell.Current.DisplayAlertAsync("Delete event", "Are you sure you want to delete this event?", "Delete", "Cancel");
        if (!confirm)
            return;

        try
        {
            var account = await KeysModel.GetAccountAsync();
            if (account is null)
                return;

            var ok = await CommunityClientHelper.EventsApi.DeleteAsync(account, id);
            if (ok)
            {
                await Shell.Current.DisplayAlertAsync("Deleted", "Event has been deleted.", "OK");
                await Shell.Current.Navigation.PopAsync();
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Not found", "Event was not found.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    public EventDetailsViewModel(long id)
    {
        this.id = id;
    }

    [RelayCommand]
    private async Task OpenUrlAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        await Shell.Current.Navigation.PushAsync(new ExtensionWebViewPage(url));
    }

    [RelayCommand]
    private async Task OpenUrlInLauncherAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        await Launcher.Default.OpenAsync(new Uri(url));
    }

    [RelayCommand]
    private async Task OpenMapsAsync()
    {
        if (!HasMapsLink) return;

        string? url = !string.IsNullOrWhiteSpace(MapsUrl)
            ? MapsUrl
            : (!string.IsNullOrWhiteSpace(AddressLine) ? $"https://www.google.com/maps?q={Uri.EscapeDataString(AddressLine)}" : null);

        if (string.IsNullOrWhiteSpace(url)) return;

        await Launcher.Default.OpenAsync(new Uri(url));
    }

    [RelayCommand]
    private async Task OpenOrganizerAsync(string? addr)
    {
        if (string.IsNullOrWhiteSpace(addr)) return;
        var url = $"https://assethub-polkadot.subscan.io/account/{Uri.EscapeDataString(addr)}";
        await Shell.Current.Navigation.PushAsync(new ExtensionWebViewPage(url));
    }

    [RelayCommand]
    private async Task AddToCalendarAsync()
    {
        if (startTime is null)
        {
            await Shell.Current.DisplayAlertAsync("Missing date", "Event start time is not available.", "OK");
            return;
        }

        try
        {
            var startUtc = startTime.Value.ToUniversalTime();
            var endUtc = (endTime ?? startTime.Value.AddHours(1)).ToUniversalTime();

            string Escape(string value) => value
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n");

            var safeTitle = string.IsNullOrWhiteSpace(Title) ? "Event" : Title;
            var builder = new StringBuilder()
                .AppendLine("BEGIN:VCALENDAR")
                .AppendLine("VERSION:2.0")
                .AppendLine("PRODID:-//PolkadotRoots//EN")
                .AppendLine("BEGIN:VEVENT")
                .AppendLine($"UID:{Guid.NewGuid()}")
                .AppendLine($"DTSTAMP:{DateTimeOffset.UtcNow:yyyyMMddTHHmmssZ}")
                .AppendLine($"DTSTART:{startUtc:yyyyMMddTHHmmssZ}")
                .AppendLine($"DTEND:{endUtc:yyyyMMddTHHmmssZ}")
                .AppendLine($"SUMMARY:{Escape(safeTitle)}");

            if (!string.IsNullOrWhiteSpace(Description))
            {
                builder.AppendLine($"DESCRIPTION:{Escape(Description)}");
            }

            if (!string.IsNullOrWhiteSpace(AddressLine))
            {
                builder.AppendLine($"LOCATION:{Escape(AddressLine)}");
            }

            builder.AppendLine("END:VEVENT")
                   .AppendLine("END:VCALENDAR");

            var filePath = Path.Combine(FileSystem.CacheDirectory, $"event-{id}.ics");
            await File.WriteAllTextAsync(filePath, builder.ToString());

            await Launcher.Default.OpenAsync(new OpenFileRequest("Add to calendar", new ReadOnlyFile(filePath)));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    public async Task LoadAsync()
    {
        var eventTask = CommunityClientHelper.EventsApi.GetAsync(id);
        var interestTask = CommunityClientHelper.InterestApi.ListAsync(id);
        var adminsTask = CommunityClientHelper.AdminsApi.GetAllAsync();

        var ev = await eventTask;
        if (ev == null) return;

        Title = ev.Name ?? "Untitled event";

        var (startText, endText) = TimeDateHelper.FormatTimes(ev.TimeStart, ev.TimeEnd);
        StartText = startText; EndText = endText;

        startTime = FromUnixMaybe(ev.TimeStart);
        endTime = FromUnixMaybe(ev.TimeEnd);

        Description = ev.Description ?? string.Empty;
        AddressLine = FirstNonEmpty(ev.Address, ev.Country) ?? string.Empty;
        PriceText = string.IsNullOrWhiteSpace(ev.Price) ? "See details" : ev.Price;
        LumaUrl = ev.LumaUrl; Website = ev.Website;
        MapsUrl = ev.MapsUrl;
        GoogleMeetsUrl = ev.GoogleMeetsUrl;
        ZoomUrl = ev.ZoomUrl;
        CapacityText = ev.Capacity.HasValue ? ev.Capacity.Value.ToString() : string.Empty;
        Country = ev.Country!;
        SupportsDotback = ev.SupportsDotback ?? false;
        Telegram = ev.Telegram;
        Discord = ev.Discord;
        X = ev.X;
        Youtube = ev.Youtube;
        Eventbrite = ev.Eventbrite;
        ImportedFrom = ev.ImportedFrom;

        if (ev.OrganizatorAddresses != null)
            Organizers = new ObservableCollection<string>(ev.OrganizatorAddresses);

        BannerImage = await CommunityClientHelper.StorageApi.GetImageAsync(ev.Image!);

        var interest = await interestTask;

        Interested = interest?.Count ?? 0;

        var address = KeysModel.GetSubstrateKey("");
        IsInterested = interest?.Any(i => i.Address == address) ?? false;

        try
        {
            var admins = await adminsTask;
            IsAdmin = admins?.Contains(address) == true;
        }
        catch
        {
            IsAdmin = false;
        }

        UpdateCountdown();
        StartCountdown();
    }

    public void Stop()
    {
        StopCountdown();
    }

    private void StartCountdown()
    {
        StopCountdown();

        UpdateCountdown();

        if (Application.Current?.Dispatcher is null)
            return;

        countdownTimer = Application.Current.Dispatcher.CreateTimer();
        countdownTimer.Interval = TimeSpan.FromSeconds(1);
        countdownTimer.Tick += OnCountdownTick;
        countdownTimer.Start();
    }

    private void StopCountdown()
    {
        if (countdownTimer != null)
        {
            countdownTimer.Tick -= OnCountdownTick;
            countdownTimer.Stop();
            countdownTimer = null;
        }
    }

    private void OnCountdownTick(object? sender, EventArgs e) => UpdateCountdown();

    private void UpdateCountdown()
    {
        ShowDaysOnly = false;
        ShowTimeBreakdown = false;
        EventEndedIsVisible = false;
        InProgressIsVisible = false;
        DaysOnlyText = string.Empty;

        if (startTime is null)
        {
            HoursRemaining = MinutesRemaining = SecondsRemaining = 0;
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var startUtc = startTime.Value.ToUniversalTime();
        var endUtc = endTime?.ToUniversalTime();

        if (endUtc is not null && now >= endUtc.Value)
        {
            HoursRemaining = MinutesRemaining = SecondsRemaining = 0;
            EventEndedIsVisible = true;
            return;
        }

        if (now > startUtc && now < endUtc)
        {
            InProgressIsVisible = true;
            return;
        }

        var span = startUtc - now;
        if (span < TimeSpan.Zero) span = TimeSpan.Zero;

        if (span >= TimeSpan.FromDays(1))
        {
            ShowDaysOnly = true;
            DaysOnlyText = $"{span.Days} day{(span.Days == 1 ? string.Empty : "s")}";
            HoursRemaining = MinutesRemaining = SecondsRemaining = 0;
            return;
        }

        ShowTimeBreakdown = true;
        HoursRemaining = span.Hours + span.Days * 24;
        MinutesRemaining = span.Minutes;
        SecondsRemaining = span.Seconds;
    }

    private static DateTimeOffset? FromUnixMaybe(long? val)
    {
        if (val is null) return null;
        try
        {
            var v = val.Value;

            if (v < 1_000_000_000_000)
                return DateTimeOffset.FromUnixTimeSeconds(v);

            return DateTimeOffset.FromUnixTimeMilliseconds(v);
        }
        catch { return null; }
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
}
