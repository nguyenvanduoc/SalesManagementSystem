using System;
using System.Data.SqlClient;
using SalesManagementSystem.Helpers.Security;

namespace SalesManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string connStr = ConfigManager.GetConnectionString("DefaultConnection");
                Console.WriteLine("Connection string: " + connStr);
                
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT ID, TenDangNhap, HoDem, Ten, MatKhau, IsActive FROM ACL_Login", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("Users list:");
                        while (reader.Read())
                        {
                            Console.WriteLine(string.Format("ID: {0}, TenDangNhap: {1}, Name: {2} {3}, PasswordHash: {4}, IsActive: {5}",
                                reader["ID"], reader["TenDangNhap"], reader["HoDem"], reader["Ten"], reader["MatKhau"], reader["IsActive"]));
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
