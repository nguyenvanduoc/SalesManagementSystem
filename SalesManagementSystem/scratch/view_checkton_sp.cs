using System;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;

namespace TestCheck
{
    class Program
    {
        static void Main()
        {
            var db = new DbConnectionFactory();
            using (var conn = db.CreateConnection())
            {
                conn.Open();

                var def = conn.ExecuteScalar<string>("SELECT OBJECT_DEFINITION(OBJECT_ID('sp_KHO_TonKho_CheckByKho'))");
                Console.WriteLine("==================== sp_KHO_TonKho_CheckByKho ====================");
                Console.WriteLine(def);
            }
        }
    }
}
