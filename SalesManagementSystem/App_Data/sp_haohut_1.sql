CREATE PROCEDURE sp_KHO_HaoHutHangHoa_GetList
    @TuNgay DATETIME,
    @DenNgay DATETIME,
    @LoaiHaoHut INT,
    @IDKho INT,
    @IDKhachHang INT,
    @SoChungTu NVARCHAR(50),
    @TrangThai INT,
    @Skip INT,
    @Take INT
AS
BEGIN
    SELECT h.*, 
           k.TenKhoHang AS TenKho,
           kh.TenKhachHang,
           d.SoDonHang,
           (SELECT COUNT(*) FROM KHO_HaoHutHangHoa h2 WHERE 
                h2.NgayHaoHut >= @TuNgay AND h2.NgayHaoHut <= @DenNgay
                AND (@LoaiHaoHut = 0 OR h2.LoaiHaoHut = @LoaiHaoHut)
                AND (@IDKho = 0 OR h2.IDKho = @IDKho)
                AND (@IDKhachHang = 0 OR h2.IDKhachHang = @IDKhachHang)
                AND (@SoChungTu IS NULL OR @SoChungTu = '' OR h2.SoChungTu LIKE '%' + @SoChungTu + '%')
                AND (@TrangThai = 0 OR h2.TrangThai = @TrangThai)
           ) AS TotalRecords
    FROM KHO_HaoHutHangHoa h
    LEFT JOIN DM_KhoHang k ON h.IDKho = k.ID
    LEFT JOIN NS_KhachHang kh ON h.IDKhachHang = kh.ID
    LEFT JOIN NS_DonDatHang d ON h.IDDonHang = d.ID
    WHERE h.NgayHaoHut >= @TuNgay AND h.NgayHaoHut <= @DenNgay
      AND (@LoaiHaoHut = 0 OR h.LoaiHaoHut = @LoaiHaoHut)
      AND (@IDKho = 0 OR h.IDKho = @IDKho)
      AND (@IDKhachHang = 0 OR h.IDKhachHang = @IDKhachHang)
      AND (@SoChungTu IS NULL OR @SoChungTu = '' OR h.SoChungTu LIKE '%' + @SoChungTu + '%')
      AND (@TrangThai = 0 OR h.TrangThai = @TrangThai)
    ORDER BY h.NgayHaoHut DESC, h.ID DESC
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
END
GO

CREATE PROCEDURE sp_KHO_HaoHutHangHoa_GetByID
    @ID INT
AS
BEGIN
    SELECT h.*, 
           k.TenKhoHang AS TenKho,
           kh.TenKhachHang,
           d.SoDonHang,
           c.SoChungTu AS SoChungTuBanHang
    FROM KHO_HaoHutHangHoa h
    LEFT JOIN DM_KhoHang k ON h.IDKho = k.ID
    LEFT JOIN NS_KhachHang kh ON h.IDKhachHang = kh.ID
    LEFT JOIN NS_DonDatHang d ON h.IDDonHang = d.ID
    LEFT JOIN BAN_ChungTuBanHang c ON h.IDChungTuBanHang = c.ID
    WHERE h.ID = @ID;

    SELECT c.*, s.MaSanPham, s.TenSanPham
    FROM KHO_HaoHutHangHoa_ChiTiet c
    INNER JOIN DM_SanPham s ON c.IDSanPham = s.ID
    WHERE c.IDHaoHut = @ID;
END
GO

CREATE PROCEDURE sp_KHO_HaoHutHangHoa_Insert
    @SoChungTu NVARCHAR(50) OUT,
    @NgayHaoHut DATETIME,
    @LoaiHaoHut INT,
    @IDKho INT,
    @IDDonHang INT,
    @IDChungTuBanHang INT,
    @IDKhachHang INT,
    @LyDo NVARCHAR(1000),
    @GhiChu NVARCHAR(1000),
    @NguoiTao INT,
    @ID INT OUT
AS
BEGIN
    -- Generate SoChungTu
    DECLARE @MaxID INT = ISNULL((SELECT MAX(ID) FROM KHO_HaoHutHangHoa), 0) + 1;
    SET @SoChungTu = 'HH' + RIGHT('000000' + CAST(@MaxID AS VARCHAR), 6);

    INSERT INTO KHO_HaoHutHangHoa(SoChungTu, NgayHaoHut, LoaiHaoHut, IDKho, IDDonHang, IDChungTuBanHang, IDKhachHang, LyDo, GhiChu, TongSoLuong, TongTienHaoHut, TrangThai, NgayTao, NguoiTao)
    VALUES(@SoChungTu, @NgayHaoHut, @LoaiHaoHut, @IDKho, @IDDonHang, @IDChungTuBanHang, @IDKhachHang, @LyDo, @GhiChu, 0, 0, 1, GETDATE(), @NguoiTao);

    SET @ID = SCOPE_IDENTITY();
END
GO

CREATE PROCEDURE sp_KHO_HaoHutHangHoa_Update
    @ID INT,
    @NgayHaoHut DATETIME,
    @LoaiHaoHut INT,
    @IDKho INT,
    @IDDonHang INT,
    @IDChungTuBanHang INT,
    @IDKhachHang INT,
    @LyDo NVARCHAR(1000),
    @GhiChu NVARCHAR(1000),
    @NguoiCapNhat INT
AS
BEGIN
    UPDATE KHO_HaoHutHangHoa
    SET NgayHaoHut = @NgayHaoHut,
        LoaiHaoHut = @LoaiHaoHut,
        IDKho = @IDKho,
        IDDonHang = @IDDonHang,
        IDChungTuBanHang = @IDChungTuBanHang,
        IDKhachHang = @IDKhachHang,
        LyDo = @LyDo,
        GhiChu = @GhiChu,
        NgayCapNhat = GETDATE(),
        NguoiCapNhat = @NguoiCapNhat
    WHERE ID = @ID AND TrangThai = 1; -- Only update if Draft
END
GO

CREATE PROCEDURE sp_KHO_HaoHutHangHoa_Delete
    @ID INT,
    @NguoiHuy INT
AS
BEGIN
    -- Soft delete or hard delete if draft
    DELETE FROM KHO_HaoHutHangHoa_ChiTiet WHERE IDHaoHut = @ID AND (SELECT TrangThai FROM KHO_HaoHutHangHoa WHERE ID = @ID) = 1;
    DELETE FROM KHO_HaoHutHangHoa WHERE ID = @ID AND TrangThai = 1;
END
GO

CREATE PROCEDURE sp_KHO_HaoHutHangHoa_ChiTiet_DeleteByHaoHut
    @IDHaoHut INT
AS
BEGIN
    DELETE FROM KHO_HaoHutHangHoa_ChiTiet WHERE IDHaoHut = @IDHaoHut AND (SELECT TrangThai FROM KHO_HaoHutHangHoa WHERE ID = @IDHaoHut) = 1;
END
GO

CREATE PROCEDURE sp_KHO_HaoHutHangHoa_ChiTiet_Insert
    @IDHaoHut INT,
    @IDSanPham INT,
    @SoLuongHaoHut DECIMAL(18,2),
    @DonGiaHaoHut DECIMAL(18,2),
    @TienHaoHut DECIMAL(18,2),
    @DonGiaBan DECIMAL(18,2),
    @DoanhThuGiam DECIMAL(18,2),
    @GhiChu NVARCHAR(1000),
    @NguoiTao INT
AS
BEGIN
    INSERT INTO KHO_HaoHutHangHoa_ChiTiet(IDHaoHut, IDSanPham, SoLuongHaoHut, DonGiaHaoHut, TienHaoHut, DonGiaBan, DoanhThuGiam, GhiChu, NgayTao, NguoiTao)
    VALUES(@IDHaoHut, @IDSanPham, @SoLuongHaoHut, @DonGiaHaoHut, @TienHaoHut, @DonGiaBan, @DoanhThuGiam, @GhiChu, GETDATE(), @NguoiTao);
END
GO
