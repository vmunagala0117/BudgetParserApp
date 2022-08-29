using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace BudgetParserApp
{
    public static class Logger
    {
        private static string GetTempPath()
        {
            string path = System.Environment.GetEnvironmentVariable("TEMP");
            if (!path.EndsWith("\\")) path += "\\";
            return path;
        }

        public static void LogMessageToFile(string msg)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText(
                GetTempPath() + "BudgetTransactionLog.txt");
            try
            {
                string logLine = System.String.Format(
                    "{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }

        public static void LogMessageToCsvFile(StringBuilder buffer, string fileName)
        {
            System.IO.File.AppendAllText(GetTempPath() + fileName, buffer.ToString());
        }

        public static void LogMessagetoExcelFile(IEnumerable<BudgetReport> report)
        {
            var excelApp = new Excel.Application();
            // Make the object visible.
            excelApp.Visible = true;
            object misValue = System.Reflection.Missing.Value;

            // Create a new, empty workbook and add it to the collection returned 
            // by property Workbooks. The new workbook becomes the active workbook.
            // Add has an optional parameter for specifying a praticular template. 
            // Because no argument is sent in this example, Add creates a new workbook. 
            Excel.Workbook workBook = excelApp.Workbooks.Add(misValue);

            // This example uses a single workSheet. The explicit type casting is
            // removed in a later procedure.
            Excel._Worksheet workSheet = (Excel.Worksheet)excelApp.ActiveSheet;

            // Establish column headings in cells A1 and B1.
            workSheet.Cells[1, "A"] = "Category";
            workSheet.Cells[1, "B"] = "Total Amount";
            workSheet.Cells[1, "C"] = "Potential Duplicates";

            var row = 1;
            foreach (var budget in report)
            {
                row++;
                workSheet.Cells[row, "A"] = budget.Category;
                workSheet.Cells[row, "B"] = budget.TotalAmount;
                workSheet.Cells[row, "C"] = budget.TotalPotentialDuplicates;
                if (budget.TotalPotentialDuplicates > 0)
                {
                    ((Excel.Range)workSheet.Cells[row, "C"]).Interior.Color = Excel.XlRgbColor.rgbLightSteelBlue;
                }

                if (!String.IsNullOrEmpty(budget.Notes))
                {
                    Excel.Range notesCell = excelApp.Application.get_Range("B" + row);
                    Excel.Comment comment = notesCell.AddComment();
                    comment.Shape.TextFrame.AutoSize = true;
                    comment.Text(budget.Notes);
                }
            }
            workSheet.Columns[1].AutoFit();
            workSheet.Columns[2].AutoFit();

            //workBook.SaveAs(GetTempPath() + fileName, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            //workBook.Close(true, misValue, misValue);
            //excelApp.Quit();

            //Marshal.ReleaseComObject(workSheet);
            //Marshal.ReleaseComObject(workBook);
            //Marshal.ReleaseComObject(excelApp);
        }
    }
}
