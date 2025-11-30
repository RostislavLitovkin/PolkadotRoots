using OpenQA.Selenium.Appium.Service;
using System;
using System.IO;

namespace UITests;

public static class AppiumServerHelper
{
    private static AppiumLocalService? _appiumLocalService;

    public const string DefaultHostAddress = "127.0.0.1";
    public const int DefaultHostPort = 4723;

    public static void StartAppiumLocalServer(string host = DefaultHostAddress,
        int port = DefaultHostPort,
        TimeSpan? startupTimeout = null)
    {
        if (_appiumLocalService is not null)
        {
            return;
        }

        var appiumJsPath = TryGetAppiumJsPath();

        var builder = new AppiumServiceBuilder()
            .WithIPAddress(host)
            .UsingPort(port)
            .WithAppiumJS(new FileInfo(appiumJsPath))
            .WithStartUpTimeOut(TimeSpan.FromSeconds(20));

        // Base path customization not supported via current AppiumServiceBuilder API in this project version.
        // If needed, configure the client to use the default base path.

        // Startup timeout configuration not supported via current AppiumServiceBuilder API.

        // Try to locate Appium's main.js explicitly to avoid default path issues

        // Start the server with the builder
        _appiumLocalService = builder.Build();
        _appiumLocalService.Start();
    }

    public static void DisposeAppiumLocalServer()
    {
        _appiumLocalService?.Dispose();
    }

    private static string TryGetAppiumJsPath()
    {
        // 1) Explicit override via environment variable
        var envPath = Environment.GetEnvironmentVariable("APPIUM_JS_PATH");
        Console.WriteLine("APPIUM_JS_PATH: " + envPath);
        if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath))
        {
            Console.WriteLine("Got APPIUM_JS_PATH");
            return envPath;
        }

        // Locate the NPM root directory: npm root -g
        // Then set it: setx "C:\Users\<yourUser>\AppData\Roaming\npm\node_modules\appium\build\lib\main.js"

        throw new Exception("Cannot locate Appium JS path. Please set the APPIUM_JS_PATH environment variable to point to Appium's main.js file.");
    }
}