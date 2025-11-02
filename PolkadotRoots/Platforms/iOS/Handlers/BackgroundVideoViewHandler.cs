#if IOS
using AVFoundation;
using AVKit;
using CoreMedia;
using Foundation;
using Microsoft.Maui.Handlers;
using PolkadotRoots.Controls;
using UIKit;

namespace PolkadotRoots.Platforms.iOS.Handlers;

public class BackgroundVideoViewHandler : ViewHandler<BackgroundVideoView, UIView>
{
    AVPlayer? _player;
    AVPlayerLayer? _layer;

    public BackgroundVideoViewHandler() : base(ViewMapper) { }

    public static IPropertyMapper<BackgroundVideoView, BackgroundVideoViewHandler> ViewMapper = new PropertyMapper<BackgroundVideoView, BackgroundVideoViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(BackgroundVideoView.Source)] = MapSource,
        [nameof(BackgroundVideoView.IsLooping)] = MapSource,
        [nameof(BackgroundVideoView.ShouldAutoPlay)] = MapSource,
    };

    protected override UIView CreatePlatformView()
    {
        var v = new UIView { BackgroundColor = UIColor.Black };
        return v;
    }

    protected override void ConnectHandler(UIView platformView)
    {
        base.ConnectHandler(platformView);
        UpdateSource();

        if (VirtualView != null)
            VirtualView.SizeChanged += OnSizeChanged;
    }

    protected override void DisconnectHandler(UIView platformView)
    {
        if (VirtualView != null)
            VirtualView.SizeChanged -= OnSizeChanged;

        _player?.Pause();
        _player = null;
        _layer?.RemoveFromSuperLayer();
        _layer = null;
        base.DisconnectHandler(platformView);
    }

    void OnSizeChanged(object? sender, EventArgs e)
    {
        if (_layer != null && PlatformView != null)
            _layer.Frame = PlatformView.Bounds;
    }

    static void MapSource(BackgroundVideoViewHandler handler, BackgroundVideoView view)
        => handler.UpdateSource();

    void UpdateSource()
    {
        if (PlatformView == null || VirtualView == null)
            return;

        if (string.IsNullOrWhiteSpace(VirtualView.Source))
            return;

        var url = NSUrl.FromString(VirtualView.Source);
        _player = new AVPlayer(new AVPlayerItem(url));
        _player.Muted = true;

        _layer?.RemoveFromSuperLayer();
        _layer = AVPlayerLayer.FromPlayer(_player);
        _layer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill; // AspectFill
        _layer.Frame = PlatformView.Bounds;
        PlatformView.Layer.AddSublayer(_layer);

        if (VirtualView.IsLooping)
        {
            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, n =>
            {
                _player?.Seek(CoreMedia.CMTime.Zero);
                _player?.Play();
            }, _player.CurrentItem);
        }

        if (VirtualView.ShouldAutoPlay)
            _player.Play();
    }
}
#endif
