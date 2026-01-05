-- =============================================
-- Traffic Monitor Database - Sample Queries
-- =============================================

USE TrafficMonitorDb;
GO

-- =============================================
-- 1. VIEW ALL SESSIONS
-- =============================================
SELECT 
    SessionId,
    StartTime,
    EndTime,
    SourceType,
    SourcePath,
    TotalVehicles,
    ProcessedFrames,
    AverageFPS,
    CAST(ProcessingTime AS DECIMAL(10,2)) AS ProcessingTime_Seconds
FROM TrafficSessions
ORDER BY StartTime DESC;
GO

-- =============================================
-- 2. VIEW SESSION DETAILS WITH COUNTS
-- =============================================
SELECT 
    s.SessionId,
    s.StartTime,
    s.SourceType,
    s.TotalVehicles,
    COUNT(d.DetectionId) AS TotalDetections,
    s.AverageFPS,
    DATEDIFF(SECOND, s.StartTime, s.EndTime) AS Duration_Seconds
FROM TrafficSessions s
LEFT JOIN VehicleDetections d ON s.SessionId = d.SessionId
GROUP BY 
    s.SessionId, s.StartTime, s.SourceType, 
    s.TotalVehicles, s.AverageFPS, s.EndTime
ORDER BY s.StartTime DESC;
GO

-- =============================================
-- 3. VEHICLE COUNT BY TYPE (ALL SESSIONS)
-- =============================================
SELECT 
    VehicleType,
    COUNT(*) AS TotalDetections,
    CAST(AVG(Confidence) * 100 AS DECIMAL(5,2)) AS AvgConfidence_Percent
FROM VehicleDetections
GROUP BY VehicleType
ORDER BY TotalDetections DESC;
GO

-- =============================================
-- 4. VEHICLE COUNT BY TYPE (SPECIFIC SESSION)
-- =============================================
DECLARE @SessionId INT = 1; -- Change this

SELECT 
    VehicleType,
    COUNT(*) AS Count,
    CAST(AVG(Confidence) * 100 AS DECIMAL(5,2)) AS AvgConfidence_Percent
FROM VehicleDetections
WHERE SessionId = @SessionId
GROUP BY VehicleType
ORDER BY Count DESC;
GO

-- =============================================
-- 5. RECENT DETECTIONS (Last 50)
-- =============================================
SELECT TOP 50
    d.DetectionId,
    s.SourceType,
    d.DetectedTime,
    d.VehicleType,
    CAST(d.Confidence * 100 AS DECIMAL(5,2)) AS Confidence_Percent,
    d.PositionX,
    d.PositionY,
    d.FrameNumber
FROM VehicleDetections d
INNER JOIN TrafficSessions s ON d.SessionId = s.SessionId
ORDER BY d.DetectedTime DESC;
GO

-- =============================================
-- 6. HOURLY STATISTICS
-- =============================================
SELECT 
    HourTimestamp,
    TotalVehicles,
    CarCount,
    MotorcycleCount,
    BusCount,
    BicycleCount,
    CASE CongestionLevel
        WHEN 0 THEN 'No Congestion'
        WHEN 1 THEN 'Light'
        WHEN 2 THEN 'Moderate'
        WHEN 3 THEN 'Heavy'
        WHEN 4 THEN 'Very Heavy'
        WHEN 5 THEN 'Extreme'
    END AS CongestionStatus
FROM HourlyStatistics
ORDER BY HourTimestamp DESC;
GO

-- =============================================
-- 7. TODAY'S STATISTICS
-- =============================================
SELECT 
    DATEPART(HOUR, HourTimestamp) AS Hour,
    TotalVehicles,
    CarCount,
    MotorcycleCount,
    BusCount,
    BicycleCount
FROM HourlyStatistics
WHERE CAST(HourTimestamp AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY HourTimestamp;
GO

-- =============================================
-- 8. PEAK TRAFFIC HOURS (Top 10)
-- =============================================
SELECT TOP 10
    HourTimestamp,
    TotalVehicles,
    CASE CongestionLevel
        WHEN 0 THEN 'No Congestion'
        WHEN 1 THEN 'Light'
        WHEN 2 THEN 'Moderate'
        WHEN 3 THEN 'Heavy'
        WHEN 4 THEN 'Very Heavy'
        WHEN 5 THEN 'Extreme'
    END AS CongestionStatus
FROM HourlyStatistics
ORDER BY TotalVehicles DESC;
GO

-- =============================================
-- 9. SESSION PERFORMANCE STATS
-- =============================================
SELECT 
    COUNT(*) AS TotalSessions,
    SUM(TotalVehicles) AS TotalVehiclesDetected,
    AVG(AverageFPS) AS AvgFPS,
    AVG(ProcessingTime) AS AvgProcessingTime_Seconds,
    MIN(StartTime) AS FirstSession,
    MAX(StartTime) AS LastSession
FROM TrafficSessions;
GO

-- =============================================
-- 10. DETECTIONS BY DATE
-- =============================================
SELECT 
    CAST(DetectedTime AS DATE) AS Date,
    COUNT(*) AS TotalDetections,
    COUNT(DISTINCT VehicleType) AS UniqueVehicleTypes
FROM VehicleDetections
GROUP BY CAST(DetectedTime AS DATE)
ORDER BY Date DESC;
GO

-- =============================================
-- 11. CONFIDENCE DISTRIBUTION
-- =============================================
SELECT 
    CASE 
        WHEN Confidence >= 0.9 THEN '90-100%'
        WHEN Confidence >= 0.8 THEN '80-89%'
        WHEN Confidence >= 0.7 THEN '70-79%'
        WHEN Confidence >= 0.6 THEN '60-69%'
        ELSE 'Below 60%'
    END AS ConfidenceRange,
    COUNT(*) AS DetectionCount
FROM VehicleDetections
GROUP BY 
    CASE 
        WHEN Confidence >= 0.9 THEN '90-100%'
        WHEN Confidence >= 0.8 THEN '80-89%'
        WHEN Confidence >= 0.7 THEN '70-79%'
        WHEN Confidence >= 0.6 THEN '60-69%'
        ELSE 'Below 60%'
    END
ORDER BY ConfidenceRange DESC;
GO

-- =============================================
-- 12. DELETE OLD SESSIONS (older than 30 days)
-- =============================================
-- CAUTION: This will delete sessions and all related detections
/*
DELETE FROM TrafficSessions
WHERE StartTime < DATEADD(DAY, -30, GETDATE());
GO
*/

-- =============================================
-- 13. TABLE SIZES
-- =============================================
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCount,
    CAST(ROUND(((SUM(a.total_pages) * 8) / 1024.00), 2) AS DECIMAL(10,2)) AS TotalSpaceMB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.NAME IN ('TrafficSessions', 'VehicleDetections', 'HourlyStatistics')
    AND t.is_ms_shipped = 0
    AND i.OBJECT_ID > 255
GROUP BY t.Name, p.Rows
ORDER BY TotalSpaceMB DESC;
GO
