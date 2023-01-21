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
        public bool IsAPotentialDuplicate { get; set; }
    }

    public sealed class BudgetClassMap : ClassMap<Budget>
    {
        public BudgetClassMap()
        {
            Map(m => m.AccountName).Name("Account Name");
            Map(m => m.Amount).Name("Amount").TypeConverterOption.NumberStyles(NumberStyles.Float);
            Map(m => m.Date).Name("Date").TypeConverterOption.DateTimeStyles(DateTimeStyles.AdjustToUniversal);
            Map(m => m.Description).Name("Description").TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
            Map(m => m.OriginalDescription).Name("Original Description").TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
            Map(m => m.TransactionType).Name("Transaction Type");
            Map(m => m.Category).Name("Category");
        }
    }

    public class BudgetReport
    {
        public string Category { get; set; }
        public double TotalAmount { get; set; }
        public string Notes { get; set; }
        public string Description { get; set; }
        public string AccountName { get; set; }
        public string TransType { get; set; }
        public int TotalPotentialDuplicates { get; set; }

        public bool IsProcessed = false;
    }

    public class DistinctItemComparer : IEqualityComparer<Budget>
    {
        public bool Equals(Budget x, Budget y)
        {
            return x.Date == y.Date &&
                x.Description.Replace(" ", string.Empty) == y.Description.Replace(" ", string.Empty) &&
                x.OriginalDescription.Replace(" ", string.Empty) == y.OriginalDescription.Replace(" ", string.Empty) &&
                x.TransactionType == y.TransactionType &&
                x.Category.Replace(" ", string.Empty) == y.Category.Replace(" ", string.Empty) &&
                x.AccountName.Replace(" ", string.Empty) == y.AccountName.Replace(" ", string.Empty) &&
                x.Amount == y.Amount;
        }

        public int GetHashCode(Budget obj)
        {
            
            return obj.Date.GetHashCode() ^
                obj.Description.Replace(" ", string.Empty).GetHashCode() ^
                obj.OriginalDescription.Replace(" ", string.Empty).GetHashCode() ^
                obj.TransactionType.GetHashCode() ^
                obj.Category.Replace(" ", string.Empty).GetHashCode() ^
                obj.AccountName.Replace(" ", string.Empty).GetHashCode() ^
                obj.Amount.GetHashCode();
        }
    }

    //Ignoring Account Name
    public class DistinctItemComparerV2 : IEqualityComparer<Budget>
    {

        public bool Equals(Budget x, Budget y)
        {
            return x.Date == y.Date &&
                x.Description.Replace(" ", string.Empty) == y.Description.Replace(" ", string.Empty) &&
                x.OriginalDescription.Replace(" ", string.Empty) == y.OriginalDescription.Replace(" ", string.Empty) &&
                x.TransactionType == y.TransactionType &&
                x.Category.Replace(" ", string.Empty) == y.Category.Replace(" ", string.Empty) &&
                x.Amount == y.Amount;
        }

        public int GetHashCode(Budget obj)
        {
            return obj.Date.GetHashCode() ^
                obj.Description.Replace(" ", string.Empty).GetHashCode() ^
                obj.OriginalDescription.Replace(" ", string.Empty).GetHashCode() ^
                obj.TransactionType.GetHashCode() ^
                obj.Category.Replace(" ", string.Empty).GetHashCode() ^
                obj.Amount.GetHashCode();
        }
    }
}
