-- =============================================
-- Script: Tạo tài khoản SQL Server cho ứng dụng
-- Mục đích: Cho phép nhiều máy client kết nối
-- Chạy script này trên SQL Server Management Studio
-- =============================================

USE master;
GO

-- =============================================
-- BƯỚC 1: Tạo Login (Server Level)
-- =============================================
PRINT '----------------------------------------';
PRINT 'BƯỚC 1: Tạo SQL Server Login...';
PRINT '----------------------------------------';

-- Xóa login cũ nếu tồn tại
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = 'TrafficApp')
BEGIN
    DROP LOGIN TrafficApp;
    PRINT '✓ Đã xóa login cũ: TrafficApp';
END

-- Tạo login mới với mật khẩu mạnh
CREATE LOGIN TrafficApp 
WITH PASSWORD = 'Traffic@2024!Strong',
     DEFAULT_DATABASE = QuanLyGiaoThong,
     CHECK_POLICY = ON,
     CHECK_EXPIRATION = OFF;
GO

PRINT '✓ Đã tạo login: TrafficApp';
PRINT '';

-- =============================================
-- BƯỚC 2: Tạo User trong Database
-- =============================================
USE QuanLyGiaoThong;
GO

PRINT '----------------------------------------';
PRINT 'BƯỚC 2: Tạo User trong database...';
PRINT '----------------------------------------';

-- Xóa user cũ nếu tồn tại
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'TrafficApp')
BEGIN
    DROP USER TrafficApp;
    PRINT '✓ Đã xóa user cũ: TrafficApp';
END

-- Tạo user mới
CREATE USER TrafficApp FOR LOGIN TrafficApp;
GO

PRINT '✓ Đã tạo user: TrafficApp';
PRINT '';

-- =============================================
-- BƯỚC 3: Gán quyền cho User
-- =============================================
PRINT '----------------------------------------';
PRINT 'BƯỚC 3: Gán quyền cho user...';
PRINT '----------------------------------------';

-- Gán quyền db_datareader (đọc dữ liệu)
ALTER ROLE db_datareader ADD MEMBER TrafficApp;
PRINT '✓ Đã gán quyền: db_datareader (đọc dữ liệu)';

-- Gán quyền db_datawriter (ghi dữ liệu)
ALTER ROLE db_datawriter ADD MEMBER TrafficApp;
PRINT '✓ Đã gán quyền: db_datawriter (ghi dữ liệu)';

-- Gán quyền EXECUTE (chạy stored procedures)
GRANT EXECUTE TO TrafficApp;
PRINT '✓ Đã gán quyền: EXECUTE (chạy stored procedures)';

-- Gán quyền VIEW DEFINITION (xem cấu trúc bảng)
GRANT VIEW DEFINITION TO TrafficApp;
PRINT '✓ Đã gán quyền: VIEW DEFINITION (xem cấu trúc)';

PRINT '';

-- =============================================
-- BƯỚC 4: Kiểm tra quyền
-- =============================================
PRINT '----------------------------------------';
PRINT 'BƯỚC 4: Kiểm tra quyền của user...';
PRINT '----------------------------------------';

SELECT 
    'TrafficApp' AS [User],
    CASE 
        WHEN IS_MEMBER('db_datareader') = 1 THEN 'CÓ' 
        ELSE 'KHÔNG' 
    END AS [Quyền_Đọc],
    CASE 
        WHEN IS_MEMBER('db_datawriter') = 1 THEN 'CÓ' 
        ELSE 'KHÔNG' 
    END AS [Quyền_Ghi],
    HAS_PERMS_BY_NAME(NULL, NULL, 'EXECUTE') AS [Quyền_Execute];
GO

PRINT '';
PRINT '========================================';
PRINT 'HOÀN TẤT!';
PRINT '========================================';
PRINT '';
PRINT 'Thông tin tài khoản:';
PRINT '  - Login: TrafficApp';
PRINT '  - Password: Traffic@2024!Strong';
PRINT '  - Database: QuanLyGiaoThong';
PRINT '';
PRINT 'Connection String mẫu:';
PRINT 'Server=.\SQLEXPRESS;Database=QuanLyGiaoThong;User Id=TrafficApp;Password=Traffic@2024!Strong;TrustServerCertificate=True;MultipleActiveResultSets=true';
PRINT '';
PRINT '⚠️ LƯU Ý BẢO MẬT:';
PRINT '  1. Đổi mật khẩu trước khi triển khai thực tế';
PRINT '  2. Không chia sẻ thông tin login công khai';
PRINT '  3. Sử dụng mật khẩu mạnh (ít nhất 12 ký tự)';
PRINT '';
GO
