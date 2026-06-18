using System;
using System.Web;
using System.Data.SqlClient;
using SalesManagementSystem.Helpers.Security;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SalesManagementSystem
{
    public class DumpSchema : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            string connStr = ConfigManager.GetConnectionString("DefaultConnection");
            var tables = new[] { "BAN_ChungTuBanHang", "BAN_ChungTuBanHang_ChiTiet", "KT_TaiKhoanKeToan", "KT_NhatKyChung" };
            var result = new Dictionary<string, List<object>>();
            
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                foreach(var tbl in tables)
                {
                    var cols = new List<object>();
                    string sql = "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tbl ORDER BY ORDINAL_POSITION";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@tbl", tbl);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                cols.Add(new {
                                    Name = reader["COLUMN_NAME"].ToString(),
                                    Type = reader["DATA_TYPE"].ToString(),
                                    Length = reader["CHARACTER_MAXIMUM_LENGTH"],
                                    Nullable = reader["IS_NULLABLE"].ToString()
                                });
                            }
                        }
                    }
                    result[tbl] = cols;
                }
            }
            context.Response.Write(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public bool IsReusable => false;
    }
}
