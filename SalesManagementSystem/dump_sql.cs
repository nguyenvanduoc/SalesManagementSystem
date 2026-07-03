using System;
using System.Data.SqlClient;
using System.IO;

class Program
{
    static void Main()
    {
        try {
            using(var conn = new SqlConnection(@"Data Source=.;Initial Catalog=SalesManagementSystem;Integrated Security=True"))
            {
                conn.Open();
                using(var cmd = new SqlCommand("sp_helptext 'sp_KT_PhieuChi_GetList'", conn))
                {
                    using(var reader = cmd.ExecuteReader())
                    {
                        using(var writer = new StreamWriter(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_KT_PhieuChi_GetList.sql"))
                        {
                            while(reader.Read())
                            {
                                writer.Write(reader.GetString(0));
                            }
                        }
                    }
                }
            }
        } catch(Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
