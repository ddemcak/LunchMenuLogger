using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LunchMenuLogger
{
    static class DbService
    {
        public static string _DbServer = "127.0.0.1";
        public static string _DbUser = "root";
        public static string _DbPass = "pwdpwd";
        public static string _DbName = "test";


        private static string ConnectionString()
        {
            return string.Format("Server={0};User ID={1};Password={2};Database={3}", _DbServer, _DbUser, _DbPass, _DbName);
        }


        public static async Task SaveLunchMenuToDatabaseAsync(LunchMenu lm)
        {
            var connString = ConnectionString();

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
                        
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }


        }

        public static bool CheckIfExistsInDatabase(string checksum)
        {
            var connString = ConnectionString();

            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(string.Format("SELECT * FROM saved_lunch_menus WHERE Checksum = '{0}';", checksum), conn))
                using (var reader = cmd.ExecuteReader())
                    return reader.HasRows;

            }
        }

        public static async Task LogMenuSavedAsync(string checksum, LunchMenu lm)
        {
            var connString = ConnectionString();

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



    }
}
