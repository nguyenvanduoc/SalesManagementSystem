UPDATE pn
SET pn.DaThanhToan = ISNULL(pn.DaThanhToan, 0) - ct.SoTienPhanBo,
    pn.ConLai = ISNULL(pn.TongCong, 0) - (ISNULL(pn.DaThanhToan, 0) - ct.SoTienPhanBo),
    pn.TrangThaiThanhToan = CASE 
        WHEN ISNULL(pn.TongCong, 0) - (ISNULL(pn.DaThanhToan, 0) - ct.SoTienPhanBo) <= 0 THEN 2 
        WHEN ISNULL(pn.DaThanhToan, 0) - ct.SoTienPhanBo <= 0 THEN 0 
        ELSE 1 
    END
FROM KHO_PhieuNhap pn
INNER JOIN KT_PhieuChiChiTiet ct ON pn.ID = ct.IDPhieuNhap
INNER JOIN KT_PhieuChi pc ON ct.IDPhieuChi = pc.ID
WHERE pc.TrangThai = 3 AND ct.LoaiChi = 1;
GO
