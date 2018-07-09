using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetParserApp
{
    public class Budget
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string OriginalDescription { get; set; }
        public double Amount { get; set; }
        public string TransactionType { get; set; }
        public string Category { get; set; }
        public string AccountName { get; set; }
    }

    public sealed class BudgetClassMap : CsvClassMap<Budget>
    {
        public BudgetClassMap()
        {
            Map(m => m.AccountName).Name("Account Name");
            Map(m => m.Amount).Name("Amount").TypeConverterOption(NumberStyles.Float);
            Map(m => m.Date).Name("Date").TypeConverterOption(DateTimeStyles.AdjustToUniversal);
            Map(m => m.Description).Name("Description").TypeConverterOption(CultureInfo.InvariantCulture);
            Map(m => m.OriginalDescription).Name("Original Description").TypeConverterOption(CultureInfo.InvariantCulture);
            Map(m => m.TransactionType).Name("Transaction Type");
            Map(m => m.Category).Name("Category");
        }
    }

    public class BudgetReport
    {
        public string Category { get; set; }
        public double TotalAmount { get; set; }
        public string Notes { get; set; }
        public string TransType { get; set; }

        public bool IsProcessed = false;

    }
}
