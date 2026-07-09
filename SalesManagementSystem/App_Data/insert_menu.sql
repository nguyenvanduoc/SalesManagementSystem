IF NOT EXISTS(SELECT 1 FROM ACL_ManHinh WHERE TenManHinh = N'Hao hụt hàng hóa')
BEGIN
    INSERT INTO ACL_ManHinh(TenManHinh, NhomChaManHinh, IsSuDung, STT)
    VALUES(N'Hao hụt hàng hóa', N'Kho Hàng', 1, 99);

    DECLARE @MaxID INT = SCOPE_IDENTITY();

    INSERT INTO ACL_Action(IDManHinh, TenAction, TenController, LoaiPhanQuyen)
    VALUES(@MaxID, 'Index', 'KHO_HaoHut', 1);
END
GO
