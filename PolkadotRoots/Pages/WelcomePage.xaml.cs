using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls; // HtmlWebViewSource
using PolkadotRoots.Controls;

namespace PolkadotRoots.Pages;

public partial class WelcomePage : ContentPage
{
    private bool _videoInitialized;

	public WelcomePage()
	{
        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);

        InitializeComponent();

        BindingContext = new WelcomePageViewModel();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_videoInitialized)
            return;

        _videoInitialized = true;

        try
        {
            var videoName = "ethpraguevertical.mp4";

            var cacheDir = FileSystem.CacheDirectory;
            var localVideoPath = Path.Combine(cacheDir, videoName);

            if (!File.Exists(localVideoPath))
            {
                try
                {
                    using var input = await FileSystem.OpenAppPackageFileAsync(videoName);
                    using var output = File.Create(localVideoPath);
                    await input.CopyToAsync(output);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"WelcomePage: Video asset not found in app package: {ex.Message}");
                    return;
                }
            }

            var fileUri = new Uri(localVideoPath).AbsoluteUri;

            // Fallback to WebView background if present
            var webView = this.FindByName<WebView>("BackgroundWebView");
            if (webView != null)
            {
                // Use a proper base URL to allow relative path
                var baseUrl = new Uri(cacheDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar).AbsoluteUri;
                var html = @"<!DOCTYPE html>
<html>
<head>
<meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no' />
<style>
    html, body { margin:0; padding:0; background:black; height:100%; overflow:hidden; }
    .wrapper { position: fixed; inset: 0; width: 100vw; height: 100vh; overflow: hidden; background: black; }
    video.bg { position:absolute; inset:0; width:100%; height:100%; object-fit:cover; }
</style>
</head>
<body>
<div class='wrapper'>
  <video id='bg' class='bg' src='ethpraguevertical.mp4' autoplay muted loop playsinline webkit-playsinline preload='auto'></video>
</div>
<script>
  (function() {
    const tryPlay = () => {
      const v = document.getElementById('bg');
      if (!v) return;
      v.muted = true;
      const p = v.play();
      if (p && p.catch) p.catch(() => setTimeout(tryPlay, 250));
    };
    if (document.readyState === 'complete' || document.readyState === 'interactive') {
      tryPlay();
    } else {
      document.addEventListener('DOMContentLoaded', tryPlay);
    }
  })();
</script>
</body>
</html>";

                var source = new HtmlWebViewSource { Html = html, BaseUrl = baseUrl };
                webView.BackgroundColor = Colors.Black;
                webView.Source = source;
                webView.InputTransparent = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WelcomePage: Failed to initialize background video: {ex}");
        }
    }
}