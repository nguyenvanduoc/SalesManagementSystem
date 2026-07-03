ALTER PROCEDURE [dbo].[sp_KT_PhieuChi_Delete]
    @ID int,
    @NguoiXoa int
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE KT_PhieuChi 
    SET IsDeleted = 1
    WHERE ID = @ID;
    
    DELETE FROM KT_PhieuChiChiTiet WHERE IDPhieuChi = @ID;
END
GO
