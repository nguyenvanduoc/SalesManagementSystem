using System;
using System.Data.SqlClient;
using SalesManagementSystem.Data;

namespace SalesManagementSystem
{
    class GetSpText
    {
        static void Main(string[] args)
        {
            try
            {
                var db = new DbConnectionFactory();
                using (var conn = (SqlConnection)db.CreateConnection())
                {
                    conn.Open();
                    using (var command = new SqlCommand("sp_helptext 'sp_KHO_TonKho_CheckChuyenKho'", conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.Write(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
            }
        }
    }
}
