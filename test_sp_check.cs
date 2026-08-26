using System;
using System.Data.SqlClient;
using Dapper;

class Program
{
    static void Main()
    {
        string connStr = "Data Source=localhost\\MSSQLSERVER01;Initial Catalog=SalesWarehouseDB;Integrated Security=True;";
        using (var conn = new SqlConnection(connStr))
        {
            conn.Open();

            string json6 = "[{\"IDSanPham\":6,\"SoLuongCanXuat\":5000}]";
            string json7 = "[{\"IDSanPham\":7,\"SoLuongCanXuat\":5000}]";

            var res6 = conn.Query("sp_KHO_TonKho_CheckByKho", new { IDKho = 4, ListSanPham = json6 }, commandType: System.Data.CommandType.StoredProcedure);
            foreach (var r in res6)
            {
                Console.WriteLine("SP6: IDKho=" + r.IDKho + ", Ma=" + r.MaSanPham + ", Ten=" + r.TenSanPham + ", CanXuat=" + r.SoLuongCanXuat + ", Ton=" + r.SoLuongTon + ", IsDuTon=" + r.IsDuTon);
            }

            var res7 = conn.Query("sp_KHO_TonKho_CheckByKho", new { IDKho = 4, ListSanPham = json7 }, commandType: System.Data.CommandType.StoredProcedure);
            foreach (var r in res7)
            {
                Console.WriteLine("SP7: IDKho=" + r.IDKho + ", Ma=" + r.MaSanPham + ", Ten=" + r.TenSanPham + ", CanXuat=" + r.SoLuongCanXuat + ", Ton=" + r.SoLuongTon + ", IsDuTon=" + r.IsDuTon);
            }
        }
    }
}
