<%@ Page Language="C#" %>
<%@ Import Namespace="SalesManagementSystem.Data" %>
<%@ Import Namespace="Dapper" %>
<%
    try {
        using(var conn = new DbConnectionFactory().CreateConnection()) {
            var chiTiets = conn.Query("SELECT TOP 5 IDDonDatHang, IDSanPham, SoLuong, DonGia, DonGiaBocXep, ThanhTienBocXep FROM NS_DonDatHangChiTiet ORDER BY ID DESC").ToList();
            Response.Write("<pre>");
            foreach(var ct in chiTiets) {
                Response.Write($"Don={ct.IDDonDatHang}, SP={ct.IDSanPham}, SL={ct.SoLuong}, DG={ct.DonGia}, DGBX={ct.DonGiaBocXep}, TTBX={ct.ThanhTienBocXep}\n");
            }
            Response.Write("</pre>");
        }
    } catch (Exception ex) {
        Response.Write(ex.ToString());
    }
%>
