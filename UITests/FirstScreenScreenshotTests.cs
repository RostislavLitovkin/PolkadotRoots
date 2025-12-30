using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
// Local Appium service builder removed; we connect to an already running server.
using OpenQA.Selenium.Remote;

namespace UITests;

[TestFixture]
public class FirstScreenScreenshotTests : BaseTest
{
    [Test]
    public void AppLaunches()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", ".."));
        var screenshotsDir = Path.Combine(projectRoot, "screenshots");
        Directory.CreateDirectory(screenshotsDir);
        var screenshotPath = Path.Combine(screenshotsDir, $"{nameof(AppLaunches)}.png");
        App.GetScreenshot().SaveAsFile(screenshotPath);
    }
}
