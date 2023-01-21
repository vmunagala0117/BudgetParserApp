using CsvHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using Microsoft.Office.Interop.Excel;

namespace BudgetParserApp
{
    public partial class BudgetParserWinApp : Form
    {
        public BudgetParserWinApp()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                txtFilePath.Text = openFileDialog.FileName;
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            using (var csv = new CsvReader(File.OpenText(txtFilePath.Text), CultureInfo.InvariantCulture))
            {
                //csv.Configuration.HasHeaderRecord = true;
                csv.Context.RegisterClassMap<BudgetClassMap>();

                DateTime sDate = DateTime.Parse(startDate.Text);
                DateTime eDate = DateTime.Parse(endDate.Text);

                // *********************    1. Extract  ***********************************//
                //Get all records b/w the time range
                List<Budget> budgetRecords = csv.GetRecords<Budget>().Where(f => f.Date >= sDate && f.Date <= eDate).OrderBy(a => a.Amount).ToList();

                //  ********************    2. Transform    *******************************//
                //Perform some transformations
                budgetRecords = Transform(budgetRecords);
                //Perform Cleanup of Budget Records
                budgetRecords = DoCleanup(budgetRecords);

                //  ******************  3. Process  ******************************//
                List<BudgetReport> budgetReportList = ProcessBudget(budgetRecords);

                //  ******************  3. Generate Report  ******************************//
                Logger.LogMessagetoExcelFile(budgetReportList);
            }
        }

        private List<Budget> DoCleanup(List<Budget> budgetEntries)
        {
            CleanupEntries(budgetEntries, "Transfer", accountName: "Checking-3256", description: "Morgan Stanley");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Checking", description: "Money Market");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Checking", description: "Transfer to Savings");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Checking", description: "Transfer From Savings");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Wells Fargo", description: "TRANSFER CREDIT FROM");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Wells Fargo", description: "TRANSFER CREDIT TO");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Wells Fargo", description: "RECURRING TRANSFER TO CHITIKIREDDI V");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Wells Fargo", description: "RECURRING TRANSFER FROM CHITIKIREDDI V");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Costco Anywhere", description: "AUTOPAY AUTO-PMT");

            CleanupEntries(budgetEntries, "Transfer", accountName: "Savings Plus Account");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Day-to-Day Savings");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Accelerate Savings");
            CleanupEntries(budgetEntries, "Transfer", accountName: "SAVINGS Account");
            CleanupEntries(budgetEntries, "Income", accountName: "SAVINGS Account");
            CleanupEntries(budgetEntries, "Financial", accountName: "SAVINGS Account");
            CleanupEntries(budgetEntries, "Transfer", accountName: "Primary Account");

            CleanupEntries(budgetEntries, "Transfer", description: "ONLINE TRANSFER TO CAPITAL ONE");
            CleanupEntries(budgetEntries, "Transfer", description: "ZELLE TO MUNAGALA VAMSI");
            CleanupEntries(budgetEntries, "Transfer", description: "ZELLE FROM VAMSI MUNAGALA");

            CleanupEntries(budgetEntries, "Transfer", description: "Zelle Credit");
            CleanupEntries(budgetEntries, "Transfer", description: "SAVE AS YOU GO TRANSFER");
            CleanupEntries(budgetEntries, "Transfer", description: "ONLINE TRANSFER TO CHITIKIREDDI");
            CleanupEntries(budgetEntries, "Transfer", description: "ONLINE TRANSFER FROM CHITIKIREDDI");

            CleanupEntries(budgetEntries, "Dividend & Cap Gains");
            CleanupEntries(budgetEntries, "Investments");
            CleanupEntries(budgetEntries, "Trade Commissions");

            CleanupEntries(budgetEntries, "Credit Card Payment");

            budgetEntries = RemoveDuplicateEntries(budgetEntries, removePotentialDuplicateEntries: chkRemoveIdentifiedDuplicates.Checked);

            return budgetEntries;
        }

        private List<Budget> Transform(List<Budget> budgetEntries)
        {
            /*
                FixCategories("Shopping & Sporting Goods", budgetReportList, "Groceries", new[] { "Sam's Club", "Costco" });
                FixCategories("Transfer", budgetReportList, "Daycare", new[] { "NANCYGKELLY", "HALLIEPETER" });
                FixCategories("Transfer", budgetReportList, "Shopping & Sporting Goods", new[] { "Venmo" });
                FixCategories("Entertainment", budgetReportList, "Fun & Getaways", new[] { "TICKETSATWORK.COM" });
                FixCategories("Transfer", budgetReportList, "Mobile (ATT)", new[] { "SREELEELA" });
                FixCategories("Transfer", budgetReportList, "Food Services", new[] { "ZELLE TO S PROMILA" });
                FixCategories("Transfer", budgetReportList, "Miscellaneous", new[] { "ZELLE Debit", "#NAME?" });
                FixCategories("Restaurants", budgetReportList, "Food Services", new[] { "Food" });
                FixCategories("Transfer", budgetReportList, "Mortgage", new[] { "ZELLE TO AHUJA APARNA", "BLOSSOM REAL" });
                FixCategories("Transfer", budgetReportList, "India Investment", new[] { "RIAMONEYTRANSFER", "Xoom", "ZELLE TO KONDA KRISHNA" });
                FixCategories("Transfer", budgetReportList, "Car Payment", new[] { "- JPMorgan Chase Ext" });
                FixCategories("Transfer", budgetReportList, "Education", new[] { "MCPHS" });
                FixCategories("Fun & Getaways", budgetReportList, "Education", new[] { "WATERTOWN MA PAR" });
                FixCategories("Transfer", budgetReportList, "Daycare", new[] { "ZELLE TO GANNU RAJANI" });
                FixCategories("Groceries", budgetReportList, "Water", new[] { "DS SERVICES" });
                FixCategories("Restaurants", budgetReportList, "Water", new[] { "WATER - COFFEE DELIVERY" });
                FixCategories("Restaurants", budgetReportList, "Groceries", new[] { "CRESCENT RI" });
                FixCategories("Utilities", budgetReportList, "Mortgage", new[] { "CHARLESTON MANAGEMENT CORP" });
                FixCategories("Utilities", budgetReportList, "Daycare", new[] { "ADVENTURES PRESCHOOL" });
                FixCategories("Mortgage", budgetReportList, "Daycare", new[] { "ADVENTURES PRESCHOOL" });
                FixCategories("Mortgage", budgetReportList, "Room Rent", new[] { "Jeff", "Elan" });
                FixCategories("Credit Card Payments", budgetReportList, "Groceries", new[] { "Hello Brother Indian" });
                FixCategories("Credit Card Payments", budgetReportList, "Room Rent", new[] { "Erenterplan" });
                FixCategories("Rental Income", budgetReportList, "Paycheck Income", new[] { "DEPOSIT MADE IN A BRANCH" });
                FixCategories("Other Income", budgetReportList, "Rental Income", new[] { "RAVINDRA M BHEEM" });
                FixCategories("Other Income", budgetReportList, "Groceries", new[] { "Costco Whse" });
                FixCategories("Other Income", budgetReportList, "Paycheck Income", new[] { "INSIGHT", "BAXALTA", "TAKEDA DEVELOPME DIRECT DEP", "Check Deposit" });
                FixCategories("Service & Parts", budgetReportList, "Gas & Fuel", new[] { "Auto Clinic Inc" });
                FixCategories("Miscellaneous", budgetReportList, "Gas & Fuel", new[] { "Auto Clinic Inc", "Auto Clinic" });
                FixCategories("Miscellaneous", budgetReportList, "Restaurants", new[] { "Good To Go, Inc." });
                FixCategories("ATM Withdrawal", budgetReportList, "Car Payment", new[] { "TRANSFER TO LOAN 141" });
                
                //At last calculate Expenses from the above budget list
                CalculateTotalExpenses(budgetReportList);
                */

            //Modify Columns
            ModifyColumn(budgetEntries, "AccountName", "Checking", "Checking-3256");
            //Modify Categories
            ModifyCategories(budgetEntries, newCategory: "Vamsi Paycheck", existingCategory: "Paycheck", accountNames: new string[] { "Checking" }, descriptions: new string[] { "INSIGHT DIRECT", "ACH Electronic Credit" });
            ModifyCategories(budgetEntries, newCategory: "Vidya Paycheck", existingCategory: "Paycheck", accountNames: new string[] { "Checking" }, descriptions: new string[] { "TAKEDA DEVELOPME DIRECT DEP", "ALEXION PHARMACE DIRECT DEP" });
            ModifyCategories(budgetEntries, newCategory: "Childcare", existingCategory: "Tuition", accountNames: new string[] { "Checking" }, descriptions: new string[] { "SMART LLC" });
            ModifyCategories(budgetEntries, newCategory: "Tuition", descriptions: new string[] { "MCPHS" });
            ModifyCategories(budgetEntries, newCategory: "Childcare", descriptions: new string[] {"FULTON SCIENCE ACADEMY", "ADVENTURES PRESCHOOL", "St Peter School", "ZELLE TO BUDANG", "ZELLE TO GANNU RAJANI", "NANCYGKELLY", "HALLIEPETER" });
            ModifyCategories(budgetEntries, newCategory: "Kids", descriptions: new string[] { "British Swim School", "DIVENTURES ATLANTA" });
            ModifyCategories(budgetEntries, newCategory: "529k", accountNames: new string[] { "Wells Fargo" }, descriptions: new string[] { "Morgan Stanley" });
            ModifyCategories(budgetEntries, newCategory: "Groceries", descriptions: new string[] { "COSTCO" });
            ModifyCategories(budgetEntries, newCategory: "Groceries", accountNames: new string[] { "Costco Anywhere" } , descriptions: new string[] { "AUTOPAY" });
            ModifyCategories(budgetEntries, newCategory: "Restaurants", descriptions: new string[] { "Good To Go, Inc." });
            ModifyCategories(budgetEntries, newCategory: "Shopping", descriptions: new string[] { "Affirm", "The UPS Store", "BEST BUY", "IKEA" });
            ModifyCategories(budgetEntries, newCategory: "Gas", descriptions: new string[] { "NATIONAL GRID", "SPI*GA NATGAS" });
            ModifyCategories(budgetEntries, newCategory: "Electricity", existingCategory: "Utilities", descriptions: new string[] { "EVERSOURCE", "SAWNEE ELECTRIC" });
            ModifyCategories(budgetEntries, newCategory: "Sewer", existingCategory: "Utilities", descriptions: new string[] { "WATER & SEWER SERVICES" });
            ModifyCategories(budgetEntries, newCategory: "Gas & Fuel", descriptions: new string[] { "Auto Clinic", "COSTCO GAS" });
            ModifyCategories(budgetEntries, newCategory: "Service & Parts", descriptions: new string[] { "Car Wash", "Trad Auto Center" });
            ModifyCategories(budgetEntries, newCategory: "Gym", descriptions: new string[] { "LIFE TIME" });
            ModifyCategories(budgetEntries, newCategory: "Auto Payment", descriptions: new string[] { "MONTHLY AUTO NEW PAYMENT", "TRANSFER TO LOAN 141" });
            ModifyCategories(budgetEntries, newCategory: "Food Services", descriptions: new string[] { "RASHMI SHAH", "ZELLE TO SONIA", "ZELLE TO S PROMILA" });
            ModifyCategories(budgetEntries, newCategory: "Home Services", descriptions: new string[] { "ZELLE TO VERONICA"});
            ModifyCategories(budgetEntries, newCategory: "Others", accountNames: new string[] { "VENMO" });
            ModifyCategories(budgetEntries, newCategory: "Others", descriptions: new string[] { "SRIKANTH KAM", "SAKAEM LOGISTICS", "Zelle Debit", "RIAMONEYTRANSFER", "Xoom", "ZELLE TO KONDA KRISHNA", "CITY OF ", "VENMO", "TOWN OF WATERTOWTOWN OF WA", "SH DRAFT" });
            ModifyCategories(budgetEntries, newCategory: "Room Rent", descriptions: new string[] { "ZELLE TO THIRUMALAI SANTHOSH", "BILL PAY Jeff", "Elan Union" });
            ModifyCategories(budgetEntries, newCategory: "Tax", descriptions: new string[] { "MATTHEW JAIBU" });
            ModifyCategories(budgetEntries, newCategory: "Mobile Phone", descriptions: new string[] { "SREELEELA KA" });
            return budgetEntries;

        }

        private void ModifyColumn(List<Budget> budgetEntries, string propertyName, string existingValue, string newValue)
        {
            if (propertyName == "AccountName")
            {
                var entries = budgetEntries.Where(a => a.AccountName == existingValue).ToList();
                entries.ForEach(i => i.AccountName = newValue);
            }
        }

        private void ModifyCategories(List<Budget> budgetEntries, string newCategory, string existingCategory = null, string[] accountNames = null, string[] descriptions = null)
        {
            var tmpAccountNames = accountNames?.ToList();
            var tmpDescriptions = descriptions?.ToList();
            var filterBudgetList = (existingCategory != null) ? budgetEntries.Where(b => b.Category == existingCategory) : budgetEntries;

            if (descriptions != null && accountNames != null)
            {
                filterBudgetList = filterBudgetList.Where(b => tmpDescriptions.Any(description => b.Description.ToLower().Contains(description.ToLower())) && tmpAccountNames.Any(accountName => b.AccountName.ToLower().Contains(accountName.ToLower())));
            }
            else if (descriptions != null)
            {
                filterBudgetList = filterBudgetList.Where(b => tmpDescriptions.Any(description => b.Description.ToLower().Contains(description.ToLower())));

            }
            else if (accountNames != null)
            {
                filterBudgetList = filterBudgetList.Where(b => tmpAccountNames.Any(accountName => b.AccountName.ToLower().Contains(accountName.ToLower())));
            }
            foreach (var item in filterBudgetList.ToList())
            {
                budgetEntries.Find(i => i == item).Category = newCategory;
            }

        }

        private void CleanupEntries(List<Budget> budgetEntries, string category, string accountName = null, string description = null)
        {

            var filterBudgetList = budgetEntries.Where(b => b.Category == category);
            if (description != null && accountName != null)
            {
                filterBudgetList = filterBudgetList.Where(b => b.Description.ToLower().Contains(description.ToLower()) && b.AccountName.ToLower().Contains(accountName.ToLower()));
            }
            else if (description != null)
            {
                filterBudgetList = filterBudgetList.Where(b => b.Description.ToLower().Contains(description.ToLower()));

            }
            else if (accountName != null)
            {
                filterBudgetList = filterBudgetList.Where(b => b.AccountName.ToLower().Contains(accountName.ToLower()));
            }
            foreach (var item in filterBudgetList.ToList())
            {
                budgetEntries.Remove(item);
            }
        }

        private List<Budget> RemoveDuplicateEntries(List<Budget> budgetEntries, bool removePotentialDuplicateEntries)
        {
            //Get "Distinct" records. Often there are duplicates
            budgetEntries = budgetEntries.Distinct(new DistinctItemComparer()).ToList();

            //Identify Potential Duplicates
            var potentialDuplicates = budgetEntries.GroupBy(x => new { x.Category, x.Date, x.Amount, x.TransactionType })
                                                  .Where(g => g.Count() > 1)
                                                  .Select(y => y.Key)
                                                  .ToList();

            //Flag them as duplicates OR Remove them
            foreach (var duplicate in potentialDuplicates)
            {
                var duplicates = budgetEntries.FindAll(i => i.Amount == duplicate.Amount && i.Date == duplicate.Date && i.Category == duplicate.Category && i.TransactionType == duplicate.TransactionType);
                for (int i = 1; i < duplicates.Count; i ++)
                {
                    if (removePotentialDuplicateEntries)
                    {
                        budgetEntries.Remove(duplicates[i]);

                    } else
                    {
                        duplicates[i].IsAPotentialDuplicate = true;
                        duplicates[i].Description = "[MIGHT BE A DUPLICATE]:" + duplicates[i].Description;
                    }
                }
            }
            return budgetEntries;
        }
        
        private List<BudgetReport> ProcessBudget(List<Budget> budgetRecords)
        {
            //Get "debit" TransType
            List<BudgetReport> tmpBudgetDebitReportList = ProcessBudgetRecordsByTransType(budgetRecords, "debit");
            //Get "credit" TransType -- REFUNDS
            List<BudgetReport> tmpBudgetCreditReportList = ProcessBudgetRecordsByTransType(budgetRecords, "credit");

            //Bind multiple categories into a useful form by looking at the config file
            List<BudgetReport> budgetReportList = new List<BudgetReport>();
            var appSettings = ConfigurationManager.AppSettings;
            foreach (var key in appSettings.AllKeys)
            {
                BudgetReport bReport = new BudgetReport();
                bReport.Category = key;
                string[] categoryValues = appSettings[key].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string category in categoryValues)
                {
                    var tmpDebitBudget = tmpBudgetDebitReportList.Find(i => i.Category.Equals(category));
                    if (tmpDebitBudget != null)
                    {
                        tmpDebitBudget.IsProcessed = true;
                        bReport.TotalAmount += tmpDebitBudget.TotalAmount;
                        bReport.TotalPotentialDuplicates += tmpDebitBudget.TotalPotentialDuplicates;
                        bReport.Notes += tmpDebitBudget.Notes;
                    }

                    //also check for refunds - based on credit type
                    var tmpCreditBudget = tmpBudgetCreditReportList.Find(i => i.Category.Equals(category));
                    if (tmpCreditBudget != null)
                    {
                        tmpCreditBudget.IsProcessed = true;
                        bReport.TotalAmount -= tmpCreditBudget.TotalAmount;
                        bReport.TotalPotentialDuplicates += tmpCreditBudget.TotalPotentialDuplicates;
                        bReport.Notes += tmpCreditBudget.Notes;
                    }
                }
                budgetReportList.Add(bReport);
            }
            //Add all remaining Debit purchases
            budgetReportList.AddRange(tmpBudgetDebitReportList.FindAll(i => !i.IsProcessed));
            //Add all remaining Credit purchases
            budgetReportList.AddRange(tmpBudgetCreditReportList.FindAll(i => !i.IsProcessed));

            return budgetReportList;
        }
        
        private List<BudgetReport> ProcessBudgetRecordsByTransType(IEnumerable<Budget> budgetRecords, string transType)
        {
            List<BudgetReport> tmpBudgetReportList = new List<BudgetReport>();

            var queryByCategory =
                                        from record in budgetRecords
                                        where record.TransactionType.Equals(transType)
                                        group record by record.Category into g
                                        select new
                                        {
                                            Category = g.Key,
                                            TotalAmount = g.Sum(record => record.Amount),
                                            TotalPotentialDuplicates = g.Count(record => record.IsAPotentialDuplicate),
                                            TempDescription = string.Join("#;", g.Select(i => i.Description)),
                                            TempAmount = string.Join("#;", g.Select(i => i.Amount)),
                                            TempDates = string.Join("#;", g.Select(i => i.Date))
                                        };


            foreach (var row in queryByCategory)
            {
                BudgetReport bReport = new BudgetReport();
                bReport.Category = row.Category;
                bReport.TotalAmount = row.TotalAmount;
                bReport.TotalPotentialDuplicates = row.TotalPotentialDuplicates;
                bReport.TransType = transType;

                string[] splitDescription = row.TempDescription.Split(new string[] { "#;" }, StringSplitOptions.RemoveEmptyEntries);
                string[] splitAmount = row.TempAmount.Split(new string[] { "#;" }, StringSplitOptions.RemoveEmptyEntries);
                string[] splitDates = row.TempDates.Split(new string[] { "#;" }, StringSplitOptions.RemoveEmptyEntries);

                if (splitDescription.Length == splitAmount.Length)
                {
                    StringBuilder strNotes = new StringBuilder();
                    for (int i = 0; i < splitDescription.Length; i++)
                    {
                        strNotes = strNotes.AppendLine(formatNotes(transType, splitDescription[i], splitDates[i], Double.Parse(splitAmount[i])).ToString());
                    }
                    bReport.Notes = strNotes.ToString();
                }
                tmpBudgetReportList.Add(bReport);
            }
            return tmpBudgetReportList;
        }        

        private string formatNotes(string transType, string description, string strDate, double amount)
        {
            var date = DateTime.Parse(strDate);
            return string.Format("{1}:{2}:  {3}:({0})", transType, description, amount.ToString("C", CultureInfo.CurrentCulture), date.ToString("MM/dd/yyyy"));
        }

     /*
         private void RemoveNotesFromBudget(string note, BudgetReport budget)
         {
             int i = budget.Notes.IndexOf(note);
             budget.Notes = budget.Notes.Remove(i, note.Length);
         }

         private void AddToReport(string category, double amount, string date, string description, List<BudgetReport> report, string type = null)
         {
             bool isNewCategory = false;
             var budgetCategory = report.Find(i => i.Category.Contains(category));
             if (budgetCategory == null)
             {
                 budgetCategory = new BudgetReport();
                 budgetCategory.Category = category;
                 budgetCategory.TransType = "debit"; //By default - TODO
                 isNewCategory = true;
             }
             budgetCategory.TotalAmount += amount;
             if (type != null)
                 budgetCategory.TransType = type;
             if (!String.IsNullOrEmpty(description) && !String.IsNullOrEmpty(date))
             {
                 StringBuilder sBuilder = new StringBuilder();
                 budgetCategory.Notes += sBuilder.AppendLine(formatNotes(budgetCategory.TransType, description, date, amount).ToString()).ToString();
             }
             if (isNewCategory) report.Add(budgetCategory);
         }


         private void CalculateTotalExpenses(List<BudgetReport> budgetReportList)
         {
             //TotalIncome = Rental Income +  Paycheck Income + Interest Income
             //TotalExpenses = All - TotalIncome - Credit Card Payments - Other Income - Transfer

             double totalIncome = (from record in budgetReportList
                                   where record.Category.Equals("Rental Income") ||
                                         record.Category.Equals("Paycheck Income") ||
                                         record.Category.Equals("Interest Income")
                                   select record.TotalAmount).Sum();

             double totalExpenses = (from record in budgetReportList
                                     where !record.Category.Equals("Credit Card Payments") &&
                                           !record.Category.Equals("Other Income") &&
                                           !record.Category.Equals("Transfer") &&
                                           !record.Category.Equals("Rental Income") &&
                                           !record.Category.Equals("Paycheck Income") &&
                                           !record.Category.Equals("Interest Income")
                                     select record.TotalAmount).Sum();

             //AddEmptyRecord(budgetReportList);
             AddToReport("Total Income", Math.Abs(totalIncome), "", "", budgetReportList, "");
             AddToReport("Total Expenses", Math.Abs(totalExpenses), "", "", budgetReportList, "");
             AddToReport("Total Savings", Math.Abs(totalIncome) - Math.Abs(totalExpenses), "", "", budgetReportList, "");
         }

         private void FixCategories(string existingCategory, List<BudgetReport> budgetReportList, string newCategory, string[] labels)
         {
             //Moving Sam's & Costco from "Shopping and Sporting Goods" to "Groceries"
             //var shoppingEntries = budgetReportList.Find(i => i.Category.Equals("Shopping & Sporting Goods"));
             var entries = budgetReportList.Find(i => i.Category.Equals(existingCategory));
             if (entries != null)
             {
                 if (entries.Notes != null)
                 {
                     string[] notes = entries.Notes.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                     for (int i = 0; i < notes.Length; i++)
                     {
                         string[] note = notes[i].Split(new char[] { ':' });
                         var description = String.Join(" ", note);
                         bool ifLabelExists = Array.Exists(labels, e =>
                         {
                             if (description.IndexOf(e, StringComparison.OrdinalIgnoreCase) >= 0)
                                 return true;
                             else
                                 return false;
                         });
                         if (ifLabelExists)
                         {
                             int noteIdx = 1;
                             description = note[0];
                             double amount = 0;
                             //In most cases, you will amount in index location '1'...
                             while (!Double.TryParse(note[noteIdx].Trim(), NumberStyles.Currency, CultureInfo.CreateSpecificCulture("en-US"), out amount))
                             {
                                 description = $"{description} {note[noteIdx]}";
                                 noteIdx++;
                             }
                             string date = note[noteIdx + 1].Trim();
                             string type = note[noteIdx + 2].Replace('(', ' ').Replace(')', ' ').Trim();
                             if (type.Equals("credit"))
                                 amount = -amount;
                             AddToReport(newCategory, amount, date, description, budgetReportList, type);
                             RemoveNotesFromBudget(notes[i], entries);
                             entries.TotalAmount -= amount;
                         }
                     }
                 }
             }
         }
     */

    }
}