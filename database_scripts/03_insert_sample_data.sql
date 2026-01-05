-- =============================================
-- Traffic Monitor Database - Sample Data
-- Dữ Liệu Mẫu Cho Database Giám Sát Giao Thông
-- Insert test data for development/testing
-- Thêm dữ liệu test để phát triển và kiểm thử
-- =============================================

USE TrafficMonitorDb;
GO

-- =============================================
-- Insert Sample Sessions
-- Thêm Các Phiên Giám Sát Mẫu
-- =============================================
DECLARE @Session1 INT, @Session2 INT; -- Biến lưu ID của 2 session

-- Session 1: Video processing
-- Phiên 1: Xử lý từ video
INSERT INTO TrafficSessions (
    StartTime,              -- Thời gian bắt đầu
    EndTime,                -- Thời gian kết thúc
    SourceType,             -- Loại nguồn (video/camera/image)
    SourcePath,             -- Đường dẫn file/camera
    ModelPath,              -- Đường dẫn model YOLO
    ConfidenceThreshold,    -- Ngưỡng độ tin cậy (0.25 = 25%)
    IouThreshold,           -- Ngưỡng IoU để loại bỏ duplicate
    TotalVehicles,          -- Tổng số xe phát hiện được
    ProcessedFrames,        -- Số frame đã xử lý
    ProcessingTime,         -- Thời gian xử lý (giây)
    AverageFPS              -- FPS trung bình
)
VALUES (
    DATEADD(HOUR, -2, GETDATE()),  -- Bắt đầu 2 giờ trước
    DATEADD(HOUR, -1, GETDATE()),  -- Kết thúc 1 giờ trước
    'video',
    'D:\Videos\traffic_sample.mp4',
    'D:\Models\yolov8n.pt',
    0.25,    -- Confidence 25%
    0.45,    -- IoU 45%
    125,     -- Phát hiện 125 xe
    1500,    -- Xử lý 1500 frames
    180.5,   -- Mất 180.5 giây
    28.3     -- Đạt 28.3 FPS
);
SET @Session1 = SCOPE_IDENTITY(); -- Lấy ID của session vừa tạo

-- Session 2: Camera processing
-- Phiên 2: Xử lý từ camera
INSERT INTO TrafficSessions (
    StartTime, EndTime, SourceType, SourcePath, ModelPath,
    ConfidenceThreshold, IouThreshold, TotalVehicles, ProcessedFrames, ProcessingTime, AverageFPS
)
VALUES (
    DATEADD(HOUR, -5, GETDATE()),  -- Bắt đầu 5 giờ trước
    DATEADD(HOUR, -4, GETDATE()),  -- Kết thúc 4 giờ trước
    'camera',
    'Camera 0',
    'D:\Models\yolov8n.pt',
    0.30,    -- Confidence 30% (cao hơn để giảm false positive)
    0.45,    -- IoU 45%
    89,      -- Phát hiện 89 xe
    2000,    -- Xử lý 2000 frames
    240.2,   -- Mất 240.2 giây
    30.1     -- Đạt 30.1 FPS
);
SET @Session2 = SCOPE_IDENTITY(); -- Lấy ID của session vừa tạo

PRINT 'Inserted ' + CAST(@Session1 AS VARCHAR) + ' sessions';
PRINT 'Đã thêm ' + CAST(@Session1 AS VARCHAR) + ' phiên giám sát';
GO

-- =============================================
-- Insert Sample Vehicle Detections
-- Thêm Dữ Liệu Phát Hiện Xe Mẫu
-- =============================================
DECLARE @SessionId INT = 1; -- Sử dụng session đầu tiên (ID = 1)

-- Cars - Ô tô
-- Thêm 5 lần phát hiện ô tô với confidence cao (88-95%)
INSERT INTO VehicleDetections (
    SessionId,      -- ID phiên
    DetectedTime,   -- Thời điểm phát hiện
    TrackerId,      -- ID tracker (theo dõi xe qua các frame)
    VehicleType,    -- Loại xe
    Confidence,     -- Độ tin cậy (0-1)
    PositionX,      -- Tọa độ X (pixel)
    PositionY,      -- Tọa độ Y (pixel)
    Width,          -- Chiều rộng bounding box
    Height,         -- Chiều cao bounding box
    FrameNumber     -- Số thứ tự frame
)
VALUES 
    (@SessionId, DATEADD(MINUTE, -115, GETDATE()), 1, 'car', 0.92, 120, 340, 80, 60, 10),   -- Ô tô 1: Confidence 92%
    (@SessionId, DATEADD(MINUTE, -114, GETDATE()), 1, 'car', 0.93, 125, 345, 82, 62, 50),   -- Ô tô 1: Di chuyển (frame 50)
    (@SessionId, DATEADD(MINUTE, -113, GETDATE()), 2, 'car', 0.88, 450, 220, 75, 55, 120),  -- Ô tô 2: Vị trí khác
    (@SessionId, DATEADD(MINUTE, -112, GETDATE()), 3, 'car', 0.95, 300, 280, 85, 65, 180),  -- Ô tô 3: Confidence 95%
    (@SessionId, DATEADD(MINUTE, -111, GETDATE()), 4, 'car', 0.90, 200, 310, 78, 58, 240);  -- Ô tô 4: Confidence 90%

-- Motorcycles - Xe máy
-- Thêm 3 lần phát hiện xe máy với confidence trung bình (83-87%)
INSERT INTO VehicleDetections (SessionId, DetectedTime, TrackerId, VehicleType, Confidence, PositionX, PositionY, Width, Height, FrameNumber)
VALUES 
    (@SessionId, DATEADD(MINUTE, -110, GETDATE()), 5, 'motorcycle', 0.85, 180, 360, 45, 50, 300),  -- Xe máy 1
    (@SessionId, DATEADD(MINUTE, -109, GETDATE()), 6, 'motorcycle', 0.87, 350, 250, 42, 48, 360),  -- Xe máy 2
    (@SessionId, DATEADD(MINUTE, -108, GETDATE()), 7, 'motorcycle', 0.83, 420, 290, 44, 49, 420);  -- Xe máy 3

-- Buses - Xe buýt
-- Thêm 2 lần phát hiện xe buýt với confidence rất cao (94-96%)
INSERT INTO VehicleDetections (SessionId, DetectedTime, TrackerId, VehicleType, Confidence, PositionX, PositionY, Width, Height, FrameNumber)
VALUES 
    (@SessionId, DATEADD(MINUTE, -107, GETDATE()), 8, 'bus', 0.96, 250, 200, 120, 90, 480),  -- Xe buýt 1: Lớn (120x90)
    (@SessionId, DATEADD(MINUTE, -106, GETDATE()), 9, 'bus', 0.94, 150, 180, 125, 95, 540);  -- Xe buýt 2: Lớn (125x95)

-- Bicycles - Xe đạp
-- Thêm 2 lần phát hiện xe đạp với confidence thấp hơn (78-82%)
INSERT INTO VehicleDetections (SessionId, DetectedTime, TrackerId, VehicleType, Confidence, PositionX, PositionY, Width, Height, FrameNumber)
VALUES 
    (@SessionId, DATEADD(MINUTE, -105, GETDATE()), 10, 'bicycle', 0.78, 100, 380, 35, 40, 600),  -- Xe đạp 1: Nhỏ (35x40)
    (@SessionId, DATEADD(MINUTE, -104, GETDATE()), 11, 'bicycle', 0.82, 500, 320, 38, 42, 660);  -- Xe đạp 2: Nhỏ (38x42)

PRINT 'Inserted sample vehicle detections';
PRINT 'Đã thêm dữ liệu phát hiện xe mẫu: 5 ô tô, 3 xe máy, 2 xe buýt, 2 xe đạp';
GO

-- =============================================
-- Insert Sample Hourly Statistics
-- Thêm Thống Kê Theo Giờ Mẫu
-- =============================================
DECLARE @HourStart DATETIME2 = DATEADD(HOUR, DATEDIFF(HOUR, 0, GETDATE()), 0); -- Làm tròn giờ hiện tại

-- Thêm thống kê 6 giờ gần nhất với mức tắc nghẽn tăng dần
INSERT INTO HourlyStatistics (
    HourTimestamp,      -- Mốc thời gian (làm tròn giờ)
    TotalVehicles,      -- Tổng số xe
    CarCount,           -- Số ô tô
    MotorcycleCount,    -- Số xe máy
    BusCount,           -- Số xe buýt
    BicycleCount,       -- Số xe đạp
    AverageSpeed,       -- Tốc độ TB (km/h)
    CongestionLevel     -- Mức tắc nghẽn (0-5)
)
VALUES 
    (DATEADD(HOUR, -5, @HourStart), 45, 25, 15, 3, 2, 45.5, 1),   -- 5 giờ trước: Thông thoáng (level 1)
    (DATEADD(HOUR, -4, @HourStart), 67, 38, 22, 5, 2, 42.3, 2),   -- 4 giờ trước: Bắt đầu đông (level 2)
    (DATEADD(HOUR, -3, @HourStart), 89, 50, 30, 6, 3, 38.7, 3),   -- 3 giờ trước: Đông vừa (level 3)
    (DATEADD(HOUR, -2, @HourStart), 125, 70, 45, 7, 3, 32.5, 4),  -- 2 giờ trước: Đông (level 4)
    (DATEADD(HOUR, -1, @HourStart), 156, 90, 55, 8, 3, 25.2, 5),  -- 1 giờ trước: Tắc nghẽn nặng (level 5)
    (@HourStart, 98, 55, 35, 5, 3, 40.8, 3);                      -- Giờ hiện tại: Giảm xuống (level 3)

PRINT 'Inserted sample hourly statistics';
PRINT 'Đã thêm thống kê theo giờ: 6 giờ gần nhất với mức tắc nghẽn từ 1-5';
GO

-- =============================================
-- Verify Sample Data
-- =============================================
PRINT '';
PRINT 'Sample Data Summary:';
PRINT '--------------------';

SELECT 'TrafficSessions' AS TableName, COUNT(*) AS RecordCount FROM TrafficSessions
UNION ALL
SELECT 'VehicleDetections', COUNT(*) FROM VehicleDetections
UNION ALL
SELECT 'HourlyStatistics', COUNT(*) FROM HourlyStatistics;
GO