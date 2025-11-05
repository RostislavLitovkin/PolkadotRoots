#if ANDROID
using Android.Content;
using Android.Widget;
using Android.Net;
using Microsoft.Maui.Handlers;
using PolkadotRoots.Controls;
using Uri = Android.Net.Uri;
using Android.Media;

namespace PolkadotRoots.Platforms.Android.Handlers;

public class BackgroundVideoViewHandler : ViewHandler<BackgroundVideoView, VideoView>
{
    public BackgroundVideoViewHandler() : base(ViewMapper)
    {
    }

    public static IPropertyMapper<BackgroundVideoView, BackgroundVideoViewHandler> ViewMapper = new PropertyMapper<BackgroundVideoView, BackgroundVideoViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(BackgroundVideoView.Source)] = MapSource,
        [nameof(BackgroundVideoView.IsLooping)] = MapSource,
        [nameof(BackgroundVideoView.ShouldAutoPlay)] = MapSource,
    };

    protected override VideoView CreatePlatformView()
    {
        var vv = new VideoView(Context);
        vv.SetOnPreparedListener(new PreparedListener(this));
        return vv;
    }

    protected override void ConnectHandler(VideoView platformView)
    {
        base.ConnectHandler(platformView);
        UpdateSource();
    }

    static void MapSource(BackgroundVideoViewHandler handler, BackgroundVideoView view)
        => handler.UpdateSource();

    void UpdateSource()
    {
        if (PlatformView == null || VirtualView == null)
            return;

        if (string.IsNullOrWhiteSpace(VirtualView.Source))
            return;

        var uri = Uri.Parse(VirtualView.Source);
        PlatformView.SetVideoURI(uri);
        PlatformView.SetZOrderOnTop(false);
        PlatformView.SetZOrderMediaOverlay(false);

        if (VirtualView.ShouldAutoPlay)
            PlatformView.Start();
    }

    class PreparedListener : Java.Lang.Object, MediaPlayer.IOnPreparedListener
    {
        private readonly BackgroundVideoViewHandler _handler;
        public PreparedListener(BackgroundVideoViewHandler handler)
        {
            _handler = handler;
        }

        public void OnPrepared(MediaPlayer? mp)
        {
            if (mp != null)
            {
                mp.Looping = _handler.VirtualView?.IsLooping ?? true;
                // Fill screen by matching parent; VideoView will letterbox; keep as-is
                mp.SetVolume(0f, 0f);
            }
        }
    }
}
#endif
