using System;
using System.IO;
using SalesManagementSystem.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem
{
    class ApplySql
    {
        static void Main(string[] args)
        {
            try
            {
                string script = File.ReadAllText(@"..\App_Data\sp_KHO_TonKho.sql");
                var db = new DbConnectionFactory();
                using (var conn = (SqlConnection)db.CreateConnection())
                {
                    conn.Open();
                    var commands = script.Split(new[] { "GO\r\n", "GO\n", "GO " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var cmd in commands)
                    {
                        if (string.IsNullOrWhiteSpace(cmd)) continue;
                        using (var command = new SqlCommand(cmd, conn))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine("Successfully applied SQL script.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
            }
        }
    }
}
