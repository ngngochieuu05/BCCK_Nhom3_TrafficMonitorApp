using System;
using System.Collections.Generic;
using System.Linq;

namespace TrafficMonitorApp
{
    /// <summary>
    /// Quản lý bãi xe với tracking ID - ghi nhận xe vào/ra và tạo báo cáo
    /// </summary>
    public class ParkingManager
    {
        private bool _isParkingMode = false;
        private DateTime _sessionStartTime;
        private readonly Dictionary<int, ParkingVehicle> _vehiclesInParking = new Dictionary<int, ParkingVehicle>();
        private readonly List<ParkingRecord> _parkingHistory = new List<ParkingRecord>();

        public class ParkingVehicle
        {
            public int TrackerId { get; set; }
            public string VehicleType { get; set; } = "";
            public DateTime EntryTime { get; set; }
            public DateTime? ExitTime { get; set; }
            public string Status { get; set; } = "InParking"; // InParking, Exited
            public int EntryFrameNumber { get; set; }
            public int? ExitFrameNumber { get; set; }
        }

        public class ParkingRecord
        {
            public int TrackerId { get; set; }
            public string VehicleType { get; set; } = "";
            public DateTime EntryTime { get; set; }
            public DateTime? ExitTime { get; set; }
            public TimeSpan? Duration { get; set; }
            public string Status { get; set; } = "";
        }

        public class ParkingReport
        {
            public DateTime SessionStartTime { get; set; }
            public DateTime SessionEndTime { get; set; }
            public TimeSpan TotalDuration { get; set; }
            public int TotalVehiclesEntered { get; set; }
            public int TotalVehiclesExited { get; set; }
            public int VehiclesStillInParking { get; set; }
            public Dictionary<string, int> VehicleTypeCount { get; set; } = new Dictionary<string, int>();
            public List<ParkingRecord> DetailedRecords { get; set; } = new List<ParkingRecord>();
            public Dictionary<string, int> StillInParkingByType { get; set; } = new Dictionary<string, int>();
        }

        public bool IsParkingMode => _isParkingMode;
        public int CurrentVehicleCount => _vehiclesInParking.Count(v => v.Value.Status == "InParking");
        public int TotalEntered => _parkingHistory.Count;

        public void StartParkingSession()
        {
            _isParkingMode = true;
            _sessionStartTime = DateTime.Now;
            _vehiclesInParking.Clear();
            _parkingHistory.Clear();
            Console.WriteLine($"[ParkingManager] ✓ Parking session started at {_sessionStartTime:yyyy-MM-dd HH:mm:ss}");
        }

        public void StopParkingSession()
        {
            _isParkingMode = false;
            Console.WriteLine($"[ParkingManager] ✓ Parking session stopped. Total vehicles: {_parkingHistory.Count}, Still in parking: {CurrentVehicleCount}");
        }

        /// <summary>
        /// Ghi nhận xe vào bãi (khi phát hiện xe mới)
        /// </summary>
        public void RecordVehicleEntry(int trackerId, string vehicleType, int frameNumber)
        {
            if (!_isParkingMode) return;

            // Nếu xe đã có trong hệ thống, bỏ qua
            if (_vehiclesInParking.ContainsKey(trackerId))
            {
                return;
            }

            var entryTime = DateTime.Now;
            var vehicle = new ParkingVehicle
            {
                TrackerId = trackerId,
                VehicleType = vehicleType,
                EntryTime = entryTime,
                Status = "InParking",
                EntryFrameNumber = frameNumber
            };

            _vehiclesInParking[trackerId] = vehicle;

            var record = new ParkingRecord
            {
                TrackerId = trackerId,
                VehicleType = vehicleType,
                EntryTime = entryTime,
                Status = "InParking"
            };
            _parkingHistory.Add(record);

            Console.WriteLine($"[ParkingManager] 🚗 Vehicle ENTERED: ID={trackerId}, Type={vehicleType}, Time={entryTime:HH:mm:ss}, Total in parking: {CurrentVehicleCount}");
        }

        /// <summary>
        /// Ghi nhận xe rời bãi (khi xe vượt qua đường counting line theo hướng ra)
        /// </summary>
        public void RecordVehicleExit(int trackerId, int frameNumber)
        {
            if (!_isParkingMode) return;

            if (!_vehiclesInParking.ContainsKey(trackerId))
            {
                // Xe không có trong danh sách (có thể đã ra trước đó hoặc vào trước khi bật parking mode)
                return;
            }

            var vehicle = _vehiclesInParking[trackerId];
            if (vehicle.Status == "Exited")
            {
                // Đã ghi nhận rời bãi rồi
                return;
            }

            var exitTime = DateTime.Now;
            vehicle.ExitTime = exitTime;
            vehicle.Status = "Exited";
            vehicle.ExitFrameNumber = frameNumber;

            // Cập nhật record trong history
            var record = _parkingHistory.FirstOrDefault(r => r.TrackerId == trackerId && r.ExitTime == null);
            if (record != null)
            {
                record.ExitTime = exitTime;
                record.Status = "Exited";
                record.Duration = exitTime - record.EntryTime;
            }

            Console.WriteLine($"[ParkingManager] 🚙 Vehicle EXITED: ID={trackerId}, Type={vehicle.VehicleType}, Duration={record?.Duration?.TotalMinutes:F1} minutes, Total in parking: {CurrentVehicleCount}");
        }

        /// <summary>
        /// Tạo báo cáo chi tiết về bãi xe
        /// </summary>
        public ParkingReport GenerateReport()
        {
            var endTime = DateTime.Now;
            var report = new ParkingReport
            {
                SessionStartTime = _sessionStartTime,
                SessionEndTime = endTime,
                TotalDuration = endTime - _sessionStartTime,
                TotalVehiclesEntered = _parkingHistory.Count,
                TotalVehiclesExited = _parkingHistory.Count(r => r.ExitTime != null),
                VehiclesStillInParking = CurrentVehicleCount,
                DetailedRecords = new List<ParkingRecord>(_parkingHistory)
            };

            // Thống kê theo loại xe
            var typeGroups = _parkingHistory.GroupBy(r => r.VehicleType);
            foreach (var group in typeGroups)
            {
                report.VehicleTypeCount[group.Key] = group.Count();
            }

            // Thống kê xe còn trong bãi theo loại
            var stillInParking = _vehiclesInParking.Values.Where(v => v.Status == "InParking");
            var stillGroups = stillInParking.GroupBy(v => v.VehicleType);
            foreach (var group in stillGroups)
            {
                report.StillInParkingByType[group.Key] = group.Count();
            }

            return report;
        }

        /// <summary>
        /// Lấy danh sách xe hiện đang trong bãi
        /// </summary>
        public List<ParkingVehicle> GetCurrentVehiclesInParking()
        {
            return _vehiclesInParking.Values
                .Where(v => v.Status == "InParking")
                .OrderBy(v => v.EntryTime)
                .ToList();
        }

        /// <summary>
        /// Lấy lịch sử đầy đủ
        /// </summary>
        public List<ParkingRecord> GetFullHistory()
        {
            return new List<ParkingRecord>(_parkingHistory);
        }

        /// <summary>
        /// Reset toàn bộ dữ liệu parking
        /// </summary>
        public void Reset()
        {
            _vehiclesInParking.Clear();
            _parkingHistory.Clear();
            _isParkingMode = false;
            Console.WriteLine("[ParkingManager] ✓ Reset completed");
        }
        
        /// <summary>
        /// Kiểm tra xe có trong bãi không
        /// </summary>
        public bool IsVehicleInParking(int trackerId)
        {
            return _vehiclesInParking.ContainsKey(trackerId) && 
                   _vehiclesInParking[trackerId].Status == "InParking";
        }
    }
}
