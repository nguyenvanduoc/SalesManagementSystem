IF OBJECT_ID('sp_KT_PhieuThuChiTiet_DeleteByPhieuThu', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThuChiTiet_DeleteByPhieuThu;
GO
CREATE PROCEDURE sp_KT_PhieuThuChiTiet_DeleteByPhieuThu
    @IDPhieuThu INT
AS
BEGIN
    DELETE FROM KT_PhieuThuChiTiet WHERE IDPhieuThu = @IDPhieuThu;
END
GO

IF OBJECT_ID('sp_KT_PhieuThu_DieuChinhPhanBo', 'P') IS NOT NULL DROP PROC sp_KT_PhieuThu_DieuChinhPhanBo;
GO
CREATE PROCEDURE sp_KT_PhieuThu_DieuChinhPhanBo
    @ID INT,
    @DienGiai NVARCHAR(500),
    @NguoiCapNhat INT
AS
BEGIN
    UPDATE KT_PhieuThu 
    SET DienGiai = @DienGiai, 
        NguoiCapNhat = @NguoiCapNhat, 
        NgayCapNhat = GETDATE()
    WHERE ID = @ID;
END
GO
