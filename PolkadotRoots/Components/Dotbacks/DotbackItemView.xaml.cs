using CommunityToolkit.Mvvm.Input;
using PlutoFramework.Model.Currency;
using PlutoFramework.Model.HydraDX;

namespace PolkadotRoots.Components.Dotbacks;

public partial class DotbackItemView : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(DotbackItemView), default(string));

    public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
        nameof(ImageSource), typeof(string), typeof(DotbackItemView));

    public static readonly BindableProperty UsdRequestedProperty = BindableProperty.Create(
        nameof(UsdRequested), typeof(double), typeof(DotbackItemView), 0.0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            var control = (DotbackItemView)bindable;
            control.usdLabel.Text = ((double)newValue).ToCurrencyString();

            var dotSpotPrice = Sdk.GetSpotPrice("DOT");
            Console.WriteLine($"DOT Spot Price: {dotSpotPrice}");

            var dotAmount = String.Format((string)Application.Current.Resources["CurrencyFormat"], (double)newValue / dotSpotPrice);

            control.dotPriceLabelText.Text = $"{dotAmount} DOT";
        });

    public static readonly BindableProperty PaidProperty = BindableProperty.Create(
        nameof(Paid), typeof(bool), typeof(DotbackItemView), propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (!(bool)newValue)
            {
                return;
            }

            var control = (DotbackItemView)bindable;
            control.operationLabelText.Text = "Paid";
            control.operationLabelText.TextColor = Colors.Green;
        });

    public static readonly BindableProperty RejectedProperty = BindableProperty.Create(
        nameof(Rejected), typeof(bool), typeof(DotbackItemView),
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (!(bool)newValue)
            {
                return;
            }

            var control = (DotbackItemView)bindable;
            control.operationLabelText.Text = "Rejected";
            control.operationLabelText.TextColor = Colors.DarkRed;
        });

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(IAsyncRelayCommand), typeof(DotbackItemView)
        );

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(DotbackItemView)
        );
    public DotbackItemView()
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

    public double UsdRequested
    {
        get => (double)GetValue(UsdRequestedProperty);
        set => SetValue(UsdRequestedProperty, value);
    }

    public bool Paid
    {
        get => (bool)GetValue(PaidProperty);
        set => SetValue(PaidProperty, value);
    }

    public bool Rejected
    {
        get => (bool)GetValue(RejectedProperty);
        set => SetValue(RejectedProperty, value);
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