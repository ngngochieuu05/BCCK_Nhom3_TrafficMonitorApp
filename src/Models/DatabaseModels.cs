using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficMonitorApp.Models
{
    /// <summary>
    /// Entity for storing traffic monitoring session information
    /// Bảng: PhienGiamSat
    /// </summary>
    [Table("PhienGiamSat")]
    public class TrafficSessionDb
    {
        [Key]
        [Column("MaPhien")]
        public int SessionId { get; set; }
        
        [Column("ThoiGianBatDau")]
        public DateTime StartTime { get; set; }
        
        [Column("ThoiGianKetThuc")]
        public DateTime? EndTime { get; set; }
        
        [Column("LoaiNguon")]
        public string SourceType { get; set; } = ""; // video, camera, image
        
        [Column("DuongDanNguon")]
        public string SourcePath { get; set; } = "";
        
        [Column("DuongDanMoHinh")]
        public string ModelPath { get; set; } = "";
        
        [Column("NguyenDoTinCay")]
        public double ConfidenceThreshold { get; set; }
        
        [Column("NguyenIoU")]
        public double IouThreshold { get; set; }
        
        [Column("TongSoXe")]
        public int TotalVehicles { get; set; }
        
        [Column("SoKhungHinhXuLy")]
        public int ProcessedFrames { get; set; }
        
        [Column("ThoiGianXuLy")]
        public double ProcessingTime { get; set; }
        
        [Column("TocDoTrungBinh")]
        public double AverageFPS { get; set; }
        
        // Navigation property
        public virtual ICollection<VehicleDetectionDb> Detections { get; set; } = new List<VehicleDetectionDb>();
    }
    
    /// <summary>
    /// Entity for storing individual vehicle detection records
    /// Bảng: PhatHienXe
    /// </summary>
    [Table("PhatHienXe")]
    public class VehicleDetectionDb
    {
        [Key]
        [Column("MaPhatHien")]
        public int DetectionId { get; set; }
        
        [ForeignKey("Session")]
        [Column("MaPhien")]
        public int SessionId { get; set; }
        
        [Column("ThoiGianPhatHien")]
        public DateTime DetectedTime { get; set; }
        
        [Column("MaTheoDoiXe")]
        public int TrackerId { get; set; }
        
        [Column("LoaiXe")]
        public string VehicleType { get; set; } = ""; // car, motorcycle, bus, bicycle
        
        [Column("DoTinCay")]
        public double Confidence { get; set; }
        
        [Column("ToaDoX")]
        public int PositionX { get; set; }
        
        [Column("ToaDoY")]
        public int PositionY { get; set; }
        
        [Column("ChieuRong")]
        public int Width { get; set; }
        
        [Column("ChieuCao")]
        public int Height { get; set; }
        
        [Column("SoKhungHinh")]
        public int FrameNumber { get; set; }
        
        // Navigation property
        public virtual TrafficSessionDb Session { get; set; } = null!;
    }
    
    /// <summary>
    /// Entity for storing hourly aggregated statistics
    /// Bảng: ThongKeTheoGio
    /// </summary>
    [Table("ThongKeTheoGio")]
    public class HourlyStatisticsDb
    {
        [Key]
        [Column("MaThongKe")]
        public int StatId { get; set; }
        
        [Column("ThoiGianGio")]
        public DateTime HourTimestamp { get; set; }
        
        [Column("TongSoXe")]
        public int TotalVehicles { get; set; }
        
        [Column("SoXeOto")]
        public int CarCount { get; set; }
        
        [Column("SoXeMay")]
        public int MotorcycleCount { get; set; }
        
        [Column("SoXeBuyt")]
        public int BusCount { get; set; }
        
        [Column("SoXeDap")]
        public int BicycleCount { get; set; }
        
        [Column("TocDoTrungBinh")]
        public double AverageSpeed { get; set; }
        
        [Column("MucDoTacNghen")]
        public int CongestionLevel { get; set; } // 0-5: 0=no congestion, 5=extreme
    }
}
