using CommunityToolkit.Mvvm.Input;

namespace PolkadotRoots.Components.Events;

public partial class EventItemView : ContentView
{
    public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
        nameof(ImageSource), typeof(ImageSource), typeof(EventItemView));

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(EventItemView), default(string));

    public static readonly BindableProperty StartTextProperty = BindableProperty.Create(
        nameof(StartText), typeof(string), typeof(EventItemView), default(string));

    public static readonly BindableProperty EndTextProperty = BindableProperty.Create(
        nameof(EndText), typeof(string), typeof(EventItemView), default(string));

    public static readonly BindableProperty LocationProperty = BindableProperty.Create(
        nameof(Location), typeof(string), typeof(EventItemView), default(string));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(IAsyncRelayCommand), typeof(EventItemView));

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(EventItemView));

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? StartText
    {
        get => (string?)GetValue(StartTextProperty);
        set => SetValue(StartTextProperty, value);
    }

    public string? EndText
    {
        get => (string?)GetValue(EndTextProperty);
        set => SetValue(EndTextProperty, value);
    }

    public string? Location
    {
        get => (string?)GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

    public IAsyncRelayCommand? Command
    {
        get => (IAsyncRelayCommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public EventItemView()
    {
        InitializeComponent();
    }
}
