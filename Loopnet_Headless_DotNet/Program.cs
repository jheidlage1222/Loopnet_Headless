using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support;
using OpenQA.Selenium;

namespace Loopnet_Headless_DotNet
{
    class Program
    {
        static PhantomJSDriver driver = new PhantomJSDriver();
        static TimeSpan implicitWait = new TimeSpan(0, 0, 30);
        static TimeSpan pageLoadWait = new TimeSpan(0, 0, 60);
        static TimeSpan shortWait = new TimeSpan(0, 0, 5);
        static string outputMessage = "";
        /// <summary>
        /// Our objective from here is to get the json data in the source and save it.  We'll pull it apart later.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hit enter to start the magic.....");
                Console.ReadLine();
                string uid = "jennifer.leblanc@colliers.com", pwd = "leblanc2016";
                //
                PhantomJSOptions options = new PhantomJSOptions();
                //
                driver.Manage().Timeouts().ImplicitWait = implicitWait;
                driver.Manage().Timeouts().PageLoad = pageLoadWait;
                //I can't really say what this might do but fuck it, why not?
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl("http://www.loopnet.com/xNet/MainSite/User/customlogin.aspx?LinkCode=31824");
                //var bigAssTextBox = driver.FindElementByName("geography");
                //Console.WriteLine("This element has this for a class value: " + bigAssTextBox.GetAttribute("class"));
                //login
                driver.FindElement(By.Name("ctlLogin$LogonEmail")).SendKeys(uid);
                driver.FindElement(By.Name("ctlLogin$LogonPassword")).SendKeys(pwd);
                driver.FindElement(By.Id("ctlLogin_btnLogon")).Click();
                //Go to the searches page
                //
                driver.Navigate().GoToUrl("http://www.loopnet.com/xNet/MainSite/Listing/SavedSearches/MySavedSearches_FSFL.aspx?LinkCode=29400");
                //
                //Get the search names first, then get their urls
                //
                var submarketNamesCollection = driver.FindElement(By.ClassName("savedSearchContainer")).FindElements(By.XPath("./tbody/tr/td[2]"));
                var searchLinkElements = driver.FindElementsByXPath("//*[@id='form1']/div[5]/div/div/table/tbody/tr/td[1]/div/a[1]");
                //Spin up a collection to hold our data from here on out
                //
                List<BaseSearch> recoveredSearches = new List<BaseSearch>();
                if(submarketNamesCollection.Count != searchLinkElements.Count)
                    throw new Exception($"Submarket/Search names count: {submarketNamesCollection.Count}. Doesn't equal recovered link elements count: {searchLinkElements.Count}");
                for (int i = 0; i < submarketNamesCollection.Count; i++)
                {
                    recoveredSearches.Add(new BaseSearch() { Name = submarketNamesCollection[i].Text, BaseResultsURL = searchLinkElements[i].GetAttribute("href") });
                }
                //Iterate through the results and do your thing
                for (int searchIndex = 0; searchIndex < recoveredSearches.Count; searchIndex++)
                {
                    var currentSearch = recoveredSearches[searchIndex];
                    driver.Navigate().GoToUrl(currentSearch.BaseResultsURL);
                    //Property name is in the title attribute of these link elements
                    var propertyNamesList = driver.FindElements(By.XPath("//*[@id='placardSec']//h5[@class = 'listing-address']/a")).Select(x => x.GetAttribute("title")).ToList<string>() ;
                    //Let's get the building class since they need that.  May also need broker info.
                    var possibleBldgClasses = driver.FindElements(By.XPath("//*[@id='placardSec']/div[2]/div/article/div[1]/section[2]/div[1]/ul/li[3]/i")).Select(x => x.Text.Trim()).ToList<string>();
                    //Make sure the classes list and names list are 1 to 1
                    if (propertyNamesList.Count != possibleBldgClasses.Count)
                        throw new Exception($"The property names list count: {propertyNamesList.Count} does not match the Bldg Class candidate list count: {possibleBldgClasses.Count}");
                    for (int tempIndex = 0; tempIndex < propertyNamesList.Count; tempIndex++)
                    {
                        currentSearch.Listings.Add(new Listing()
                        {
                            PropertyName = propertyNamesList[tempIndex],
                            BldgClass = char.IsLetter(possibleBldgClasses[tempIndex][0]) ? possibleBldgClasses[tempIndex] : "N/A"
                        });
                    }
                    //Broker info.  Deal with that later.
                    //Click the create reports button
                    driver.FindElement(By.ClassName(".button.primary.punchout.inverted.create-reports.advanced")).Click();
                    //Select all reports
                    bool firstTry = true;
                    bool lastPage = false;
                    while (!lastPage)
                    {
                        //We're already on the page for the first group we need to select, so we don't go to the next one on the first go around
                        //
                        if (!firstTry)
                        {
                            //firstTry = false;
                            FlipDriverTimeout(true);
                            var nextPageLinkContainer = driver.FindElements(By.CssSelector("a.caret-right-large"));
                            FlipDriverTimeout(false);
                            if (nextPageLinkContainer?.Count > 0)
                                nextPageLinkContainer[0].Click();
                            else
                                lastPage = true;
                        }
                        firstTry = false;
                        //Select all the elements then circle around to the next page and repeat.
                        driver.FindElement(By.XPath("//button[text()='Select all']")).Click();
                    }
                    //Onward to our report. Click the big red generate reports button.
                    driver.FindElement(By.XPath("//button[text()='Generate Reports']")).Click();
                    //
                    driver.FindElement(By.XPath(""));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Explosion: " + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
            }
            finally
            {
                driver.Close();
                Console.WriteLine("Tear down complete, strike [ENTER] to exit.");
            }

        }
        static void FlipDriverTimeout(bool engageShortWait)
        {
            if (engageShortWait)
            {
                driver.Manage().Timeouts().ImplicitWait = shortWait;
                driver.Manage().Timeouts().PageLoad = shortWait;
            }
            else
            {
                driver.Manage().Timeouts().ImplicitWait = implicitWait;
                driver.Manage().Timeouts().PageLoad = pageLoadWait;
            }
        }
    }
}
