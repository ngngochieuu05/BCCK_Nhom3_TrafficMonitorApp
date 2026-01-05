-- =====================================================
-- CREATE USERS TABLE FOR AUTHENTICATION
-- Tạo bảng Users để lưu thông tin đăng nhập
-- =====================================================

USE TrafficMonitorDb;
GO

-- Tạo bảng Users nếu chưa tồn tại
-- Create Users table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId NVARCHAR(450) NOT NULL PRIMARY KEY,  -- GUID dạng string
        Username NVARCHAR(50) NOT NULL,             -- Tên đăng nhập (unique)
        PasswordHash NVARCHAR(256) NOT NULL,        -- Mật khẩu đã hash (SHA256)
        FullName NVARCHAR(100) NULL,                -- Họ và tên đầy đủ
        Email NVARCHAR(100) NULL,                   -- Email liên hệ
        Role NVARCHAR(20) NOT NULL DEFAULT 'User',  -- Vai trò: Admin, User
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(), -- Ngày tạo tài khoản
        LastLoginDate DATETIME2 NULL,               -- Đăng nhập gần nhất
        IsActive BIT NOT NULL DEFAULT 1             -- Trạng thái kích hoạt
    );

    -- Tạo unique constraint cho Username
    -- Create unique constraint for Username
    ALTER TABLE Users 
    ADD CONSTRAINT UQ_Users_Username UNIQUE (Username);

    -- Tạo index cho Role để tăng tốc query phân quyền
    -- Create index on Role for permission queries
    CREATE INDEX IX_Users_Role ON Users(Role);

    -- Tạo index cho IsActive để lọc user đang hoạt động
    -- Create index on IsActive for active user filtering
    CREATE INDEX IX_Users_IsActive ON Users(IsActive);

    PRINT 'Bảng Users đã được tạo thành công!'
END
ELSE
BEGIN
    PRINT 'Bảng Users đã tồn tại.'
END
GO

-- =====================================================
-- INSERT DEFAULT ADMIN ACCOUNT
-- Thêm tài khoản Admin mặc định
-- =====================================================

-- Kiểm tra xem đã có Admin chưa
-- Check if Admin account exists
IF NOT EXISTS (SELECT * FROM Users WHERE Role = 'Admin')
BEGIN
    -- Tạo Admin account: username=admin, password=admin
    -- Password hash của "admin" (SHA256): 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
    INSERT INTO Users (UserId, Username, PasswordHash, FullName, Email, Role, CreatedDate, LastLoginDate, IsActive)
    VALUES (
        NEWID(),  -- Generate new GUID
        'admin',  -- Username
        '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',  -- Password hash của "admin"
        'Administrator',  -- Full name
        'admin@trafficmonitor.com',  -- Email
        'Admin',  -- Role
        GETDATE(),  -- Created date
        NULL,  -- Last login date (chưa đăng nhập)
        1  -- Is active
    );

    PRINT 'Tài khoản Admin mặc định đã được tạo!'
    PRINT 'Username: admin'
    PRINT 'Password: admin'
END
ELSE
BEGIN
    PRINT 'Tài khoản Admin đã tồn tại.'
END
GO

-- =====================================================
-- SAMPLE USER ACCOUNTS (Optional)
-- Tài khoản User mẫu (Tùy chọn)
-- =====================================================

-- Uncomment để tạo thêm user mẫu
-- Uncomment to create sample users

/*
-- User 1: Password = "user123"
-- Hash SHA256 của "user123": ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae
INSERT INTO Users (UserId, Username, PasswordHash, FullName, Email, Role, CreatedDate, IsActive)
VALUES (
    NEWID(),
    'user1',
    'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae',
    'Nguyen Van A',
    'user1@example.com',
    'User',
    GETDATE(),
    1
);

-- User 2: Password = "user456"
-- Hash SHA256 của "user456": 0b14d501a594442a01c6859541bcb3e8164d183d32937b851835442f69d5c94e
INSERT INTO Users (UserId, Username, PasswordHash, FullName, Email, Role, CreatedDate, IsActive)
VALUES (
    NEWID(),
    'user2',
    '0b14d501a594442a01c6859541bcb3e8164d183d32937b851835442f69d5c94e',
    'Tran Thi B',
    'user2@example.com',
    'User',
    GETDATE(),
    1
);

PRINT 'Đã tạo 2 tài khoản User mẫu!'
*/
GO

-- =====================================================
-- VERIFY TABLE AND DATA
-- Kiểm tra bảng và dữ liệu
-- =====================================================

-- Hiển thị cấu trúc bảng
-- Show table structure
PRINT ''
PRINT '===== CẤU TRÚC BẢNG USERS ====='
SELECT 
    COLUMN_NAME AS [Tên Cột],
    DATA_TYPE AS [Kiểu Dữ Liệu],
    CHARACTER_MAXIMUM_LENGTH AS [Độ Dài],
    IS_NULLABLE AS [Nullable],
    COLUMN_DEFAULT AS [Giá Trị Mặc Định]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

-- Hiển thị danh sách users
-- Show list of users
PRINT ''
PRINT '===== DANH SÁCH USERS ====='
SELECT 
    Username AS [Tên Đăng Nhập],
    FullName AS [Họ Tên],
    Email,
    Role AS [Vai Trò],
    FORMAT(CreatedDate, 'dd/MM/yyyy HH:mm') AS [Ngày Tạo],
    CASE WHEN IsActive = 1 THEN N'Hoạt động' ELSE N'Khóa' END AS [Trạng Thái]
FROM Users
ORDER BY CreatedDate;

-- Hiển thị thống kê
-- Show statistics
PRINT ''
PRINT '===== THỐNG KÊ ====='
SELECT 
    COUNT(*) AS [Tổng Users],
    SUM(CASE WHEN Role = 'Admin' THEN 1 ELSE 0 END) AS [Số Admin],
    SUM(CASE WHEN Role = 'User' THEN 1 ELSE 0 END) AS [Số User],
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS [Đang Hoạt Động],
    SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) AS [Bị Khóa]
FROM Users;

PRINT ''
PRINT '===== HOÀN THÀNH ====='
PRINT 'Bảng Users đã sẵn sàng sử dụng!'
PRINT ''
PRINT 'Đăng nhập với tài khoản Admin:'
PRINT '  Username: admin'
PRINT '  Password: admin'
GO
