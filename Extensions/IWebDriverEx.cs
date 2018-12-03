using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium;

namespace Utilities.Extensions
{
    public class IWebDriverEx
    {
        public static void innerHtml(IWebDriver driver, IWebElement elm, String html)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript(
                "var ele=arguments[0]; ele.innerHTML = '" + html + "';", elm);
        }

        public static void waitVisible(IWebElement elm, int iTimeout = 15)
        {
            DateTime start = DateTime.Now;
            TimeSpan timeout = TimeSpan.FromSeconds(iTimeout);
            bool visibile = false;
            while (!visibile)
            {
                if (start + timeout < DateTime.Now)
                    throw new TimeoutException();
                try
                {
                    visibile = elm.Displayed;
                }
                catch (Exception ex)
                { }
            }
        }

        public static void ScrollElementIntoView(IWebDriver driver, IWebElement elm)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("" +
                "Element.prototype.documentOffsetTop = function () {" +
                "return this.offsetTop + (this.offsetParent ? this.offsetParent.documentOffsetTop() : 0);" +
                "};" +
                "var top = arguments[0].documentOffsetTop() - (window.innerHeight/2);" +
                "window.scrollTo(0, top);" +
                "var top = arguments[0].documentOffset() - (window.innerHeight/2);" +
                "", elm);
            Thread.Sleep(1000);
        }

        public static void ClickElement(IWebDriver driver, String name)
        {
            ClickElement(driver, By.Id(name));
        }

        public static void ClickElement(IWebDriver driver, By b)
        {
            IWebElement elm = findElement(driver, b);
            if (elm != null)
            {
                ScrollElementIntoView(driver, elm);
                elm.Click();
            }
        }
        public static void clickLink(IWebDriver driver, String href)
        {
            var link = IWebDriverEx.findElement(driver, By.XPath("//a[@href='" + href + "']"));
            try
            {
                ScrollElementIntoView(driver, link);
                link.Click();
            }
            catch (WebDriverException ex)
            {
                if (!ex.Message.Contains("time out"))
                    throw;
            }
        }

        public static IWebElement findElement(IWebDriver driver, By b, int iTimeout = 15)
        {
            IWebElement elm = null;
            DateTime start = DateTime.Now;
            TimeSpan timeout = TimeSpan.FromSeconds(iTimeout);
            bool find = false;
            while (!find)
            {
                if (start + timeout < DateTime.Now)
                    break;
                try
                {
                    elm = driver.FindElement(b);
                    if (elm != null)
                        find = true;
                }
                catch (Exception ex)
                { }
            }
            return elm;
        }

        protected static ReadOnlyCollection<IWebElement> findElements(IWebDriver driver, By b)
        {
            ReadOnlyCollection<IWebElement> elms = null;
            DateTime start = DateTime.Now;
            TimeSpan timeout = TimeSpan.FromSeconds(15);
            bool find = false;
            while (!find)
            {
                if (start + timeout < DateTime.Now)
                    throw new TimeoutException();
                try
                {
                    elms = driver.FindElements(b);
                    if (elms != null && elms.Count > 0)
                        find = true;
                }
                catch (Exception ex)
                { }
            }
            return elms;
        }
    }
}
