<%@ WebHandler Language="C#" Class="SalesManagementSystem.CheckDates" %>

using System;
using System.Web;
using System.Data.SqlClient;
using SalesManagementSystem.Helpers.Security;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SalesManagementSystem
{
    public class CheckDates : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            string connStr = ConfigManager.GetConnectionString("DefaultConnection");
            var result = new Dictionary<string, object>();
            
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                
                // 1. BAN_ChungTuBanHang
                var bhList = new List<object>();
                using (var cmd = new SqlCommand("SELECT ID, SoChungTu, NgayChungTu, TrangThai, IsDeleted, TongCong, DaThanhToan FROM BAN_ChungTuBanHang", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bhList.Add(new {
                            ID = reader["ID"],
                            SoChungTu = reader["SoChungTu"].ToString(),
                            NgayChungTu = reader["NgayChungTu"].ToString(),
                            TrangThai = reader["TrangThai"],
                            IsDeleted = reader["IsDeleted"],
                            TongCong = reader["TongCong"],
                            DaThanhToan = reader["DaThanhToan"]
                        });
                    }
                }
                result["BAN_ChungTuBanHang"] = bhList;

                // 2. KHO_PhieuNhap
                var pnList = new List<object>();
                using (var cmd = new SqlCommand("SELECT ID, SoChungTu, NgayNhap, TrangThai, IsDeleted, TongCong FROM KHO_PhieuNhap", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pnList.Add(new {
                            ID = reader["ID"],
                            SoChungTu = reader["SoChungTu"].ToString(),
                            NgayNhap = reader["NgayNhap"].ToString(),
                            TrangThai = reader["TrangThai"],
                            IsDeleted = reader["IsDeleted"],
                            TongCong = reader["TongCong"]
                        });
                    }
                }
                result["KHO_PhieuNhap"] = pnList;

                // 3. NS_DonDatHang
                var dhList = new List<object>();
                using (var cmd = new SqlCommand("SELECT ID, SoDonHang, NgayTaoDon, TrangThaiDon, ThoiHanGiaoHang FROM NS_DonDatHang", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dhList.Add(new {
                            ID = reader["ID"],
                            SoDonHang = reader["SoDonHang"].ToString(),
                            NgayTaoDon = reader["NgayTaoDon"].ToString(),
                            TrangThaiDon = reader["TrangThaiDon"],
                            ThoiHanGiaoHang = reader["ThoiHanGiaoHang"].ToString()
                        });
                    }
                }
                result["NS_DonDatHang"] = dhList;

                // 4. BAN_PhieuThuKhachHang
                var pthList = new List<object>();
                using (var cmd = new SqlCommand("SELECT ID, SoPhieuThu, NgayThu, TrangThai, IsDeleted, SoTienThu FROM BAN_PhieuThuKhachHang", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pthList.Add(new {
                            ID = reader["ID"],
                            SoPhieuThu = reader["SoPhieuThu"].ToString(),
                            NgayThu = reader["NgayThu"].ToString(),
                            TrangThai = reader["TrangThai"],
                            IsDeleted = reader["IsDeleted"],
                            SoTienThu = reader["SoTienThu"]
                        });
                    }
                }
                result["BAN_PhieuThuKhachHang"] = pthList;

                // 5. KT_PhieuChi
                var pcList = new List<object>();
                using (var cmd = new SqlCommand("SELECT ID, SoPhieuChi, NgayChi, TrangThai, IsDeleted, SoTienChi FROM KT_PhieuChi", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pcList.Add(new {
                            ID = reader["ID"],
                            SoPhieuChi = reader["SoPhieuChi"].ToString(),
                            NgayChi = reader["NgayChi"].ToString(),
                            TrangThai = reader["TrangThai"],
                            IsDeleted = reader["IsDeleted"],
                            SoTienChi = reader["SoTienChi"]
                        });
                    }
                }
                result["KT_PhieuChi"] = pcList;
            }
            
            context.Response.Write(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        public bool IsReusable => false;
    }
}
