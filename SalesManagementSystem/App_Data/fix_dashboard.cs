using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\create_dashboard_stored_procedure.sql";
        string content = File.ReadAllText(path);

        string oldSubquery = @"ISNULL\(\(SELECT SUM\(pc2\.SoTienChi\) FROM KT_PhieuChi pc2 WHERE pc2\.IDPhieuNhap = pn\.ID AND pc2\.TrangThai = 2 AND pc2\.IsDeleted = 0\), 0\)";
        content = Regex.Replace(content, oldSubquery, "ISNULL(pd.DaThanhToan, 0)");

        string cteLogic = @"
    -- Tính DaThanhToan cho tất cả phiếu nhập
    SELECT 
        pn.ID AS IDPhieuNhap,
        ISNULL(
            (SELECT SUM(ct.SoTienPhanBo)
             FROM KT_PhieuChiChiTiet ct
             INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
             WHERE ct.IDPhieuNhap = pn.ID 
               AND ct.LoaiChi = 1
               AND pc.TrangThai = 2
               AND pc.IsDeleted = 0),
            0
        ) + ISNULL(
            (SELECT SUM(pc2.SoTienChi)
             FROM KT_PhieuChi pc2
             WHERE pc2.IDPhieuNhap = pn.ID
               AND pc2.TrangThai = 2
               AND pc2.IsDeleted = 0
               AND NOT EXISTS (SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc2.ID)
            ),
            0
        ) AS DaThanhToan
    INTO #PaidNCC
    FROM KHO_PhieuNhap pn
    WHERE pn.IsDeleted = 0;
";

        if (!content.Contains("#PaidNCC"))
        {
            content = content.Replace("SET NOCOUNT ON;", "SET NOCOUNT ON;\n" + cteLogic);
        }

        content = content.Replace(
            "FROM KHO_PhieuNhap pn\r\n        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay",
            "FROM KHO_PhieuNhap pn\r\n        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap\r\n        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay"
        );
        content = content.Replace(
            "FROM KHO_PhieuNhap pn\n        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay",
            "FROM KHO_PhieuNhap pn\n        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap\n        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay"
        );

        content = content.Replace(
            "FROM KHO_PhieuNhap pn\r\n        JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID\r\n        WHERE pn.IsDeleted = 0",
            "FROM KHO_PhieuNhap pn\r\n        JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID\r\n        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap\r\n        WHERE pn.IsDeleted = 0"
        );
        content = content.Replace(
            "FROM KHO_PhieuNhap pn\n        JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID\n        WHERE pn.IsDeleted = 0",
            "FROM KHO_PhieuNhap pn\n        JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID\n        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap\n        WHERE pn.IsDeleted = 0"
        );

        content = content.Replace(
            "    FROM KHO_PhieuNhap pn\r\n    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID\r\n    WHERE pn.IsDeleted = 0",
            "    FROM KHO_PhieuNhap pn\r\n    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID\r\n    LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap\r\n    WHERE pn.IsDeleted = 0"
        );
        content = content.Replace(
            "    FROM KHO_PhieuNhap pn\n    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID\n    WHERE pn.IsDeleted = 0",
            "    FROM KHO_PhieuNhap pn\n    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID\n    LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap\n    WHERE pn.IsDeleted = 0"
        );

        string oldBlock15 = @"    -- 15. TopNhaCungCap
    WITH NccPaid AS (
        SELECT pc2.IDPhieuNhap, SUM(pc2.SoTienChi) AS DaThanhToan
        FROM KT_PhieuChi pc2
        WHERE pc2.TrangThai = 2 AND pc2.IsDeleted = 0
        GROUP BY pc2.IDPhieuNhap
    )
    SELECT TOP 10 ncc.TenNhaCungCap AS TenDoiTuong,
           SUM(pn.TongTienHang) AS DoanhThuHoacGiaTriNhap,
           SUM(pn.TongTienHang - ISNULL(np.DaThanhToan, 0)) AS CongNo
    FROM KHO_PhieuNhap pn
    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN NccPaid np ON pn.ID = np.IDPhieuNhap
    WHERE pn.IsDeleted = 0";
        string newBlock15 = @"    -- 15. TopNhaCungCap
    SELECT TOP 10 ncc.TenNhaCungCap AS TenDoiTuong,
           SUM(pn.TongCong) AS DoanhThuHoacGiaTriNhap,
           SUM(pn.TongCong - ISNULL(pd.DaThanhToan, 0)) AS CongNo
    FROM KHO_PhieuNhap pn
    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
    WHERE pn.IsDeleted = 0";

        // Remove block 15 first just in case
        content = content.Replace(oldBlock15, newBlock15);
        content = content.Replace(oldBlock15.Replace("\r\n", "\n"), newBlock15.Replace("\r\n", "\n"));

        content = content.Replace("pn.TongTienHang", "pn.TongCong");

        // The first block logic replacement might be needed if not fully matched by Regex earlier.
        // Let's explicitly replace it using Regex to handle whitespace
        string patternFirstBlock = @"SELECT\s+@TongTienHangNCC\s*=\s*ISNULL\(SUM\(TongTienHang\),\s*0\),\s*@DaThanhToanNCC\s*=\s*ISNULL\(SUM\(DaThanhToan\),\s*0\)\s*FROM\s*\(\s*SELECT\s+pn\.TongCong\s+AS\s+TongTienHang,\s*ISNULL\(\s*\(\s*SELECT\s+SUM\(ct\.SoTienPhanBo\)\s*FROM\s+KT_PhieuChiChiTiet\s+ct\s*INNER\s+JOIN\s+KT_PhieuChi\s+pc\s+ON\s+ct\.IDPhieuChi\s*=\s*pc\.ID\s*WHERE\s+ct\.IDPhieuNhap\s*=\s*pn\.ID\s*AND\s+ct\.LoaiChi\s*=\s*1\s*AND\s+pc\.TrangThai\s*=\s*2\s*AND\s+pc\.IsDeleted\s*=\s*0\s*\),\s*0\s*\)\s*\+\s*ISNULL\(\s*\(\s*SELECT\s+SUM\(pc2\.SoTienChi\)\s*FROM\s+KT_PhieuChi\s+pc2\s*WHERE\s+pc2\.IDPhieuNhap\s*=\s*pn\.ID\s*AND\s+pc2\.TrangThai\s*=\s*2\s*AND\s+pc2\.IsDeleted\s*=\s*0\s*AND\s+NOT\s+EXISTS\s*\(\s*SELECT\s+1\s+FROM\s+KT_PhieuChiChiTiet\s+ct\s+WHERE\s+ct\.IDPhieuChi\s*=\s*pc2\.ID\s*\)\s*\),\s*0\s*\)\s*AS\s+DaThanhToan\s*FROM\s+KHO_PhieuNhap\s+pn\s*WHERE\s+pn\.IsDeleted\s*=\s*0\s*AND\s+pn\.NgayNhap\s*<=\s*@DenNgay\s*\)\s*t;";
        
        string newFirstBlock = @"    SELECT 
        @TongTienHangNCC = ISNULL(SUM(TongTienHang), 0),
        @DaThanhToanNCC = ISNULL(SUM(DaThanhToan), 0)
    FROM (
        SELECT 
            pn.TongCong AS TongTienHang,
            ISNULL(pd.DaThanhToan, 0) AS DaThanhToan
        FROM KHO_PhieuNhap pn
        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay
    ) t;";

        content = Regex.Replace(content, patternFirstBlock, newFirstBlock);

        File.WriteAllText(path, content);
        Console.WriteLine("Done.");
    }
}
