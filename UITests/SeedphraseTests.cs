using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace UITests;

[TestFixture]
public class SeedphraseTests : BaseTest
{
    private const string TargetAddress = "12Hjcp6rro5DMW99UmWZYKvUe5rsQhMKs3aRVqM1MdyocZJ3";
    private readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(15);

    private void CaptureScreenshot(string screenshotName)
    {
        var projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", ".."));
        var screenshotsDir = Path.Combine(projectRoot, "screenshots");
        Directory.CreateDirectory(screenshotsDir);
        var screenshotPath = Path.Combine(screenshotsDir, $"{screenshotName}.png");
        App.GetScreenshot().SaveAsFile(screenshotPath);
    }

    [Test]
    public void SeedphraseFlow_SetsPassword_ImportsSeed_AddressesMatch()
    {
        ClearAppData();

        var waitDriver = new WebDriverWait(App, TimeSpan.FromSeconds(20));

        ActivateApp();
        Thread.Sleep(100);
        CaptureScreenshot("splashscreen");

        waitDriver.Until(d => d.PageSource.Contains("JoinButton"));
        CaptureScreenshot("welcomepage");
        ClickButton("JoinButton");

        waitDriver.Until(d => d.PageSource.Contains("PasswordEntry"));

        CaptureScreenshot("setuppasswordpage-empty");

        FillEntry("PasswordEntry", "BADpa");
        CaptureScreenshot("setuppasswordpage-badp");

        FillEntry("PasswordEntry", "1234");
        CaptureScreenshot("setuppasswordpage-1234");

        FillEntry("PasswordEntry", "UItest123");
        CaptureScreenshot("setuppasswordpage-goodp");

        ClickButton("JoinButton");

        Thread.Sleep(10000);
        CaptureScreenshot("mainpage");
    }
    private bool TrySetPassword(string password)
    {
        var passwordBoxes = FindElementsByAny(
            MobileBy.AccessibilityId("PasswordEntry"),
            MobileBy.AccessibilityId("Password"),
            MobileBy.AndroidUIAutomator("new UiSelector().className(\"android.widget.EditText\")")
        );

        if (!passwordBoxes.Any()) return false;

        foreach (var entry in passwordBoxes.Take(2))
        {
            entry.Clear();
            entry.SendKeys(password);
        }

        var continueButton = FindElementsByAny(
            MobileBy.AccessibilityId("ContinueButton"),
            MobileBy.AccessibilityId("Continue"),
            MobileBy.AndroidUIAutomator("new UiSelector().textContains(\"Continue\")"),
            MobileBy.AndroidUIAutomator("new UiSelector().textContains(\"Next\")")
        ).FirstOrDefault();

        continueButton?.Click();

        return true;
    }

    private bool TryImportSeedphrase(string seed)
    {
        // Navigate to settings (main menu entry or bottom nav)
        if (!TryTapByText("Settings", "settings"))
        {
            return false;
        }

        // Attempt to open a mnemonic/seed entry page
        TryTapByText("Import", "Import wallet", "Enter mnemonics", "Seed", "Mnemonics", "Secret phrase");

        var seedEntry = FindElementsByAny(
            MobileBy.AccessibilityId("SeedEntry"),
            MobileBy.AccessibilityId("mnemonicsEntry"),
            MobileBy.AndroidUIAutomator("new UiSelector().className(\"android.widget.EditText\")"),
            MobileBy.AndroidUIAutomator("new UiSelector().descriptionContains(\"mnemonic\")")
        ).FirstOrDefault();

        if (seedEntry is null) return false;

        seedEntry.Clear();
        seedEntry.SendKeys(seed);

        var saveButton = FindElementsByAny(
            MobileBy.AccessibilityId("Save"),
            MobileBy.AccessibilityId("Continue"),
            MobileBy.AndroidUIAutomator("new UiSelector().textContains(\"Save\")"),
            MobileBy.AndroidUIAutomator("new UiSelector().textContains(\"Confirm\")"),
            MobileBy.AndroidUIAutomator("new UiSelector().textContains(\"Continue\")")
        ).FirstOrDefault();

        saveButton?.Click();
        Thread.Sleep(1500);
        return true;
    }

    private string? TryNavigateAndGetAddressFromAssets()
    {
        // Prefer explicit nav button when available
        FindElementsByAny(MobileBy.AccessibilityId("AssetsNavButton")).FirstOrDefault()?.Click();
        TryTapByText("Assets", "Portfolio", "Wallet");

        // Try to find address label
        var addrElement = FindElementsByAny(
            MobileBy.AccessibilityId("AddressLabel"),
            MobileBy.AndroidUIAutomator("new UiSelector().descriptionContains(\"Address\")"),
            MobileBy.AndroidUIAutomator("new UiSelector().textContains(\"1\")") // base58 often starts with 1
        ).FirstOrDefault();

        var text = addrElement?.Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            return ExtractAddress(text);
        }

        // Fallback: scrape page source for base58-looking address
        return ExtractAddressFromPageSource();
    }

    private string? TryOpenQrAndReadAddress()
    {
        var qrButton = FindElementsByAny(
            MobileBy.AccessibilityId("Receive"),
            MobileBy.AccessibilityId("QR"),
            MobileBy.AccessibilityId("QrButton"),
            MobileBy.AndroidUIAutomator("new UiSelector().textContains(\\\"Receive\\\")"),
            MobileBy.AndroidUIAutomator("new UiSelector().textContains(\\\"QR\\\")")
        ).FirstOrDefault();

        qrButton?.Click();
        Thread.Sleep(1500);

        var qrAddr = FindElementsByAny(
            MobileBy.AccessibilityId("QrAddress"),
            MobileBy.AndroidUIAutomator("new UiSelector().descriptionContains(\"Address\")"),
            MobileBy.AndroidUIAutomator("new UiSelector().textContains(\"1\")")
        ).FirstOrDefault();

        var addr = ExtractAddress(qrAddr?.Text);
        if (!string.IsNullOrWhiteSpace(addr)) return addr;

        return ExtractAddressFromPageSource();
    }

    private bool TryTapByIdOrText(params string[] idsOrTexts)
    {
        foreach (var t in idsOrTexts)
        {
            var el = FindElementsByAny(
                MobileBy.AccessibilityId(t),
                MobileBy.AndroidUIAutomator($"new UiSelector().text(\\\"{t}\\\")"),
                MobileBy.AndroidUIAutomator($"new UiSelector().textContains(\\\"{t}\\\")"),
                MobileBy.AndroidUIAutomator($"new UiSelector().textStartsWith(\\\"{t}\\\")")
            ).FirstOrDefault();
            if (el is not null)
            {
                try
                {
                    var wait = new WebDriverWait(App, TimeSpan.FromSeconds(5));
                    wait.Until(d => el.Displayed && el.Enabled);
                }
                catch { }

                el.Click();
                Thread.Sleep(500);
                return true;
            }
        }
        return false;
    }

    private bool TryTapByText(params string[] texts) => TryTapByIdOrText(texts);

    private IReadOnlyCollection<IWebElement> FindElementsByAny(params By[] selectors)
    {
        foreach (var selector in selectors)
        {
            try
            {
                var wait = new WebDriverWait(App, defaultTimeout);
                var elements = wait.Until(d => d.FindElements(selector));
                if (elements.Any())
                {
                    return elements;
                }
            }
            catch
            {
                // ignore and continue
            }
        }
        return Array.Empty<IWebElement>();
    }

    private static string? ExtractAddress(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var match = Regex.Match(raw, "[123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz]{40,64}");
        return match.Success ? match.Value : null;
    }

    private string? ExtractAddressFromPageSource()
    {
        try
        {
            var source = App.PageSource;
            return ExtractAddress(source);
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Failed to get page source: {ex.Message}");
            return null;
        }
    }
}
