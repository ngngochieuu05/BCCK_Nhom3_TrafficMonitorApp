-- =============================================
-- Script: Kích hoạt kết nối từ xa SQL Server
-- Chạy script này trên SQL Server (máy chủ)
-- =============================================

-- =============================================
-- BƯỚC 1: Kiểm tra cấu hình hiện tại
-- =============================================
PRINT '========================================';
PRINT 'KIỂM TRA CẤU HÌNH SQL SERVER';
PRINT '========================================';
PRINT '';

-- Kiểm tra SQL Server version
SELECT 
    SERVERPROPERTY('ProductVersion') AS Version,
    SERVERPROPERTY('ProductLevel') AS [Service Pack],
    SERVERPROPERTY('Edition') AS Edition,
    SERVERPROPERTY('ServerName') AS ServerName;

PRINT '';
PRINT '----------------------------------------';
PRINT 'Lưu ý:';
PRINT '1. Sau khi chạy script này, cần:';
PRINT '   - Mở SQL Server Configuration Manager';
PRINT '   - Bật TCP/IP Protocol';
PRINT '   - Mở port 1433 trong Firewall';
PRINT '   - Restart SQL Server service';
PRINT '';
PRINT '2. Kiểm tra địa chỉ IP máy chủ:';
PRINT '   - Chạy: ipconfig trong Command Prompt';
PRINT '   - Tìm IPv4 Address (ví dụ: 192.168.1.100)';
PRINT '';
PRINT '3. Test kết nối từ máy client:';
PRINT '   - Chạy: sqlcmd -S 192.168.1.100,1433 -U TrafficApp -P "Traffic@2024!Strong"';
PRINT '';
PRINT '========================================';
PRINT '';

-- =============================================
-- BƯỚC 2: Enable SQL Server Authentication (Mixed Mode)
-- =============================================
USE master;
GO

EXEC xp_instance_regwrite 
    N'HKEY_LOCAL_MACHINE', 
    N'Software\Microsoft\MSSQLServer\MSSQLServer',
    N'LoginMode', 
    REG_DWORD, 
    2;
GO

PRINT '✓ Đã bật Mixed Mode Authentication (Windows + SQL Server Auth)';
PRINT '⚠️ Cần restart SQL Server service để áp dụng!';
PRINT '';

-- =============================================
-- BƯỚC 3: Enable remote admin connections
-- =============================================
EXEC sp_configure 'remote admin connections', 1;
RECONFIGURE;
GO

PRINT '✓ Đã bật remote admin connections';
PRINT '';

-- =============================================
-- BƯỚC 4: Hiển thị thông tin network
-- =============================================
PRINT '========================================';
PRINT 'THÔNG TIN NETWORK';
PRINT '========================================';

EXEC xp_cmdshell 'ipconfig | findstr IPv4';
GO

PRINT '';
PRINT '========================================';
PRINT 'CÁC BƯỚC TIẾP THEO (QUAN TRỌNG!)';
PRINT '========================================';
PRINT '';
PRINT '1. MỞ SQL SERVER CONFIGURATION MANAGER:';
PRINT '   - Vào Start Menu → tìm "SQL Server Configuration Manager"';
PRINT '   - Hoặc chạy: SQLServerManager15.msc (SQL 2019)';
PRINT '';
PRINT '2. BẬT TCP/IP PROTOCOL:';
PRINT '   - Chọn: SQL Server Network Configuration';
PRINT '   - Chọn: Protocols for SQLEXPRESS';
PRINT '   - Click phải "TCP/IP" → Enable';
PRINT '   - Double-click "TCP/IP" → Tab "IP Addresses"';
PRINT '   - Tìm "IPAll" → đặt "TCP Port" = 1433';
PRINT '   - Click OK';
PRINT '';
PRINT '3. RESTART SQL SERVER SERVICE:';
PRINT '   - Chọn: SQL Server Services';
PRINT '   - Click phải "SQL Server (SQLEXPRESS)"';
PRINT '   - Chọn "Restart"';
PRINT '';
PRINT '4. MỞ PORT FIREWALL:';
PRINT '   - Mở Command Prompt as Administrator';
PRINT '   - Chạy lệnh:';
PRINT '     netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433';
PRINT '';
PRINT '5. TEST KẾT NỐI TỪ MÁY CLIENT:';
PRINT '   - Lấy địa chỉ IP máy chủ (ví dụ: 192.168.1.100)';
PRINT '   - Từ máy client, chạy:';
PRINT '     sqlcmd -S 192.168.1.100,1433 -U TrafficApp -P "Traffic@2024!Strong"';
PRINT '';
PRINT '========================================';
GO
