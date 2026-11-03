USE [SalesWarehouseDB]
GO

-- =====================================================================
-- Bổ sung "Phí vận chuyển" cho phiếu Trả hàng bán (công ty chịu,
-- được CỘNG vào số tiền "Cần phải hoàn khách").
--
-- Chỉ thêm 1 cột NULL vào bảng BAN_TraHangBan:
--   - KHÔNG sửa stored procedure nào (Insert/Update/GhiSo/Huy/GetList...)
--   - Các phiếu cũ có PhiVanChuyen = NULL (hiển thị là 0)
--   - sp_BAN_TraHangBan_GetById đang SELECT t.* nên tự trả về cột mới
--   - Ứng dụng ghi giá trị cột này bằng câu UPDATE riêng trong cùng
--     transaction lúc lưu phiếu (xem TraHangBanRepository)
--
-- Chạy script này TRƯỚC khi deploy bản code mới.
-- Script chạy lại nhiều lần vẫn an toàn.
-- =====================================================================
IF COL_LENGTH('BAN_TraHangBan', 'PhiVanChuyen') IS NULL
BEGIN
    ALTER TABLE BAN_TraHangBan ADD PhiVanChuyen DECIMAL(18,2) NULL;
END
GO
