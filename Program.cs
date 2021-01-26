
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
            if (CheckIfExistsInDatabase(fileHash))
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
            SaveLunchMenuToDatabaseAsync(lm).Wait();

            Console.WriteLine("Logging this session...");
            LogMenuSavedAsync(fileHash, lm).Wait();

            

            //foreach (string file in _localtestfiles)
            //{
            //    Console.WriteLine("Calculate hash...");
            //    Console.WriteLine(string.Format("{0}: {1}", file, CalculateSHA256CheckSum(file)));
            //
            //    Console.WriteLine("Reading text...");
            //    //string menu = GetText(_menufilename);
            //    string menu = GetText(file);
            //                
            //    LunchMenu lm = new LunchMenu(menu);
            //    Console.WriteLine(lm.ToString());
            //
            //    Console.WriteLine("Saving to database...");
            //    SaveLunchMenuToDatabaseAsync(lm).Wait();
            //}

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

        static async Task SaveLunchMenuToDatabaseAsync(LunchMenu lm)
        {
            var connString = "Server=127.0.0.1;User ID=root;Password=pwdpwd;Database=test";


            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();

                foreach (LunchMenuItem lmi in lm.MenuItems)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = string.Format("INSERT INTO lunch_menu_items (Name,Price,Foodtype,Date,Weight,Alergens) VALUES ({0})",
                            lmi.ToSqlQueryValues());
                        //cmd.Parameters.AddWithValue("p", "Hello world");
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                // Retrieve all rows
                //using (var cmd = new MySqlCommand("SELECT * FROM lunchmenu", conn))
                //using (var reader = await cmd.ExecuteReaderAsync())
                //    while (await reader.ReadAsync())
                //        Console.WriteLine(string.Format("{0} - {1} - {2}", reader.GetInt16(0), reader.GetString(1), reader.GetDateTime(4)));
            }


        }

        static bool CheckIfExistsInDatabase(string checksum)
        {
            var connString = "Server=127.0.0.1;User ID=root;Password=pwdpwd;Database=test";

            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(string.Format("SELECT * FROM saved_lunch_menus WHERE Checksum = '{0}';", checksum), conn))
                using (var reader = cmd.ExecuteReader())
                    return reader.HasRows;

            }
        }

        static async Task LogMenuSavedAsync(string checksum, LunchMenu lm)
        {
            var connString = "Server=127.0.0.1;User ID=root;Password=pwdpwd;Database=test";

            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = string.Format("INSERT INTO saved_lunch_menus (SavedAt,Checksum,ValidFrom,ValidTo) VALUES ('{0:yyyy-MM-dd hh:mm:ss}','{1}','{2:yyyy-MM-dd}','{3:yyyy-MM-dd}')", DateTime.Now, checksum, lm.ValidFrom, lm.ValidTo);
                    await cmd.ExecuteNonQueryAsync();
                    }
                
                
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
