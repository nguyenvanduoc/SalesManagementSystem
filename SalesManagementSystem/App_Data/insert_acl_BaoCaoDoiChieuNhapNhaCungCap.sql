IF NOT EXISTS (SELECT * FROM ACL_ManHinh WHERE TenManHinh = N'Báo cáo đối chiếu nhập NCC')
BEGIN
    INSERT INTO ACL_ManHinh (TenManHinh, NhomChaManHinh, IsSuDung, GhiChu)
    VALUES (N'Báo cáo đối chiếu nhập NCC', N'Báo cáo', 1, N'Báo cáo đối chiếu nhập hàng theo nhà cung cấp');

    DECLARE @IDManHinh INT = SCOPE_IDENTITY();

    INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, MoTa)
    VALUES (@IDManHinh, 'Index', 'BaoCaoDoiChieuNhapNhaCungCap', 1, N'Xem báo cáo');
END
GO
