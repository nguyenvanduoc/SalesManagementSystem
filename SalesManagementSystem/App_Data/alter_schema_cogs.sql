IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'BAN_ChungTuBanHang_ChiTiet') AND name = 'DonGiaVon')
BEGIN
    ALTER TABLE BAN_ChungTuBanHang_ChiTiet ADD DonGiaVon DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'BAN_ChungTuBanHang_ChiTiet') AND name = 'ThanhTienVon')
BEGIN
    ALTER TABLE BAN_ChungTuBanHang_ChiTiet ADD ThanhTienVon DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'KHO_PhieuXuat_ChiTiet') AND name = 'DonGiaVon')
BEGIN
    ALTER TABLE KHO_PhieuXuat_ChiTiet ADD DonGiaVon DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'KHO_PhieuXuat_ChiTiet') AND name = 'ThanhTienVon')
BEGIN
    ALTER TABLE KHO_PhieuXuat_ChiTiet ADD ThanhTienVon DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'KHO_GiaoDichKho') AND name = 'DonGiaVon')
BEGIN
    ALTER TABLE KHO_GiaoDichKho ADD DonGiaVon DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'KHO_GiaoDichKho') AND name = 'ThanhTienVon')
BEGIN
    ALTER TABLE KHO_GiaoDichKho ADD ThanhTienVon DECIMAL(18,2) NULL;
END
GO
