using System;
using System.Data;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        try {
            using(var conn = new SqlConnection(@"Data Source=.;Initial Catalog=SalesManagementSystem;Integrated Security=True"))
            {
                conn.Open();
                using(var cmd = new SqlCommand("sp_KT_PhieuChi_GetList", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using(var reader = cmd.ExecuteReader())
                    {
                        for(int i = 0; i < reader.FieldCount; i++) {
                            Console.WriteLine(reader.GetName(i));
                        }
                    }
                }
            }
        } catch(Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
