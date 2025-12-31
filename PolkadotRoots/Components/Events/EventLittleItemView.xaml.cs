using CommunityToolkit.Mvvm.Input;

namespace PolkadotRoots.Components.Events;

public partial class EventLittleItemView : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(EventLittleItemView), default(string));

    public static readonly BindableProperty StartDateProperty = BindableProperty.Create(
        nameof(StartDate), typeof(string), typeof(EventLittleItemView), default(string));

    public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
        nameof(ImageSource), typeof(string), typeof(EventLittleItemView));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(IAsyncRelayCommand), typeof(EventLittleItemView)
        );

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(EventLittleItemView)
        );
    public EventLittleItemView()
    {
        InitializeComponent();
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? ImageSource
    {
        get => (string?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public string? StartDate
    {
        get => (string?)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    public IAsyncRelayCommand Command
    {
        get => (IAsyncRelayCommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty)!;
        set => SetValue(CommandParameterProperty, value);
    }
}