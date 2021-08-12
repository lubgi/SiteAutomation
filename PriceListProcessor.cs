using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace SiteAutomation
{
    public static class PriceListProcessor
    {
        private static readonly string ExePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        public static void ProcessPriceList()
        {
            DownloadPricelist();

            Excel.Application excelApp = new Excel.Application
                                         {
                                             Visible = false,
                                             DisplayAlerts = false
                                         };
            Excel.Workbook workbooks = excelApp.Workbooks.Open(Filename: Path.Combine(ExePath, "PriceList/DownloadedPriceList.xls"), CorruptLoad: true);
            Excel.Workbook workBook  = excelApp.Workbooks[1];
            var worksheet = workbooks.Sheets[1];

            int lastUsedRow = excelApp.Cells
                                      .Find("*", 
                                            SearchOrder: Excel.XlSearchOrder.xlByRows,
                                            SearchDirection: Excel.XlSearchDirection.xlPrevious,
                                            MatchCase: false)
                                      .Row;

            using (ProgressBar progress = new ProgressBar())
            {
                for (int i = 2; i < lastUsedRow; i++)
                {
                    progress.Report((double)i / lastUsedRow);
                    ProductRow productRow = new ProductRow(excelApp.Range["M" + i], excelApp.Range["A" + i]);
                    if (productRow.Price < 100 || productRow.Name.Contains("УЦЕНКА"))
                    {
                        productRow.PriceCell.EntireRow.Delete(Type.Missing);
                        lastUsedRow--;
                        i--;
                        continue;
                    }
                    productRow.PriceCell.Value2 = (int)(productRow.Price * 1.3) / 10 * 10 + 10;
                    productRow.NameCell.Value2 = ProcessName(excelApp.Range["A" + i].Value2.ToString());
                }
            }


            //fix pathes
            workBook.SaveAs(Path.Combine(ExePath, "PriceList/ProcessedPriceList.xls"),
                                         Excel.XlFileFormat.xlWorkbookDefault,
                                         ReadOnlyRecommended: false, CreateBackup: false,
                                         AccessMode: Excel.XlSaveAsAccessMode.xlNoChange,
                                         ConflictResolution: Excel.XlSaveConflictResolution.xlLocalSessionChanges);
            workBook.Close();
            excelApp.Quit();

            FtpSendFile(@"PriceList/ProcessedPriceList.xls", "/public_html/admin/uploads/8.xls");
        }

        private static void DownloadPricelist()
        {
            Directory.CreateDirectory("PriceList");
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(ConfigurationManager.AppSettings["PriceListDonor"], "PriceList/DownloadedPriceList.xls");
            }
        }


        private static string ProcessName(string productName)
        {
            string processedName = Regex.Replace(productName, @"\s\(.*\)|( в коробке)|(гр )", string.Empty)
                                        .Split(",", StringSplitOptions.RemoveEmptyEntries)[0]
                                        .Split("Цена")[0]
                                        .Trim('/', ' ');

            return Regex.Replace(processedName, @"\s+", " ");
        }

        public static string FtpSendFile(string filePath, string sitePath)
        {
            string ftpHost  = ConfigurationManager.AppSettings["FtpHost"];
            string username = ConfigurationManager.AppSettings["FtpUsername"];
            string password = ConfigurationManager.AppSettings["FtpPassword"];

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpHost + sitePath);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(username, password);

            FileInfo fi = new FileInfo(filePath);

            request.ContentLength = fi.Length;
            byte[] buffer     = new byte[4097];
            int    totalBytes = (int)fi.Length;

            using (FileStream fs = fi.OpenRead())
            using (Stream rs = request.GetRequestStream())
            {
                while (totalBytes > 0)
                {
                    int bytes = fs.Read(buffer, 0, buffer.Length);
                    rs.Write(buffer, 0, bytes);
                    totalBytes -= bytes;
                }
            }

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                return response.StatusDescription;
            }
        }

        public class ProductRow
        {
            public double Price { get; set; }
            public string Name { get; set; }
            public Excel.Range PriceCell { get; set; }
            public Excel.Range NameCell { get; set; }

            public ProductRow(Excel.Range priceCell, Excel.Range nameCell)
            {
                Price = double.Parse(priceCell.Value2.ToString());
                Name = nameCell.Value2.ToString();
                PriceCell = priceCell;
                NameCell = nameCell;
            }
        }
    }


    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int blockCount = 10;
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private const string animation = @"|/-\";

        private readonly Timer timer;

        private double currentProgress = 0;
        private string currentText = string.Empty;
        private bool disposed = false;
        private int animationIndex = 0;

        public ProgressBar()
        {
            timer = new Timer(TimerHandler);

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected) ResetTimer();
        }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref currentProgress, value);
        }

        private void TimerHandler(object state)
        {
            lock (timer)
            {
                if (disposed) return;

                int progressBlockCount = (int)(currentProgress * blockCount);
                int percent            = (int)(currentProgress * 100);
                string text = string.Format("[{0}{1}] {2,3}% {3}",
                                            new string('#', progressBlockCount),
                                            new string('-', blockCount - progressBlockCount),
                                            percent,
                                            animation[animationIndex++ % animation.Length]);
                UpdateText(text);

                ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            // Get length of common portion
            int commonPrefixLength = 0;
            int commonLength       = Math.Min(currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
                commonPrefixLength++;

            // Backtrack to the first differing character
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.Substring(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            int overlapCount = currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            Console.Write(outputBuilder);
            currentText = text;
        }

        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (timer)
            {
                disposed = true;
                UpdateText(string.Empty);
            }
        }
    }
}