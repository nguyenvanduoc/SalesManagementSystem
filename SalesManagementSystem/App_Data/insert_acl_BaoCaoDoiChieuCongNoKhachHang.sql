IF NOT EXISTS (SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Báo cáo đối chiếu KH')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung)
    VALUES (N'Báo cáo đối chiếu KH', N'Báo cáo', 1);

    DECLARE @IDManHinh INT = SCOPE_IDENTITY();

    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
    VALUES (@IDManHinh, 'Index', 'BaoCaoDoiChieuCongNoKhachHang', 1, N'Xem báo cáo');
END
GO
