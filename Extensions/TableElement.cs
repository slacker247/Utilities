using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Extensions
{
    public class TableElement
    {
        protected List<List<IWebElement>> Table = new List<List<IWebElement>>();

        public int Count
        {
            get
            {
                return Table.Count;
            }
        }

        protected void loadTable(List<IWebElement> rowElms)
        {
            int rowCount = 0;
            foreach (var row in rowElms)
            {
                Table.Add(new List<IWebElement>());
                var cols = row.FindElements(By.XPath("td")).ToList();
                foreach (var col in cols)
                {
                    Table.Last().Add(col);
                }
                rowCount++;
            }
        }
        public TableElement(IWebDriver driver, IWebElement tableElm)
        {
            List<IWebElement> rowElms = tableElm.FindElements(By.XPath("//tbody/tr")).ToList();
            loadTable(rowElms);
        }

        public TableElement(IWebDriver driver, String className, String dud)
        {
            IWebElement tableElm = driver.FindElement(By.ClassName(className));
            if (tableElm == null)
                throw new Exception("Failed to find table element: " + className);
            List<IWebElement> rowElms = tableElm.FindElements(By.XPath("//table[contains(@class, '" + className + "')]/tbody/tr")).ToList();

            loadTable(rowElms);
        }

        public TableElement(IWebDriver driver, String tableId)
        {
            IWebElement tableElm = driver.FindElement(By.Id(tableId));
            if (tableElm == null)
                throw new Exception("Failed to find table element: " + tableId);
            List<IWebElement> rowElms = tableElm.FindElements(By.XPath("id('" + tableId + "')/tbody/tr")).ToList();

            loadTable(rowElms);
        }

        public String getText(int row, int col)
        {
            String str = "";
            if (row > -1 && row < Table.Count)
                if (col > -1 && col < Table[row].Count)
                    str = Table[row][col].Text;
            return str;
        }

        public IWebElement getElement(int row, int col)
        {
            IWebElement elm = null;
            if (row > -1 && row < Table.Count)
                if (col > -1 && col < Table[row].Count)
                    elm = Table[row][col];
            return elm;
        }

        public void clickElement(int row, int col)
        {
            if (row > -1 && row < Table.Count)
                if (col > -1 && col < Table[row].Count)
                    Table[row][col].Click();
        }
    }
}
