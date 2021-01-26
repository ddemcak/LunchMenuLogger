
using LunchMenuLogger.Configurators;
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
        private static string _menufilename = "lunchmenu.pdf";
        
        #region PROPERTIES

        private static ConfigurationFileManager cfm;
        private static string LunchMenuUrl = "";

        #endregion

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please provide CONFIG filename as parameter!");
                return;
            }

            Console.WriteLine("Loading CONFIG file...");
            string configFile = args[0];


            if (!File.Exists(configFile))
            {
                Console.WriteLine("CONFIG file does not exist! Exiting application...");
                return;
            }
            else
            {
                cfm = new ConfigurationFileManager(configFile);


                LunchMenuUrl = cfm.LunchMenuUrl;
                DbService._DbServer = cfm.DbServer;
                DbService._DbName = cfm.DbName;
                DbService._DbPass = cfm.DbPass;
                DbService._DbName = cfm.DbName;
            }


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
            string menu = GetRawTextFromFile(newfilename);
            
            LunchMenu lm = new LunchMenu(menu);
            Console.WriteLine(lm.ToString());

            Console.WriteLine("Saving to database...");
            DbService.SaveLunchMenuToDatabaseAsync(lm).Wait();

            Console.WriteLine("Logging this session...");
            DbService.LogMenuSavedAsync(fileHash, lm).Wait();

        }

        static string GetRawTextFromFile(string pdffilename)
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
                client.DownloadFile(LunchMenuUrl, _menufilename);
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
