
using MySqlConnector;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace LunchMenuLogger
{
    class Program
    {
        private static string _localtestfilename = "technopark_cz_1.pdf";
        private static string[] _localtestfiles = { "technopark_cz_1.pdf", "technopark_cz_2.pdf", "technopark_cz_3.pdf", "technopark_cz_4.pdf" };

        private static string _menufilename = "lunchmenu.pdf";
        private static string _url = "https://cookforlife.cz/jidelni-listky/technopark_cz.pdf";


        //private static string _menufilename = "technopark_cz_4.pdf";


        static void Main(string[] args)
        {
            Console.WriteLine("Downloading document...");
            DownloadPDF();

            Console.WriteLine("Calculate hash...");
            string fileHash = CalculateSHA256CheckSum(_menufilename);
            Console.WriteLine(string.Format("{0}: {1}", _menufilename, fileHash));

            Console.WriteLine("Creating new name for a document...");
            string newfilename = string.Format("{0:yyyy-MM-dd_HH-mm-ss}.pdf", DateTime.Now);

            Console.WriteLine("Checking if file was processed...");
            if (DbService.CheckIfExistsInDatabase(fileHash))
            {
                Console.WriteLine("Document has been already processed. Quitting...");
                return;
            }

            Console.WriteLine("Copying document...");
            File.Copy(_menufilename, newfilename);

            Console.WriteLine("Processing menu...");
            string menu = GetText(newfilename);
            LunchMenu lm = new LunchMenu(menu);
            Console.WriteLine(lm.ToString());

            Console.WriteLine("Saving to database...");
            DbService.SaveLunchMenuToDatabaseAsync(lm).Wait();

            Console.WriteLine("Logging this session...");
            DbService.LogMenuSavedAsync(fileHash, lm).Wait();

        }

        static string GetText(string pdffilename)
        {
            using (PdfDocument document = PdfDocument.Open(pdffilename))
            {
                var result = new StringBuilder();

                foreach (Page page in document.GetPages())
                {
                    string pageText = page.Text;

                    result.Append(pageText);

                }
                return result.ToString();
            }
        }

        static void DeletePDF()
        {
            if (File.Exists(_menufilename)) File.Delete(_menufilename);
        }
        
        static void DownloadPDF()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(_url, _menufilename);
            }
        }
                

        static string CalculateSHA256CheckSum(string filename)
        {
            using (SHA256 SHA256 = SHA256Managed.Create())
            {
                using (FileStream fileStream = File.OpenRead(filename))
                    return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
            }
        }




    }
}
