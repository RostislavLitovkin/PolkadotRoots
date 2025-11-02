namespace PolkadotRoots.Controls;

public class BackgroundVideoView : View
{
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(
        nameof(Source), typeof(string), typeof(BackgroundVideoView), default(string));

    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly BindableProperty IsLoopingProperty = BindableProperty.Create(
        nameof(IsLooping), typeof(bool), typeof(BackgroundVideoView), true);

    public bool IsLooping
    {
        get => (bool)GetValue(IsLoopingProperty);
        set => SetValue(IsLoopingProperty, value);
    }

    public static readonly BindableProperty ShouldAutoPlayProperty = BindableProperty.Create(
        nameof(ShouldAutoPlay), typeof(bool), typeof(BackgroundVideoView), true);

    public bool ShouldAutoPlay
    {
        get => (bool)GetValue(ShouldAutoPlayProperty);
        set => SetValue(ShouldAutoPlayProperty, value);
    }
}
