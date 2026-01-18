using OpenQA.Selenium;
using Xunit;

namespace SuperSee.SeleniumTests;

public class LoginSeleniumTests : SeleniumTestBase
{
    [Fact]
    public void Coordinator_CanLoginSuccessfully()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/Login.html");
        
        var usernameInput = WaitUntilElementExists(By.Id("username"));
        var passwordInput = Driver.FindElement(By.Id("password"));
        var loginButton = Driver.FindElement(By.TagName("button"));
        
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", usernameInput);
        
        usernameInput.SendKeys("Khaldoon");
        passwordInput.SendKeys("Has00000");
        loginButton.Click();
        
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.Contains("Dashboord.html"));

        Assert.Contains("Dashboord.html", Driver.Url);
        var welcomeMsg = WaitUntilElementExists(By.Id("welcomeMsg"));
        Assert.Contains("خلدون", welcomeMsg.Text);
    }

    [Fact]
    public void InvalidLogin_ShowsErrorMessage()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/Login.html");
        
        var usernameInput = WaitUntilElementExists(By.Id("username"));
        var passwordInput = Driver.FindElement(By.Id("password"));
        var loginButton = Driver.FindElement(By.TagName("button"));

        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", usernameInput);

        usernameInput.SendKeys("WrongUser");
        passwordInput.SendKeys("WrongPass");
        loginButton.Click();
        
        var errorMsg = WaitUntilElementExists(By.Id("errorMsg"));
        var waitError = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(5));
        waitError.Until(d => !string.IsNullOrEmpty(d.FindElement(By.Id("errorMsg")).Text));
        
        Assert.False(string.IsNullOrEmpty(errorMsg.Text));
    }
}
