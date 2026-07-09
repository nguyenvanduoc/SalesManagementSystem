using System;
using System.Data;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        // Ket noi DB chinh cua ung dung (khac voi SalesWarehouseDB)
        // Thu ket noi de tim DB
        string[] dbNames = { "SalesManagementDB", "SalesMgmtDB", "QuanLyBanHang", "SMS", "QLBH", "SalesDB" };
        foreach(var db in dbNames)
        {
            try
            {
                string cs = "Data Source=localhost;Initial Catalog=" + db + ";Integrated Security=True;Connection Timeout=2";
                using (var conn = new SqlConnection(cs))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM KHO_PhieuNhap", conn))
                    {
                        int cnt = (int)cmd.ExecuteScalar();
                        Console.WriteLine("DB: " + db + " -> KHO_PhieuNhap count = " + cnt);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("DB: " + db + " -> FAIL: " + ex.Message.Split('.')[0]);
            }
        }
    }
}
