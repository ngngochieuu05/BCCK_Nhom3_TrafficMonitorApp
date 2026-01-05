using System;
using System.Collections.Generic;
using System.Linq;

namespace TrafficMonitorApp
{
    /// <summary>
    /// Advanced analytics for traffic monitoring system
    /// </summary>
    public class AdvancedStatistics
    {
        private readonly List<VehicleRecord> _records = new List<VehicleRecord>();
        private readonly Dictionary<string, List<int>> _hourlyCount = new Dictionary<string, List<int>>();
        private DateTime _sessionStart;

        public class VehicleRecord
        {
            public int TrackerId { get; set; }
            public string VehicleType { get; set; } = "";
            public DateTime FirstSeen { get; set; }
            public DateTime LastSeen { get; set; }
            public double EstimatedSpeed { get; set; }
            public int FrameCount { get; set; }
        }

        public class PeakHourInfo
        {
            public int Hour { get; set; }
            public int Count { get; set; }
            public string Period { get; set; } = "";
        }

        public class VehicleDistribution
        {
            public string VehicleType { get; set; } = "";
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        public AdvancedStatistics()
        {
            _sessionStart = DateTime.Now;
            InitializeHourlyTracking();
        }

        private void InitializeHourlyTracking()
        {
            for (int hour = 0; hour < 24; hour++)
            {
                _hourlyCount[hour.ToString("D2")] = new List<int>();
            }
        }

        public void RecordVehicle(int trackerId, string vehicleType, DateTime timestamp, int frameCount = 1)
        {
            var existing = _records.FirstOrDefault(r => r.TrackerId == trackerId);
            
            if (existing == null)
            {
                _records.Add(new VehicleRecord
                {
                    TrackerId = trackerId,
                    VehicleType = vehicleType,
                    FirstSeen = timestamp,
                    LastSeen = timestamp,
                    FrameCount = frameCount
                });

                // Track hourly count
                string hourKey = timestamp.Hour.ToString("D2");
                if (!_hourlyCount[hourKey].Contains(trackerId))
                {
                    _hourlyCount[hourKey].Add(trackerId);
                }
            }
            else
            {
                existing.LastSeen = timestamp;
                existing.FrameCount += frameCount;
            }
        }

        public List<PeakHourInfo> GetPeakHours(int topN = 5)
        {
            var peakHours = _hourlyCount
                .Select(kvp => new PeakHourInfo
                {
                    Hour = int.Parse(kvp.Key),
                    Count = kvp.Value.Count,
                    Period = GetPeriodName(int.Parse(kvp.Key))
                })
                .Where(p => p.Count > 0)
                .OrderByDescending(p => p.Count)
                .Take(topN)
                .ToList();

            return peakHours;
        }

        private string GetPeriodName(int hour)
        {
            if (hour >= 6 && hour < 12) return "Sang";
            if (hour >= 12 && hour < 18) return "Chieu";
            if (hour >= 18 && hour < 22) return "Toi";
            return "Dem";
        }

        public List<VehicleDistribution> GetVehicleDistribution()
        {
            var total = _records.Count;
            if (total == 0) return new List<VehicleDistribution>();

            var distribution = _records
                .GroupBy(r => r.VehicleType)
                .Select(g => new VehicleDistribution
                {
                    VehicleType = g.Key,
                    Count = g.Count(),
                    Percentage = (g.Count() * 100.0) / total
                })
                .OrderByDescending(d => d.Count)
                .ToList();

            return distribution;
        }

        public double GetAverageDwellTime()
        {
            if (_records.Count == 0) return 0;

            var avgSeconds = _records
                .Where(r => r.LastSeen > r.FirstSeen)
                .Select(r => (r.LastSeen - r.FirstSeen).TotalSeconds)
                .DefaultIfEmpty(0)
                .Average();

            return avgSeconds;
        }

        public int GetTotalVehicles()
        {
            return _records.Count;
        }

        public Dictionary<string, int> GetHourlyData()
        {
            return _hourlyCount.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Count
            );
        }

        public int GetVehicleCountByType(string vehicleType)
        {
            return _records.Count(r => r.VehicleType == vehicleType);
        }

        public double GetVehiclesPerMinute()
        {
            var elapsed = (DateTime.Now - _sessionStart).TotalMinutes;
            return elapsed > 0 ? _records.Count / elapsed : 0;
        }

        public string GetBusiestPeriod()
        {
            var peaks = GetPeakHours(1);
            return peaks.FirstOrDefault()?.Period ?? "N/A";
        }

        public double GetCongestionLevel()
        {
            var vehiclesPerMin = GetVehiclesPerMinute();
            
            // Assume congestion thresholds
            if (vehiclesPerMin < 5) return 0; // Free flow
            if (vehiclesPerMin < 10) return 25; // Light
            if (vehiclesPerMin < 20) return 50; // Moderate
            if (vehiclesPerMin < 30) return 75; // Heavy
            return 100; // Severe congestion
        }

        public void Reset()
        {
            _records.Clear();
            InitializeHourlyTracking();
            _sessionStart = DateTime.Now;
        }
    }
}
