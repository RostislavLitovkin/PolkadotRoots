using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace PolkadotRoots.Components.Countdown;

public partial class AnimatedHmsView : ContentView
{
    public static readonly BindableProperty HoursProperty = BindableProperty.Create(
        nameof(Hours), typeof(int), typeof(AnimatedHmsView), 0, propertyChanged: OnTimeChanged);

    public static readonly BindableProperty MinutesProperty = BindableProperty.Create(
        nameof(Minutes), typeof(int), typeof(AnimatedHmsView), 0, propertyChanged: OnTimeChanged);

    public static readonly BindableProperty SecondsProperty = BindableProperty.Create(
        nameof(Seconds), typeof(int), typeof(AnimatedHmsView), 0, propertyChanged: OnTimeChanged);

    public int Hours
    {
        get => (int)GetValue(HoursProperty);
        set => SetValue(HoursProperty, value);
    }

    public int Minutes
    {
        get => (int)GetValue(MinutesProperty);
        set => SetValue(MinutesProperty, value);
    }

    public int Seconds
    {
        get => (int)GetValue(SecondsProperty);
        set => SetValue(SecondsProperty, value);
    }

    public AnimatedHmsView()
    {
        InitializeComponent();
    }

    private static void OnTimeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AnimatedHmsView view)
        {
            view.UpdateLabels();
        }
    }

    private void UpdateLabels()
    {
        UpdateLabel(HoursLabel, ClampTwoDigit(Hours));
        UpdateLabel(MinutesLabel, ClampTwoDigit(Minutes));
        UpdateLabel(SecondsLabel, ClampTwoDigit(Seconds));
    }

    private static string ClampTwoDigit(int value)
    {
        if (value < 0) value = 0;
        if (value > 99) value = 99;
        return value.ToString("D2");
    }

    private void UpdateLabel(Label label, string newText)
    {
        if (label.Text == newText)
            return;

        _ = AnimateChangeAsync(label, newText);
    }

    private async Task AnimateChangeAsync(Label label, string newText)
    {
        await label.ScaleToAsync(0.9, 70, Easing.CubicOut);
        await label.TranslateToAsync(0, -4, 70, Easing.CubicOut);
        label.Text = newText;
        label.TranslationY = 4;
        await label.TranslateToAsync(0, 0, 140, Easing.CubicIn);
        await label.ScaleToAsync(1, 120, Easing.CubicIn);
    }
}
