<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="SalesManagementSystem.Helpers.Security" %>
<%
    string result = "<table border='1'><tr><th>SoDonHang</th><th>TrangThaiDon</th><th>SoChungTu</th><th>TrangThaiCT</th><th>TongCong</th><th>DaThanhToan</th><th>ConLai</th></tr>";
    try {
        string connStr = ConfigManager.GetConnectionString("DefaultConnection");
        using (var conn = new SqlConnection(connStr)) {
            conn.Open();
            using (var cmd = conn.CreateCommand()) {
                cmd.CommandText = @"
                    SELECT 
                        d.SoDonHang, d.TrangThaiDon, 
                        c.SoChungTu, c.TrangThai AS TrangThaiCT,
                        c.TongCong, c.DaThanhToan, c.ConLai
                    FROM NS_DonDatHang d
                    JOIN NS_KhachHang k ON d.IDKhachHang = k.ID
                    LEFT JOIN BAN_ChungTuBanHang c ON c.IDDonDatHang = d.ID
                    WHERE k.TenKhachHang LIKE N'%Gia Đạt%'
                ";
                using (var reader = cmd.ExecuteReader()) {
                    while(reader.Read()) {
                        result += $"<tr><td>{reader["SoDonHang"]}</td><td>{reader["TrangThaiDon"]}</td><td>{reader["SoChungTu"]}</td><td>{reader["TrangThaiCT"]}</td><td>{reader["TongCong"]}</td><td>{reader["DaThanhToan"]}</td><td>{reader["ConLai"]}</td></tr>";
                    }
                }
            }
        }
    } catch (Exception ex) {
        result += $"<tr><td colspan='7'>ERROR: {ex.Message}</td></tr>";
    }
    result += "</table>";
    Response.Write(result);
%>
