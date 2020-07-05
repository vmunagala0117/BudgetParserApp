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
            using (var csv = new CsvReader(File.OpenText(txtFilePath.Text)))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.RegisterClassMap<BudgetClassMap>();

                DateTime sDate = DateTime.Parse(startDate.Text);
                DateTime eDate = DateTime.Parse(endDate.Text);

                List<Budget> budgetRecords = csv.GetRecords<Budget>().Where(f => f.Date >= sDate && f.Date <= eDate).ToList();

                //remove duplicates -- original description is bit messed up. .. cleaning up by ignoring spaces. 
                /*List<Budget> budgetRecords = getBudgetRecords.GroupBy(r => new { r.AccountName, r.Amount, r.Category, r.Date, r.TransactionType, r.Description, OriginalDescription = r.OriginalDescription.Replace(" ","") })
                                                            .Select(r => r.First())
                                                            .ToList();
                */

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
                            bReport.Notes += tmpDebitBudget.Notes;
                        }

                        //also check for refunds - based on credit type
                        var tmpCreditBudget = tmpBudgetCreditReportList.Find(i => i.Category.Equals(category));
                        if (tmpCreditBudget != null)
                        {
                            tmpCreditBudget.IsProcessed = true;
                            bReport.TotalAmount -= tmpCreditBudget.TotalAmount;
                            bReport.Notes += tmpCreditBudget.Notes;
                        }
                    }
                    budgetReportList.Add(bReport);
                }
                //Add all remaining Debit purchases
                budgetReportList.AddRange(tmpBudgetDebitReportList.FindAll(i => !i.IsProcessed));
                //Add all remaining Credit purchases
                budgetReportList.AddRange(tmpBudgetCreditReportList.FindAll(i => !i.IsProcessed));

                FixCategories("Shopping & Sporting Goods", budgetReportList, "Groceries", new[] { "Sam's Club", "Costco" });
                FixCategories("Transfer", budgetReportList, "Daycare", new[] { "NANCYGKELLY", "HALLIEPETER" });
                FixCategories("Transfer", budgetReportList, "Miscellaneous", new[] { "Venmo" });
                FixCategories("Transfer", budgetReportList, "Mobile (ATT)", new[] { "SREELEELA" });
                FixCategories("Groceries", budgetReportList, "Water", new[] { "DS SERVICES" });
                FixCategories("Utilities", budgetReportList, "Mortgage", new[] { "CHARLESTON MANAGEMENT CORP" });
                FixCategories("Mortgage", budgetReportList, "Room Rent", new[] { "Jeff" });
                FixCategories("Credit Card Payments", budgetReportList, "Groceries", new[] { "Hello Brother Indian" });
                FixCategories("Credit Card Payments", budgetReportList, "Room Rent", new[] { "Erenterplan" });
                FixCategories("Rental Income", budgetReportList, "Paycheck Income", new[] { "DEPOSIT MADE IN A BRANCH" });
                FixCategories("Other Income", budgetReportList, "Rental Income", new[] { "RAVINDRA M BHEEM" });
                FixCategories("Other Income", budgetReportList, "Paycheck Income", new[] { "INSIGHT", "BAXALTA" });

                //At last calculate Expenses from the above budget list
                CalculateTotalExpenses(budgetReportList);

                //print
                Logger.LogMessagetoExcelFile(budgetReportList);
            }
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

        private void FixCategories(string mainCategory, List<BudgetReport> budgetReportList, string newCategory, string[] labels)
        {
            //Moving Sam's & Costco from "Shopping and Sporting Goods" to "Groceries"
            //var shoppingEntries = budgetReportList.Find(i => i.Category.Equals("Shopping & Sporting Goods"));
            var entries = budgetReportList.Find(i => i.Category.Equals(mainCategory));
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
                                            Total = g.Sum(record => record.Amount),
                                            TempDescription = string.Join("#;", g.Select(i => i.Description)),
                                            TempAmount = string.Join("#;", g.Select(i => i.Amount)),
                                            TempDates = string.Join("#;", g.Select(i => i.Date))
                                        };


            foreach (var row in queryByCategory)
            {
                BudgetReport bReport = new BudgetReport();
                bReport.Category = row.Category;
                bReport.TotalAmount = row.Total;
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

        private void RemoveNotesFromBudget(string note, BudgetReport budget)
        {
            int i = budget.Notes.IndexOf(note);
            budget.Notes = budget.Notes.Remove(i, note.Length);
        }

        private string formatNotes(string transType, string description, string strDate, double amount)
        {
            var date = DateTime.Parse(strDate);
            return string.Format("{1}:{2}:  {3}:({0})", transType, description, amount.ToString("C", CultureInfo.CurrentCulture), date.ToString("MM/dd/yyyy"));
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

        private void AddEmptyRecord(List<BudgetReport> report)
        {
            report.Add(new BudgetReport());            
        }
    }
}