using System;
using System.IO;
using SalesManagementSystem.Data;
using Dapper;

namespace SalesManagementSystem
{
    class DumpSps
    {
        static void Main(string[] args)
        {
            try
            {
                var db = new DbConnectionFactory();
                using (var conn = db.CreateConnection())
                {
                    string[] spNames = new string[] {
                        "sp_KHO_PhieuNhap_GetList",
                        "sp_KHO_PhieuNhap_GetByID",
                        "sp_KHO_PhieuNhap_Save",
                        "sp_KHO_PhieuNhap_GhiSo",
                        "sp_KHO_PhieuNhap_Huy",
                        "sp_KHO_TonKho_GetByKhoSanPham",
                        "sp_KHO_TonKho_CheckChuyenKho"
                    };

                    using (StreamWriter sw = new StreamWriter("dump_sps.sql"))
                    {
                        foreach (var sp in spNames)
                        {
                            try
                            {
                                var sql = "SELECT OBJECT_DEFINITION(OBJECT_ID(@SpName))";
                                var content = conn.ExecuteScalar<string>(sql, new { SpName = sp });
                                sw.WriteLine("-- =========================================");
                                sw.WriteLine(string.Format("-- {0}", sp));
                                sw.WriteLine("-- =========================================");
                                sw.WriteLine(content);
                                sw.WriteLine("GO");
                                sw.WriteLine();
                            }
                            catch (Exception ex)
                            {
                                sw.WriteLine(string.Format("-- Error getting {0}: {1}", sp, ex.Message));
                            }
                        }
                    }
                    Console.WriteLine("Successfully dumped SPs to dump_sps.sql");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
            }
        }
    }
}
