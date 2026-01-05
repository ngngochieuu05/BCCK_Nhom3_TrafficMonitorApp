-- =============================================
-- Traffic Monitor Database Setup Script
-- SQL Server LocalDB
-- =============================================

USE master;
GO

-- Drop database if exists (for fresh install)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TrafficMonitorDb')
BEGIN
    ALTER DATABASE TrafficMonitorDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TrafficMonitorDb;
END
GO

-- Create database
CREATE DATABASE TrafficMonitorDb;
GO

USE TrafficMonitorDb;
GO

-- =============================================
-- Table: TrafficSessions
-- Lưu thông tin phiên giám sát
-- =============================================
CREATE TABLE TrafficSessions (
    SessionId INT IDENTITY(1,1) PRIMARY KEY,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NULL,
    SourceType NVARCHAR(50) NOT NULL,
    SourcePath NVARCHAR(500) NULL,
    ModelPath NVARCHAR(500) NULL,
    ConfidenceThreshold FLOAT NOT NULL,
    IouThreshold FLOAT NOT NULL,
    TotalVehicles INT NOT NULL DEFAULT 0,
    ProcessedFrames INT NOT NULL DEFAULT 0,
    ProcessingTime FLOAT NOT NULL DEFAULT 0,
    AverageFPS FLOAT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

-- Indexes for TrafficSessions
CREATE INDEX IX_TrafficSessions_StartTime ON TrafficSessions(StartTime);
CREATE INDEX IX_TrafficSessions_EndTime ON TrafficSessions(EndTime);
CREATE INDEX IX_TrafficSessions_SourceType ON TrafficSessions(SourceType);
GO

-- =============================================
-- Table: VehicleDetections
-- Lưu chi tiết phát hiện xe
-- =============================================
CREATE TABLE VehicleDetections (
    DetectionId INT IDENTITY(1,1) PRIMARY KEY,
    SessionId INT NOT NULL,
    DetectedTime DATETIME2 NOT NULL,
    TrackerId INT NOT NULL,
    VehicleType NVARCHAR(50) NOT NULL,
    Confidence FLOAT NOT NULL,
    PositionX INT NOT NULL,
    PositionY INT NOT NULL,
    Width INT NOT NULL,
    Height INT NOT NULL,
    FrameNumber INT NOT NULL,
    
    CONSTRAINT FK_VehicleDetections_Session 
        FOREIGN KEY (SessionId) 
        REFERENCES TrafficSessions(SessionId) 
        ON DELETE CASCADE
);
GO

-- Indexes for VehicleDetections
CREATE INDEX IX_VehicleDetections_SessionId ON VehicleDetections(SessionId);
CREATE INDEX IX_VehicleDetections_DetectedTime ON VehicleDetections(DetectedTime);
CREATE INDEX IX_VehicleDetections_VehicleType ON VehicleDetections(VehicleType);
CREATE INDEX IX_VehicleDetections_TrackerId ON VehicleDetections(TrackerId);
GO

-- =============================================
-- Table: HourlyStatistics
-- Thống kê theo giờ
-- =============================================
CREATE TABLE HourlyStatistics (
    StatId INT IDENTITY(1,1) PRIMARY KEY,
    HourTimestamp DATETIME2 NOT NULL,
    TotalVehicles INT NOT NULL DEFAULT 0,
    CarCount INT NOT NULL DEFAULT 0,
    MotorcycleCount INT NOT NULL DEFAULT 0,
    BusCount INT NOT NULL DEFAULT 0,
    BicycleCount INT NOT NULL DEFAULT 0,
    AverageSpeed FLOAT NOT NULL DEFAULT 0,
    CongestionLevel INT NOT NULL DEFAULT 0,
    
    CONSTRAINT UQ_HourlyStatistics_Timestamp UNIQUE (HourTimestamp)
);
GO

-- Index for HourlyStatistics
CREATE INDEX IX_HourlyStatistics_HourTimestamp ON HourlyStatistics(HourTimestamp);
GO

PRINT 'Database TrafficMonitorDb created successfully!';
PRINT 'Tables: TrafficSessions, VehicleDetections, HourlyStatistics';
GO
