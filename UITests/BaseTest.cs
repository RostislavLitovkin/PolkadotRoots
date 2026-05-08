using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace UITests;

public abstract class BaseTest
{
    public const string PackageName = "app.plutolabs.community";
    protected AppiumDriver App => AppiumSetup.App;
    private readonly TimeSpan _waitTimeout = TimeSpan.FromSeconds(20);

    public void ActivateApp()
    {
        App.ActivateApp(PackageName);
    }

    public void ClearAppData()
    {
        App.ExecuteScript("mobile: shell", new Dictionary<string, object>
        {
            {"command", "pm"},
            {"args", new[] {"clear", PackageName}}
        });
    }

    public void ClickButton(string buttonAutomationId)
    {
        var button = App.FindElement(MobileBy.AccessibilityId(buttonAutomationId));
        button.Click();
    }

    public void FillEntry(string entryAutomationId, string text)
    {
        var wait = new WebDriverWait(App, _waitTimeout);
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
        var entry = wait.Until(d => FindFirst(d,
            MobileBy.AccessibilityId(entryAutomationId),
            MobileBy.AndroidUIAutomator($"new UiSelector().descriptionContains(\"{entryAutomationId}\")"),
            MobileBy.AndroidUIAutomator($"new UiSelector().textContains(\"{entryAutomationId}\")"),
            MobileBy.AndroidUIAutomator("new UiSelector().className(\\\"android.widget.EditText\\\")")));
        if (entry is null) throw new NoSuchElementException($"Entry with AutomationId '{entryAutomationId}' not found within {_waitTimeout.TotalSeconds} seconds");
        entry.Clear();
        entry.SendKeys(text);
    }

    private static IWebElement? FindFirst(ISearchContext context, params By[] selectors)
    {
        foreach (var selector in selectors)
        {
            try
            {
                var found = context.FindElements(selector).FirstOrDefault();
                if (found is not null)
                {
                    return found;
                }
            }
            catch
            {
                // ignore and try next
            }
        }
        return null;
    }
}