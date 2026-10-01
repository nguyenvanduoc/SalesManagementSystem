-- Script xóa chứng từ trùng lặp BH00268 của đơn hàng DH26000268
BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Xóa giao dịch kho của phiếu xuất PX00268
    DELETE FROM KHO_GiaoDichKho 
    WHERE SoChungTu = 'PX00268' AND LoaiChungTu = 2;

    -- 2. Xóa bút toán nhật ký chung của BH00268
    DELETE FROM KT_NhatKyChung 
    WHERE SoChungTu = 'BH00268' OR (LoaiChungTu = 'BAN' AND IDChungTu = 274);

    -- 3. Xóa chi tiết phiếu xuất PX00268
    DELETE FROM KHO_PhieuXuat_ChiTiet 
    WHERE IDPhieuXuat = 268;

    -- 4. Xóa phiếu xuất kho PX00268
    DELETE FROM KHO_PhieuXuat 
    WHERE ID = 268 AND SoChungTu = 'PX00268';

    -- 5. Xóa chi tiết chứng từ bán hàng BH00268
    DELETE FROM BAN_ChungTuBanHang_ChiTiet 
    WHERE IDChungTuBanHang = 274;

    -- 6. Xóa chứng từ bán hàng BH00268
    DELETE FROM BAN_ChungTuBanHang 
    WHERE ID = 274 AND SoChungTu = 'BH00268';

    -- Đảm bảo đơn đặt hàng ID = 271 giữ trạng thái 3 (Đã lập chứng từ) theo chứng từ hợp lệ BH00267 (ID = 273)
    UPDATE NS_DonDatHang 
    SET TrangThaiDon = 3 
    WHERE ID = 271;

    COMMIT TRANSACTION;
    PRINT N'Xóa thành công chứng từ BH00268 và hoàn tác các dữ liệu liên quan!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH
