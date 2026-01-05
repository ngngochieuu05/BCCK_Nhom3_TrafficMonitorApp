-- =============================================
-- Traffic Monitor Database Setup Script (Tiếng Việt)
-- SQL Server (Full Version)
-- =============================================

USE master;
GO

-- Xóa database nếu tồn tại (để cài đặt mới)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'QuanLyGiaoThong')
BEGIN
    ALTER DATABASE QuanLyGiaoThong SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyGiaoThong;
END
GO

-- Tạo database mới
CREATE DATABASE QuanLyGiaoThong;
GO

USE QuanLyGiaoThong;
GO

-- =============================================
-- Bảng: PhienGiamSat
-- Lưu thông tin các phiên giám sát giao thông
-- =============================================
CREATE TABLE PhienGiamSat (
    MaPhien INT IDENTITY(1,1) PRIMARY KEY,
    ThoiGianBatDau DATETIME2 NOT NULL,
    ThoiGianKetThuc DATETIME2 NULL,
    LoaiNguon NVARCHAR(50) NOT NULL,
    DuongDanNguon NVARCHAR(500) NULL,
    DuongDanMoHinh NVARCHAR(500) NULL,
    NguyenDoTinCay FLOAT NOT NULL,
    NguyenIoU FLOAT NOT NULL,
    TongSoXe INT NOT NULL DEFAULT 0,
    SoKhungHinhXuLy INT NOT NULL DEFAULT 0,
    ThoiGianXuLy FLOAT NOT NULL DEFAULT 0,
    TocDoTrungBinh FLOAT NOT NULL DEFAULT 0,
    NgayTao DATETIME2 DEFAULT GETDATE()
);
GO

-- Index cho PhienGiamSat
CREATE INDEX IX_PhienGiamSat_ThoiGianBatDau ON PhienGiamSat(ThoiGianBatDau);
CREATE INDEX IX_PhienGiamSat_ThoiGianKetThuc ON PhienGiamSat(ThoiGianKetThuc);
CREATE INDEX IX_PhienGiamSat_LoaiNguon ON PhienGiamSat(LoaiNguon);
GO

-- =============================================
-- Bảng: PhatHienXe
-- Lưu chi tiết từng lần phát hiện xe
-- =============================================
CREATE TABLE PhatHienXe (
    MaPhatHien INT IDENTITY(1,1) PRIMARY KEY,
    MaPhien INT NOT NULL,
    ThoiGianPhatHien DATETIME2 NOT NULL,
    MaTheoDoiXe INT NOT NULL,
    LoaiXe NVARCHAR(50) NOT NULL,
    DoTinCay FLOAT NOT NULL,
    ToaDoX INT NOT NULL,
    ToaDoY INT NOT NULL,
    ChieuRong INT NOT NULL,
    ChieuCao INT NOT NULL,
    SoKhungHinh INT NOT NULL,
    
    CONSTRAINT FK_PhatHienXe_PhienGiamSat 
        FOREIGN KEY (MaPhien) 
        REFERENCES PhienGiamSat(MaPhien) 
        ON DELETE CASCADE
);
GO

-- Index cho PhatHienXe
CREATE INDEX IX_PhatHienXe_MaPhien ON PhatHienXe(MaPhien);
CREATE INDEX IX_PhatHienXe_ThoiGianPhatHien ON PhatHienXe(ThoiGianPhatHien);
CREATE INDEX IX_PhatHienXe_LoaiXe ON PhatHienXe(LoaiXe);
CREATE INDEX IX_PhatHienXe_MaTheoDoiXe ON PhatHienXe(MaTheoDoiXe);
GO

-- =============================================
-- Bảng: ThongKeTheoGio
-- Thống kê giao thông theo từng giờ
-- =============================================
CREATE TABLE ThongKeTheoGio (
    MaThongKe INT IDENTITY(1,1) PRIMARY KEY,
    ThoiGianGio DATETIME2 NOT NULL,
    TongSoXe INT NOT NULL DEFAULT 0,
    SoXeOto INT NOT NULL DEFAULT 0,
    SoXeMay INT NOT NULL DEFAULT 0,
    SoXeBuyt INT NOT NULL DEFAULT 0,
    SoXeDap INT NOT NULL DEFAULT 0,
    TocDoTrungBinh FLOAT NOT NULL DEFAULT 0,
    MucDoTacNghen INT NOT NULL DEFAULT 0,
    
    CONSTRAINT UQ_ThongKeTheoGio_ThoiGian UNIQUE (ThoiGianGio)
);
GO

-- Index cho ThongKeTheoGio
CREATE INDEX IX_ThongKeTheoGio_ThoiGianGio ON ThongKeTheoGio(ThoiGianGio);
GO

-- =============================================
-- Bảng: NguoiDung
-- Quản lý tài khoản người dùng hệ thống
-- =============================================
CREATE TABLE NguoiDung (
    MaNguoiDung NVARCHAR(50) PRIMARY KEY,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
    MatKhau NVARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NULL,
    VaiTro NVARCHAR(20) NOT NULL,
    TrangThai BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME2 DEFAULT GETDATE(),
    LanDangNhapCuoi DATETIME2 NULL
);
GO

-- Index cho NguoiDung
CREATE INDEX IX_NguoiDung_TenDangNhap ON NguoiDung(TenDangNhap);
CREATE INDEX IX_NguoiDung_VaiTro ON NguoiDung(VaiTro);
GO

-- =============================================
-- Dữ liệu mẫu - Tài khoản Admin mặc định
-- =============================================
INSERT INTO NguoiDung (MaNguoiDung, TenDangNhap, MatKhau, HoTen, Email, VaiTro, TrangThai, NgayTao)
VALUES 
    ('ADMIN001', 'admin', 'admin123', N'Quản Trị Viên', 'admin@traffic.com', 'Admin', 1, GETDATE()),
    ('USER001', 'user', 'user123', N'Người Dùng', 'user@traffic.com', 'User', 1, GETDATE());
GO

-- =============================================
-- View: ThongKeTongQuat
-- Tổng hợp thống kê tổng quan
-- =============================================
CREATE VIEW ThongKeTongQuat AS
SELECT 
    COUNT(DISTINCT ps.MaPhien) AS TongSoPhien,
    COUNT(px.MaPhatHien) AS TongSoPhatHien,
    SUM(CASE WHEN px.LoaiXe = 'car' THEN 1 ELSE 0 END) AS TongXeOto,
    SUM(CASE WHEN px.LoaiXe = 'motorcycle' THEN 1 ELSE 0 END) AS TongXeMay,
    SUM(CASE WHEN px.LoaiXe = 'bus' THEN 1 ELSE 0 END) AS TongXeBuyt,
    SUM(CASE WHEN px.LoaiXe = 'bicycle' THEN 1 ELSE 0 END) AS TongXeDap,
    AVG(ps.TocDoTrungBinh) AS TocDoTrungBinhChung,
    MAX(ps.ThoiGianKetThuc) AS PhienGanNhat
FROM PhienGiamSat ps
LEFT JOIN PhatHienXe px ON ps.MaPhien = px.MaPhien
WHERE ps.ThoiGianKetThuc IS NOT NULL;
GO

-- =============================================
-- Stored Procedure: ThongKeTheoNgay
-- Thống kê chi tiết theo ngày
-- =============================================
CREATE PROCEDURE ThongKeTheoNgay
    @NgayBatDau DATETIME2,
    @NgayKetThuc DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CAST(ps.ThoiGianBatDau AS DATE) AS Ngay,
        COUNT(DISTINCT ps.MaPhien) AS SoPhien,
        COUNT(px.MaPhatHien) AS SoPhatHien,
        SUM(CASE WHEN px.LoaiXe = 'car' THEN 1 ELSE 0 END) AS SoXeOto,
        SUM(CASE WHEN px.LoaiXe = 'motorcycle' THEN 1 ELSE 0 END) AS SoXeMay,
        SUM(CASE WHEN px.LoaiXe = 'bus' THEN 1 ELSE 0 END) AS SoXeBuyt,
        SUM(CASE WHEN px.LoaiXe = 'bicycle' THEN 1 ELSE 0 END) AS SoXeDap,
        AVG(ps.TocDoTrungBinh) AS TocDoTrungBinh
    FROM PhienGiamSat ps
    LEFT JOIN PhatHienXe px ON ps.MaPhien = px.MaPhien
    WHERE ps.ThoiGianBatDau >= @NgayBatDau 
        AND ps.ThoiGianBatDau < DATEADD(DAY, 1, @NgayKetThuc)
    GROUP BY CAST(ps.ThoiGianBatDau AS DATE)
    ORDER BY Ngay DESC;
END
GO

-- =============================================
-- Stored Procedure: ThongKeTheoLoaiXe
-- Thống kê theo từng loại xe
-- =============================================
CREATE PROCEDURE ThongKeTheoLoaiXe
    @MaPhien INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        LoaiXe,
        COUNT(*) AS SoLuong,
        AVG(DoTinCay) AS DoTinCayTrungBinh,
        MIN(ThoiGianPhatHien) AS LanPhatHienDau,
        MAX(ThoiGianPhatHien) AS LanPhatHienCuoi
    FROM PhatHienXe
    WHERE MaPhien = @MaPhien
    GROUP BY LoaiXe
    ORDER BY SoLuong DESC;
END
GO

PRINT '✓ Database QuanLyGiaoThong đã được tạo thành công với tên tiếng Việt!';
PRINT '✓ Tài khoản mặc định:';
PRINT '  - Admin: admin / admin123';
PRINT '  - User: user / user123';
GO
