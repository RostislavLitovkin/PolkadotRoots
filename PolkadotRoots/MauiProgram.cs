using CommunityToolkit.Maui;
using Microsoft.Maui.Handlers;
using PlutoFramework;
using PolkadotRoots.Controls;

#if ANDROID26_0_OR_GREATER
using Android.Webkit;
using PlutoFramework.Platforms.Android;
using PolkadotRoots.Platforms.Android.Handlers;
#endif

#if IOS16_0_OR_GREATER

using WebKit;
using PolkadotRoots.Platforms.iOS.Handlers;

#endif

namespace PolkadotRoots
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

#if ANDROID26_0_OR_GREATER
            AndroidNotificationHelper.AppIcon = CommunityToolkit.Maui.Core.Resource.Drawable.resourceappicon;
            AndroidNotificationHelper.MainActivityType = typeof(Platforms.Android.MainActivity);
#endif

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UsePlutoFramework();

            builder.ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler(typeof(BackgroundVideoView), typeof(BackgroundVideoViewHandler));
                WebViewHandler.Mapper.AppendToMapping("VideoPlayback", (handler, view) =>
                {
                    var webView = handler.PlatformView;
                    var settings = webView.Settings;
                    settings.JavaScriptEnabled = true;
                    settings.MediaPlaybackRequiresUserGesture = false;
                    settings.AllowFileAccess = true;
                    settings.AllowContentAccess = true;
                    settings.AllowFileAccessFromFileURLs = true;
                    settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
                });
#endif
#if IOS
                handlers.AddHandler(typeof(BackgroundVideoView), typeof(BackgroundVideoViewHandler));
                WebViewHandler.Mapper.AppendToMapping("VideoPlayback", (handler, view) =>
                {
                    if (handler.PlatformView is WKWebView wv)
                    {
                        wv.Configuration.AllowsInlineMediaPlayback = true;
                        wv.Configuration.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
                    }
                });
#endif
            });

            return builder.Build();
        }
    }
}
