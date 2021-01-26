using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LunchMenuLogger
{
    static class DbService
    {
        private static string DbServer = "127.0.0.1";
        private static string DbUser = "root";
        private static string DbPass = "pwdpwd";
        private static string DbName = "test";


        public static void SetConnectionParameters(string dbServer, string dbUser, string dbPass, string dbName)
        {
            DbServer = dbServer;
            DbUser = dbUser;
            DbPass = dbPass;
            DbName = dbName;
        }


        private static string ConnectionString()
        {
            return string.Format("Server={0};User ID={1};Password={2};Database={3}", DbServer, DbUser, DbPass, DbName);
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
                    cmd.CommandText = string.Format("INSERT INTO saved_lunch_menus (SavedAt,Checksum,ValidFrom,ValidTo) VALUES ('{0:yyyy-MM-dd HH:mm:ss}','{1}','{2:yyyy-MM-dd}','{3:yyyy-MM-dd}')", DateTime.Now, checksum, lm.ValidFrom, lm.ValidTo);
                    await cmd.ExecuteNonQueryAsync();
                }


            }


        }



    }
}
