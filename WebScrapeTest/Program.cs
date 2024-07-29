// See https://aka.ms/new-console-template for more information
using HtmlAgilityPack;
using PuppeteerSharp;

// Edit Date
DateTime date = DateTime.Parse("Jul 25, 2023");
// Number of deployment days
var numDays = 5;

using var browserFetcher = new BrowserFetcher();
await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = true
});
var options = new NavigationOptions();
options.WaitUntil = new[] { WaitUntilNavigation.Load };
var page = await browser.NewPageAsync();
await page.GoToAsync("https://www4.awsdms.net/sendy/login");
string content = await page.GetContentAsync();

// log in
await page.WaitForSelectorAsync(".container-fluid");
await page.TypeAsync("#email", "support@teamdms.com");
await page.TypeAsync("#wrapper > form > input:nth-child(6)", "sendymail");
await page.ClickAsync("#wrapper > form > button");
content = await page.GetContentAsync();

// click Credit Ninja brand
await page.ClickAsync("#\\31 3 > td:nth-child(2) > p > a");
content = await page.GetContentAsync();

var url = "";
var p = 10;
url = $"http://www4.awsdms.net/sendy/app?i=13&p={p}";
for (int i = 0; i < numDays; i++)
{
    if (i == numDays - 1)
        date = date.AddDays(2);
    // select right page
    if (p <= 0) url = "http://www4.awsdms.net/sendy/app?i=13";
    //if (i == numDays - 1) url = "http://www4.awsdms.net/sendy/app?i=13";
    //else
    //{
    //    url = $"http://www4.awsdms.net/sendy/app?i=13&p={7}";
    //}
    // 10 campaigns each day
    for (int j = 0; j < 10; j++)
    {
        // navigate to page
        //if (i == 0) j = 4;
        await page.GoToAsync(url);
        
        // generate campaign name
        var campaign_name = $"_{date.ToString("yyyyMMdd")}_CN_CL59ER1_{j + 1}_of_10_A";
        // go to campaign page
        try
        {
            var anchorAttempt = await page.XPathAsync($"//a[contains(text(), '{campaign_name}')]");
            JSHandle hrefAttempt = (JSHandle)await anchorAttempt[0].GetPropertyAsync("href");

        }
        catch (Exception e)
        {
            p--;
            url = $"http://www4.awsdms.net/sendy/app?i=13&p={p}";
            if (p <= 0) url = "http://www4.awsdms.net/sendy/app?i=13";
            await page.GoToAsync(url);
        }
        var anchor = await page.XPathAsync($"//a[contains(text(), '{campaign_name}')]");
        JSHandle href = (JSHandle)await anchor[0].GetPropertyAsync("href");
        await page.GoToAsync(href.RemoteObject.Value.ToString());

        // for testing, view page contents
        content = await page.GetContentAsync();

        // select correct list to deploy (campaign and list share the same name)
        var option = await page.XPathAsync($"//select[@id=\"email_list\"]/*/option[contains(text(), \"{campaign_name}\")]");
        await option[0].ClickAsync();

        // schedule
        await page.ClickAsync("#send-later-btn");
        content = await page.GetContentAsync();

        // calcuate time and am or pm
        var time = j + 8;
        var amPm = 1;
        if (time >= 12)
        {
            amPm = 2;
        }
        if (time > 12)
        {
            time = time - 12;
        }
        var jsExpression = $@"async () => {{
document.querySelector('#hour option:nth-child({time})').selected = true;
document.querySelector('#ampm > option:nth-child({amPm})').selected = true;
document.querySelector('#datepicker').value= '{date.ToString("MMM dd, yyyy")}';


}}";
        await page.EvaluateFunctionAsync(jsExpression);
        content = await page.GetContentAsync();

        await page.ClickAsync("#schedule-btn");

    }
    // increment day
    date = date.AddDays(1);

}

return;
