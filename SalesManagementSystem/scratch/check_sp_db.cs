using System;
using System.IO;
using Dapper;
using SalesManagementSystem.Data;

namespace CheckSP
{
    class Program
    {
        static void Main()
        {
            try
            {
                var db = new DbConnectionFactory();
                using (var conn = db.CreateConnection())
                {
                    var def = conn.QueryFirstOrDefault<string>("SELECT OBJECT_DEFINITION(OBJECT_ID('sp_Dashboard_GetData'))");
                    File.WriteAllText("scratch/sp_db_def.txt", def ?? "NULL");
                    Console.WriteLine("SP definition written to scratch/sp_db_def.txt");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
