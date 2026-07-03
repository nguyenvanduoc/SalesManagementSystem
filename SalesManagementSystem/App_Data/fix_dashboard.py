import re

path = r'c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\create_dashboard_stored_procedure.sql'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the specific subquery with pd.DaThanhToan
old_subquery = r"ISNULL\(\(SELECT SUM\(pc2\.SoTienChi\) FROM KT_PhieuChi pc2 WHERE pc2\.IDPhieuNhap = pn\.ID AND pc2\.TrangThai = 2 AND pc2\.IsDeleted = 0\), 0\)"
content = re.sub(old_subquery, "ISNULL(pd.DaThanhToan, 0)", content)

# Also need to add the CTE/Temp table logic at the top of the SP, right after SET NOCOUNT ON;
cte_logic = """
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
"""

if "#PaidNCC" not in content:
    content = content.replace("SET NOCOUNT ON;", "SET NOCOUNT ON;\n" + cte_logic)

# Replace 'FROM KHO_PhieuNhap pn' with 'FROM KHO_PhieuNhap pn LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap' in relevant queries
# First block
content = content.replace(
"""        FROM KHO_PhieuNhap pn
        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay""",
"""        FROM KHO_PhieuNhap pn
        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay""")

# Second block (11. Công nợ NCC quá hạn)
content = content.replace(
"""        FROM KHO_PhieuNhap pn
        JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
        WHERE pn.IsDeleted = 0""",
"""        FROM KHO_PhieuNhap pn
        JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
        WHERE pn.IsDeleted = 0""")

# Third block (12. List NCC)
content = content.replace(
"""    FROM KHO_PhieuNhap pn
    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    WHERE pn.IsDeleted = 0""",
"""    FROM KHO_PhieuNhap pn
    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
    WHERE pn.IsDeleted = 0""")

# Block 15 (TopNhaCungCap)
content = content.replace(
"""    -- 15. TopNhaCungCap
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
    WHERE pn.IsDeleted = 0""",
"""    -- 15. TopNhaCungCap
    SELECT TOP 10 ncc.TenNhaCungCap AS TenDoiTuong,
           SUM(pn.TongCong) AS DoanhThuHoacGiaTriNhap,
           SUM(pn.TongCong - ISNULL(pd.DaThanhToan, 0)) AS CongNo
    FROM KHO_PhieuNhap pn
    JOIN DM_NhaCungCap ncc ON pn.IDNhaCungCap = ncc.ID
    LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
    WHERE pn.IsDeleted = 0""")

# Use TongCong instead of TongTienHang
content = content.replace("pn.TongTienHang", "pn.TongCong")

# Update first block calculation for DaThanhToan again because the old logic was very specific
content = re.sub(
r"""    SELECT 
        @TongTienHangNCC = ISNULL\(SUM\(TongTienHang\), 0\),
        @DaThanhToanNCC = ISNULL\(SUM\(DaThanhToan\), 0\)
    FROM \(
        SELECT 
            pn.TongCong AS TongTienHang,
            ISNULL\(
                \(SELECT SUM\(ct.SoTienPhanBo\)
                 FROM KT_PhieuChiChiTiet ct
                 INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
                 WHERE ct.IDPhieuNhap = pn.ID 
                   AND ct.LoaiChi = 1
                   AND pc.TrangThai = 2
                   AND pc.IsDeleted = 0\),
                0
            \) \+ ISNULL\(
                \(SELECT SUM\(pc2.SoTienChi\)
                 FROM KT_PhieuChi pc2
                 WHERE pc2.IDPhieuNhap = pn.ID
                   AND pc2.TrangThai = 2
                   AND pc2.IsDeleted = 0
                   AND NOT EXISTS \(SELECT 1 FROM KT_PhieuChiChiTiet ct WHERE ct.IDPhieuChi = pc2.ID\)
                \),
                0
            \) AS DaThanhToan
        FROM KHO_PhieuNhap pn
        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay
    \) t;""",
"""    SELECT 
        @TongTienHangNCC = ISNULL(SUM(TongTienHang), 0),
        @DaThanhToanNCC = ISNULL(SUM(DaThanhToan), 0)
    FROM (
        SELECT 
            pn.TongCong AS TongTienHang,
            ISNULL(pd.DaThanhToan, 0) AS DaThanhToan
        FROM KHO_PhieuNhap pn
        LEFT JOIN #PaidNCC pd ON pn.ID = pd.IDPhieuNhap
        WHERE pn.IsDeleted = 0 AND pn.NgayNhap <= @DenNgay
    ) t;""", content)


with open(path, 'w', encoding='utf-8') as f:
    f.write(content)
