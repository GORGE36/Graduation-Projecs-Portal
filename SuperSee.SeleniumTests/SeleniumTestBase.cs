using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Xunit;
using System;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;

namespace SuperSee.SeleniumTests;

public class SeleniumTestBase : IDisposable
{
    protected readonly IWebDriver Driver;
    protected readonly string BaseUrl = "http://localhost:5000";
    private Process? _appProcess;

    public SeleniumTestBase()
    { 
        StartApp();

        var options = new ChromeOptions();
        options.AddArgument("--headless"); 
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1920,1080");
        
        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    private void StartApp()
    {
        try 
        {
            using var client = new System.Net.Http.HttpClient();
            var response = client.GetAsync(BaseUrl).Result;
            if (response.IsSuccessStatusCode) return;
        }
        catch { }
        
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SuperSee"));
        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project {projectPath} --urls \"{BaseUrl}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        _appProcess.Start();
        
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(30))
        {
            try 
            {
                using var client = new System.Net.Http.HttpClient();
                var response = client.GetAsync(BaseUrl).Result;
                if (response.IsSuccessStatusCode) break;
            }
            catch { }
            Thread.Sleep(1000);
        }
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
        
        if (_appProcess != null && !_appProcess.HasExited)
        {
            _appProcess.Kill();
            _appProcess.Dispose();
        }
    }

    protected IWebElement WaitUntilElementExists(By elementLocator, int timeout = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeout));
        return wait.Until(ExpectedConditions.ElementExists(elementLocator));
    }
}
